using System;
using System.IO;
using ImageMagick;

namespace CirnoLib.Format.BLPLib.Legacy
{
    public sealed class JpegSurface : Surface
	{
		private byte[] data;

		public JpegSurface(byte[] rawData, int width, int height, bool shareBuffer = false)
			: base(width, height, 0, false)
		{
			if (rawData == null) throw new ArgumentNullException("rawData");

			data = shareBuffer ? rawData : rawData.Clone() as byte[];
		}

		public override bool CanLock { get { return false; } }

		protected override IntPtr LockInternal(out int stride) { throw new NotSupportedException(); }

		protected override void UnlockInternal() { throw new NotSupportedException(); }

		protected override void CopyToArgbInternal(SurfaceData surfaceData) { throw new NotSupportedException(); }

        public override byte[] ToArray()
        {
            using (MagickImage image = new MagickImage(data))
            {
                if (image.ColorSpace != ColorSpace.sRGB)
                    image.ColorSpace = ColorSpace.sRGB;
                MagickColorMatrix matrix = new MagickColorMatrix(3);
                matrix.SetRow(0, 0, 0, 1);
                matrix.SetRow(1, 0, 1, 0);
                matrix.SetRow(2, 1, 0, 0);
                image.ColorMatrix(matrix);
                return image.ToByteArray();
            }
        }

		/// <summary>Creates a stream for accessing the surface data.</summary>
		/// <remarks>The returned stream can be used for reading the surface data.</remarks>
		/// <returns>A stream which can be used to access the surface data.</returns>
		public override Stream CreateStream() { return new MemoryStream(data, false); }

		public override object Clone()
		{
			var clone = base.Clone() as JpegSurface;

			clone.data = data.Clone() as byte[];

			return clone;
		}
	}
}
