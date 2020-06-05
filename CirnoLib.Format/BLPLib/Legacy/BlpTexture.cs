using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CirnoLib.Format.BLPLib.Legacy
{
	/// <summary>Encapsulates bitmap data corresponding to a BLP image.</summary>
	public sealed class BlpTexture : IDisposable
	{
		#region Surface Class

		public sealed class Surface : Legacy.Surface.Wrapper
		{
            internal Surface(BlpTexture texture, Legacy.Surface surface) : base(surface) => this.Texture = texture;

            public BlpTexture Texture { get; }
        }

		#endregion

		#region BLP Header Structures

		[StructLayout(LayoutKind.Sequential)]
		private unsafe struct Blp1Header
		{
			public int Compression; // 0 for JPEG, 1 for Palette
			public int Alpha; // 8 for 8 bit alpha, 0 for no alpha
			public int Width; // Width of first mip map
			public int Height; // Height of first mip map
			public int PictureType; // Flag for alpha channel and team colors
			public int PictureSubType; // Always 1 ?
			public fixed int Offsets[16]; // Mip map offsets
			public fixed int Lengths[16]; // Mip map data lengths
		}

		#endregion

		#region SurfaceCollection Class

		public sealed class SurfaceCollection : IEnumerable<Surface>
		{
			private BlpTexture owner;

			internal  SurfaceCollection(BlpTexture owner)
			{
				this.owner = owner;
			}

			public Surface this[int index]
			{
				get
				{
					if (index < 0 || index >= owner.mipmaps.Length) throw new ArgumentOutOfRangeException("index");
					return owner.mipmaps[index];
				}
			}

			public int Count { get { return owner.mipmaps.Length; } }

			public IEnumerator<Surface> GetEnumerator() { return (owner.mipmaps as IEnumerable<Surface>).GetEnumerator(); }

			IEnumerator IEnumerable.GetEnumerator() { return owner.mipmaps.GetEnumerator(); }
		}

		#endregion

		#region AlphaOperation Enum

		private enum AlphaOperation
		{
			None,
			SetAlpha,
			InvertAlpha
		}

        #endregion
        private Surface[] mipmaps;
		//private uint version;

        public BlpTexture(byte[] bytes, bool loadMipMaps = true) : this(new MemoryStream(bytes), loadMipMaps) { }

		public BlpTexture(Stream stream, bool loadMipMaps = true)
		{
			long position = 0;

			if (stream.CanSeek) position = stream.Position;
			LoadFromStream(stream, loadMipMaps);
			if (stream.CanSeek) stream.Seek(position, SeekOrigin.Begin);

			Mipmaps = new SurfaceCollection(this);
		}

		public BlpTexture(string filename, bool loadMipMaps = true)
		{
			using (var stream = File.Open(filename, FileMode.Open))
				LoadFromStream(stream, loadMipMaps);

			Mipmaps = new SurfaceCollection(this);
		}

        public int Width { get; private set; }

        public int Height { get; private set; }

        // This method reads the file signature and, if valid,
        // dispatches the loading process to version-corresponding functions
        // NB: Actually handles BLP1 and BLP2
        // NB: BLP1 is JPEG-based and BLP2 is DXTC-based
        private void LoadFromStream(Stream stream, bool loadMipMaps)
		{
			BinaryReader reader = new BinaryReader(stream);
			uint signature;
			int startOffset;
			int mipMapCount;

			mipMapCount = loadMipMaps ? 16 : 1;

			// Begin the decoding
			startOffset = (int)stream.Position;
			signature = reader.ReadUInt32();
			if ((signature & 0xFFFFFF) != 0x504C42) throw new InvalidDataException();
			else if (signature >> 24 == 0x31) LoadBlp1(reader, startOffset, mipMapCount); // Use BLP1 reading method
			else throw new InvalidDataException();
		}

		// Incorrect rendering due to Blizzard's use of Intel's Jpeg Library (CMYK vs YMCK…)
		// We need to swap Red and Blue channels to get a correct image
		private unsafe void LoadBlp1(BinaryReader reader, int startOffset, int mipMapCount)
		{
			Stream stream = reader.BaseStream;
			Blp1Header header;

			header = ReadBlp1Header(reader, startOffset);

			Width = header.Width;
			Height = header.Height;

			if (header.Compression == 0) LoadJpegMipmaps(reader, startOffset, header.Width, header.Height, header.Offsets, header.Lengths, mipMapCount);
			else if (header.Compression == 1) LoadPalettedMipmaps(reader, startOffset, header.Width, header.Height, header.Offsets, header.Lengths, mipMapCount, header.PictureType == 5, header.PictureType == 5 ? (byte)0 : (byte)8, true);
			else mipmaps = new Surface[0];
		}

		private static unsafe Blp1Header ReadBlp1Header(BinaryReader reader, int startOffset)
		{
			Blp1Header header;

			header.Compression = reader.ReadInt32();
			header.Alpha = reader.ReadInt32();
			header.Width = reader.ReadInt32();
			header.Height = reader.ReadInt32();
			header.PictureType = reader.ReadInt32();
			header.PictureSubType = reader.ReadInt32();

			// Read mipmap info
			for (int i = 0; i < 16; i++) header.Offsets[i] = reader.ReadInt32();
			for (int i = 0; i < 16; i++) header.Lengths[i] = reader.ReadInt32();

			return header;
		}
		private int LowerMipmapDimension(int dimension) { return dimension > 1 ? dimension >> 1 : 1; }

		/// <summary>Reads a 256 color palette from a stream</summary>
		/// <param name="reader">The BinaryReader used for reading data in the stream</param>
		/// <param name="alphaOperation">The operation to apply on each palette entry's alpha component</param>
		/// <returns>An array of bytes containing the palette entries</returns>
		private ArgbColor[] ReadPalette(BinaryReader reader, AlphaOperation alphaOperation)
		{
			var palette = new ArgbColor[0x100];

			for (int i = 0; i < palette.Length; i++)
			{
				byte b = reader.ReadByte();
				byte g = reader.ReadByte();
				byte r = reader.ReadByte();
				byte a = reader.ReadByte();

				palette[i] = new ArgbColor(r, g, b, alphaOperation != AlphaOperation.None ? alphaOperation == AlphaOperation.SetAlpha ? (byte)255 : (byte)~a : a);
			}

			return palette;
		}

		private unsafe void LoadPalettedMipmaps(BinaryReader reader, int startOffset, int width, int height, int* offsets, int* lengths, int mipMapCount, bool opaque, byte separateAlphaBitCount, bool invertAlpha)
		{
			var mipmapList = new List<Surface>(16);
			int mipmapWidth = width;
			int mipmapHeight = height;

			var palette = ReadPalette(reader, !opaque ? invertAlpha ? AlphaOperation.InvertAlpha : AlphaOperation.None : AlphaOperation.SetAlpha);

			for (int i = 0; i < mipMapCount && offsets[i] != 0 && lengths[i] != 0; i++, mipmapWidth = LowerMipmapDimension(mipmapWidth), mipmapHeight = LowerMipmapDimension(mipmapHeight))
			{
				// Create a new buffer for the current mipmap
				var mipmap = new byte[lengths[i]];
				// Seek to the position of the current mipmap
				reader.BaseStream.Seek(startOffset + offsets[i], SeekOrigin.Begin);
				// Read the data into the buffer
				reader.Read(mipmap, 0, mipmap.Length);
				// Add the mipmap to the list
				mipmapList.Add(new Surface(this, new PaletteSurface(mipmap, palette, mipmapWidth, mipmapHeight, opaque, separateAlphaBitCount, false, true, true)));
			}

			this.mipmaps = mipmapList.ToArray();
		}

		private unsafe void LoadJpegMipmaps(BinaryReader reader, int startOffset, int width, int height, int* offsets, int* lengths, int mipMapCount)
		{
			// Read the JPEG header length from the current position in the stream and allocate a buffer…
			var jpegHeader = new byte[reader.ReadInt32()];
			// Read the JPEG header into the buffer.
			reader.Read(jpegHeader, 0, jpegHeader.Length);

			var mipmapList = new List<Surface>(16);
			int mipmapWidth = width;
			int mipmapHeight = height;

			// Read individual mipmaps
			for (int i = 0; i < mipMapCount && offsets[i] != 0 && lengths[i] != 0; i++, mipmapWidth = LowerMipmapDimension(mipmapWidth), mipmapHeight = LowerMipmapDimension(mipmapHeight))
			{
				// Create a new buffer for the current mipmap
				var mipmap = new byte[jpegHeader.Length + lengths[i]];
				// Copy the JPEG header at the beginning of the buffer
				Buffer.BlockCopy(jpegHeader, 0, mipmap, 0, jpegHeader.Length);
				// Seek to the position of the current mipmap
				reader.BaseStream.Seek(startOffset + offsets[i], SeekOrigin.Begin);
				// Read the data into the buffer
				reader.Read(mipmap, jpegHeader.Length, lengths[i]);
				// Add the mipmap to the list
				mipmapList.Add(new Surface(this, new JpegSurface(mipmap, mipmapWidth, mipmapHeight, true)));
			}

			mipmaps = mipmapList.ToArray();
		}

        public SurfaceCollection Mipmaps { get; }

        public Surface FirstMipmap { get { return mipmaps[0]; } }

		public void Dispose()
		{
			foreach (Surface mipmap in mipmaps)
				if (mipmap != null)
					mipmap.Dispose();
			mipmaps = null;
		}
	}
}
