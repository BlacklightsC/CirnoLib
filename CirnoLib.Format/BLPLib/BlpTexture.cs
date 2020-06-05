using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using ImageMagick;
using ImageMagick.ImageOptimizers;

namespace CirnoLib.Format.BLPLib
{
    public sealed class BlpTexture : IArrayable, IStreamable
    {
        /// <summary>BLP1를 나타내는 매직넘버입니다.</summary>
        public const int Signature = 0x31504C42;
        public CompressionType Compression { get; private set; }
        /// <summary>Alpha 값이 사용하는 비트 수 입니다. 일반적으로 0 또는 8을 사용합니다.</summary>
        public int Alpha { get; private set; }
        /// <summary>첫번째 밉맵의 가로 길이입니다.</summary>
        public int Width { get; private set; }
        /// <summary>첫번째 밉맵의 세로 길이입니다.</summary>
        public int Height { get; private set; }
        private int PictureType { get; set; }
        private bool HasMipmaps { get; set; } = true;

        public BlpMipmap[] Mipmaps { get; private set; }

        public BlpTexture() { }
        public BlpTexture(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                InitByStream(stream);
            }
        }
        public BlpTexture(Stream stream) => InitByStream(stream);
        public BlpTexture(byte[] data)
        {
            using (ByteStream bs = new ByteStream(data))
            {
                if (bs.ReadInt32() != Signature) throw new NotSupportedException("Signature 값이 일치하지 않거나, 지원하지 않는 BLP 버전입니다.");
                Compression = (CompressionType)bs.ReadInt32();
                Alpha = bs.ReadInt32();
                Width = bs.ReadInt32();
                Height = bs.ReadInt32();
                PictureType = bs.ReadInt32();
                HasMipmaps = bs.ReadInt32() == 1;
                int[] Offset = new int[16];
                int[] Length = new int[16];
                for (int i = 0; i < 16; i++) Offset[i] = bs.ReadInt32();
                for (int i = 0; i < 16; i++) Length[i] = bs.ReadInt32();
                int LastWidth = -1, LastHeight = -1;
                Mipmaps = new BlpMipmap[16];
                switch (Compression)
                {
                    case CompressionType.Jpeg:
                        int HeadLength = bs.ReadInt32();
                        if (HeadLength != 0) _HeadData = bs.ReadBytes(HeadLength);
                        for (int i = 0; i < 16; i++)
                        {
                            if (Offset[i] == 0 || Length[i] == 0) break;
                            if (i > 0 && Offset[i] == Offset[i - 1]
                                      && Length[i] == Length[i - 1])
                                Mipmaps[i] = new BlpMipmap(this);
                            else
                            {
                                BlpJpegMipmap mipmap = new BlpJpegMipmap(this, bs.ReadBytes(Length[i], Offset[i]));
                                MagickImageInfo Image = new MagickImageInfo(mipmap.RawData);
                                if (i > 0 && Image.Width == LastWidth && Image.Height == LastHeight)
                                    Mipmaps[i] = new BlpMipmap(this);
                                else Mipmaps[i] = mipmap;
                                LastWidth = Image.Width;
                                LastHeight = Image.Height;
                            }
                        }
                        if (_HeadData == null) MergeHeadData();
                        break;

                    case CompressionType.Palette:
                        ColorMap = new Color[256];
                        for (int i = 0; i < 256; i++)
                        {
                            byte blue = bs.ReadByte();
                            byte green = bs.ReadByte();
                            byte red = bs.ReadByte();
                            byte alpha = bs.ReadByte();
                            ColorMap[i] = Color.FromArgb(red, green, blue);
                        }
                        double width = Width * 2, height = Height * 2;
                        for (int i = 0; i < 16; i++)
                        {
                            if (Offset[i] == 0 || Length[i] == 0) break;
                            if (i > 0 && Offset[i] == Offset[i - 1]
                                      && Length[i] == Length[i - 1])
                                Mipmaps[i] = new BlpMipmap(this);
                            else if ((width /= 2) > 1 && (height /= 2) > 1)
                            {
                                int RoundedWidth = (int)width;
                                int RoundedHeight = (int)height;
                                BlpPalettedMipmap mipmap = new BlpPalettedMipmap(this, RoundedWidth, RoundedHeight);
                                byte[] block = bs.ReadBytes(Length[i], Offset[i]);
                                mipmap._indexList = block.SubArray(0, RoundedWidth * RoundedHeight);
                                if (Alpha > 0 && PictureType != 5)
                                    mipmap._alphaList = block.SubArray(RoundedWidth * RoundedHeight);
                                Mipmaps[i] = mipmap;
                            }
                        }

                        break;
                }
                bs.Close();
            }
        }

        private void InitByStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            if (stream.ReadInt32() != Signature) throw new NotSupportedException("Signature 값이 일치하지 않거나, 지원하지 않는 BLP 버전입니다.");
            Compression = (CompressionType)stream.ReadInt32();
            Alpha = stream.ReadInt32();
            Width = stream.ReadInt32();
            Height = stream.ReadInt32();
            PictureType = stream.ReadInt32();
            HasMipmaps = stream.ReadInt32() == 1;
            int[] Offset = new int[16];
            int[] Length = new int[16];
            for (int i = 0; i < 16; i++) Offset[i] = stream.ReadInt32();
            for (int i = 0; i < 16; i++) Length[i] = stream.ReadInt32();
            int LastWidth = -1, LastHeight = -1;
            Mipmaps = new BlpMipmap[16];
            switch (Compression)
            {
                case CompressionType.Jpeg:
                    int HeadLength = stream.ReadInt32();
                    if (HeadLength != 0) _HeadData = stream.ReadBytes(HeadLength);
                    for (int i = 0; i < 16; i++)
                    {
                        if (Offset[i] == 0 || Length[i] == 0) break;
                        if (i > 0 && Offset[i] == Offset[i - 1]
                                  && Length[i] == Length[i - 1])
                            Mipmaps[i] = new BlpMipmap(this);
                        else
                        {
                            BlpJpegMipmap mipmap = new BlpJpegMipmap(this, stream.ReadBytes(Length[i], Offset[i]));
                            MagickImageInfo Image = new MagickImageInfo(mipmap.RawData);
                            if (i > 0 && Image.Width == LastWidth && Image.Height == LastHeight)
                                Mipmaps[i] = new BlpMipmap(this);
                            else Mipmaps[i] = mipmap;
                            LastWidth = Image.Width;
                            LastHeight = Image.Height;
                        }
                    }
                    if (_HeadData == null) MergeHeadData();
                    break;

                case CompressionType.Palette:
                    ColorMap = new Color[256];
                    for (int i = 0; i < 256; i++)
                    {
                        byte blue = (byte)stream.ReadByte();
                        byte green = (byte)stream.ReadByte();
                        byte red = (byte)stream.ReadByte();
                        byte alpha = (byte)stream.ReadByte();
                        ColorMap[i] = Color.FromArgb(red, green, blue);
                    }
                    double width = Width * 2, height = Height * 2;
                    for (int i = 0; i < 16; i++)
                    {
                        if (Offset[i] == 0 || Length[i] == 0) break;
                        if (i > 0 && Offset[i] == Offset[i - 1]
                                  && Length[i] == Length[i - 1])
                            Mipmaps[i] = new BlpMipmap(this);
                        else if ((width /= 2) > 1 && (height /= 2) > 1)
                        {
                            int RoundedWidth = (int)width;
                            int RoundedHeight = (int)height;
                            BlpPalettedMipmap mipmap = new BlpPalettedMipmap(this, RoundedWidth, RoundedHeight);
                            byte[] block = stream.ReadBytes(Length[i], Offset[i]);
                            mipmap._indexList = block.SubArray(0, RoundedWidth * RoundedHeight);
                            if (Alpha > 0 && PictureType != 5)
                                mipmap._alphaList = block.SubArray(RoundedWidth * RoundedHeight);
                            Mipmaps[i] = mipmap;
                        }
                    }

                    break;
            }
        }

        #region [    Jpeg Compression    ]
        internal byte[] _HeadData { get; private set; } // Jpeg Define Data

        public void SetJpegImage(string filePath, int quality = 75, int mipmapCount = 8, bool mergeHeader = false)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                SetJpegImage(stream, quality, mipmapCount, mergeHeader);
            }
        }
        public void SetJpegImage(Stream stream, int quality = 75, int mipmapCount = 8, bool mergeHeader = false)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (MagickImage image = new MagickImage(stream))
            {
                SetJpegImage(image, quality, mipmapCount, mergeHeader);
            }
        }
        public void SetJpegImage(byte[] data, int quality = 75, int mipmapCount = 8, bool mergeHeader = false)
        {
            using (MagickImage image = new MagickImage(data))
            {
                SetJpegImage(image, quality, mipmapCount, mergeHeader);
            }
        }
        private void SetJpegImage(IMagickImage image, int quality, int mipmapCount, bool mergeHeader)
        {
            if (quality < 20) quality = 20;
            else if (quality > 90) quality = 90;
            Compression = CompressionType.Jpeg;
            Alpha = 0;
            PictureType = 5;
            _HeadData = null;
            ColorMap = null;
            image.Strip();
            image.ColorType = ColorType.TrueColor;
            RebuildToCMYK(image);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 100;
            JpegOptimizer opt = new JpegOptimizer() { OptimalCompression = true, Progressive = true };
            double width = 0, height = 0;
            int index = 0;
            Mipmaps = new BlpMipmap[16];
            do
            {
                if (mipmapCount <= index) Mipmaps[index++] = new BlpMipmap(this);
                else
                {
                    using (IMagickImage cloneImage = image.Clone())
                    {
                        cloneImage.ColormapSize = -1;
                        if (width == 0 && height == 0)
                        {
                            width = Width = cloneImage.BaseWidth;
                            height = Height = cloneImage.BaseHeight;
                        }
                        else cloneImage.Scale((int)width, (int)height);
                        using (ByteStream bs = new ByteStream(cloneImage.ToByteArray()))
                        {
                            //opt.LosslessCompress(bs);
                            opt.Compress(bs, quality);
                            Mipmaps[index++] = new BlpJpegMipmap(this, bs.ToArray());
                        }
                    }
                }
            } while ((width /= 2) > 1 && (height /= 2) > 1);
            if (index < 8) Mipmaps[index++] = new BlpMipmap(this);
            if (mergeHeader) MergeHeadData();
        }

        private IMagickImage SeparateImageChannel(IMagickImage image, Channels channel)
            => image.Clone().Separate(channel).ElementAt(0);

        private void RebuildToCMYK(IMagickImage image)
        {
            using (IMagickImage cloneImage = image.Clone())
            {
                cloneImage.ColorSpace = ColorSpace.CMY;
                image.Colorize(MagickColor.FromRgb(255, 255, 255), new Percentage(100));
                image.ColorSpace = ColorSpace.CMYK;
                image.Composite(SeparateImageChannel(cloneImage, Channels.Yellow), CompositeOperator.CopyCyan);
                image.Composite(SeparateImageChannel(cloneImage, Channels.Magenta), CompositeOperator.CopyMagenta);
                image.Composite(SeparateImageChannel(cloneImage, Channels.Cyan), CompositeOperator.CopyYellow);
            }
        }
        public void MergeHeadData()
        {
            int index = -1;
            byte[] mainBuffer = null;
            foreach (BlpMipmap item in Mipmaps)
            {
                if (!(item is BlpJpegMipmap jpeg)) break;
                byte[] buffer = jpeg.RawData;
                if (mainBuffer == null)
                {
                    index = buffer.Length < 624 ? buffer.Length : 624;
                    mainBuffer = buffer;
                }
                else for (int i = 0; i < buffer.Length && i < index && i < 624; i++)
                        if (buffer[i] != mainBuffer[i])
                        {
                            index = i;
                            break;
                        }
            }

            foreach (BlpMipmap item in Mipmaps)
            {
                if (!(item is BlpJpegMipmap jpeg)) break;
                jpeg._data = jpeg.RawData.SubArray(index);
            }

            _HeadData = mainBuffer.SubArray(0, index);
        }
        #endregion

        #region [    Paletted Uncompresion    ]
        public Color[] ColorMap { get; private set; } // BGRA[256]

        public void SetPaletteImage(string filePath, int mipmapCount = 8)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                SetPaletteImage(stream, mipmapCount);
            }
        }
        public void SetPaletteImage(Stream stream, int mipmapCount = 8)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (MagickImage image = new MagickImage(stream))
            {
                SetPaletteImage(image, mipmapCount);
            }
        }
        public void SetPaletteImage(byte[] data, int mipmapCount = 8)
        {
            using (MagickImage image = new MagickImage(data))
            {
                SetPaletteImage(image, mipmapCount);
            }
        }
        private void SetPaletteImage(IMagickImage image, int mipmapCount)
        {
            Compression = CompressionType.Palette;
            _HeadData = null;

            image.Strip();
            QuantizeSettings qSet = new QuantizeSettings { Colors = 256, ColorSpace = ColorSpace.sRGB, DitherMethod = DitherMethod.No };
            MagickImageCollection images = new MagickImageCollection();

            Alpha = 0;
            PictureType = 5;
            ColorMap = new Color[256];

            Mipmaps = new BlpMipmap[16];
            double width = 0, height = 0;
            int index = 0;
            do
            {
                if (mipmapCount <= index) Mipmaps[index++] = new BlpMipmap(this);
                else
                {
                    IMagickImage mipmap = image.Clone();
                    mipmap.ColormapSize = -1;
                    if (width == 0 && height == 0)
                    {
                        width = Width = image.BaseWidth;
                        height = Height = image.BaseHeight;
                    }
                    else mipmap.Scale((int)width, (int)height);
                    mipmap.ColormapSize = -1;
                    mipmap.Quantize(qSet);
                    images.Add(mipmap);
                }
            } while ((width /= 2) > 1 && (height /= 2) > 1);

            List<MagickColor> colorList = new List<MagickColor>();
            foreach (var img in images)
            {
                for (int i = 0; i < img.ColormapSize; i++)
                {
                    MagickColor color = img.GetColormap(i);
                    if (colorList.Find(item => item == color) == null)
                        colorList.Add(color);
                    if (colorList.Count > 256)
                        goto ColorMapCompress;
                }
            }
            colorList.Sort();
            foreach (var img in images)
                img.Map(colorList);
            for (int i = 0; i < 256; i++)
                ColorMap[i] = i < colorList.Count ? colorList[i].ToColor() : Color.Black;
            goto ColorMapped;

            ColorMapCompress:
            int baseMipmapIndex = images.Count - 1;
            for (int i = images.Count - 1; i >= 0; i--)
                if (images[i].ColormapSize >= images[baseMipmapIndex].ColormapSize)
                    baseMipmapIndex = i;
            for (int i = 0; i < images.Count; i++)
                if (i != baseMipmapIndex && FindUnMatchColorMap(images[i], images[baseMipmapIndex]))
                    images[i].Map(images[baseMipmapIndex]);

            for (int i = 0; i < 256; i++)
                ColorMap[i] = images[baseMipmapIndex].GetColormap(i)?.ToColor() ?? Color.Black;

            ColorMapped:
            foreach (var item in ColorMap)
                if (item.A != 255)
                {
                    Alpha = 8;
                    PictureType = 4;
                    break;
                }

            for (int i = 0; i < images.Count; i++)
            {
                using (IMagickImage currentImage = images[i])
                {
                    BlpPalettedMipmap mipmap = new BlpPalettedMipmap(this, currentImage.Width, currentImage.Height, false);
                    using (ByteStream idx = new ByteStream(mipmap.Width * mipmap.Height))
                    using (ByteStream alpha = new ByteStream((mipmap.Width * mipmap.Height * Alpha + 7) / 8))
                    {
                        for (int y = 0; y < currentImage.Height; y++)
                            for (int x = 0; x < currentImage.Width; x++)
                            {
                                Color pixelColor = currentImage.GetPixels()[x, y].ToColor().ToColor();
                                int mapIndex = Array.FindIndex(ColorMap, item => item == pixelColor);
                                idx.WriteByte((byte)(mapIndex == -1 ? 0 : mapIndex));
                                if (Alpha != 0) alpha.WriteByte(pixelColor.A);
                            }
                        mipmap._indexList = idx.ToArray();
                        if (Alpha != 0) mipmap._alphaList = alpha.ToArray();
                        alpha.Close();
                        idx.Close();
                    }
                    Mipmaps[index++] = mipmap;
                }
            }
            if (index < 8) Mipmaps[index++] = new BlpMipmap(this);
        }

        private bool FindUnMatchColorMap(IMagickImage a, IMagickImage b)
        {
            for (int i = 0; i < a.ColormapSize; i++)
            {
                var item = a.GetColormap(i);
                for (int j = 0; j < b.ColormapSize; j++)
                    if (item == b.GetColormap(j))
                        goto Found;
                return true;
            Found:;
            }
            return false;
        }
        #endregion

        public void Rebuild(CompressionType compression, int mipmapCount = 8, int quality = 75, bool mergeHeader = false)
        {
            switch (compression)
            {
                case CompressionType.Jpeg: SetJpegImage(Mipmaps[0].File, quality, mipmapCount, mergeHeader); return;
                case CompressionType.Palette: SetPaletteImage(Mipmaps[0].File, mipmapCount); return;
            }
        }

        public Stream GetStream()
        {
            ByteStream bs = new ByteStream();
            bs.Write(Signature);
            bs.Write((int)Compression);
            bs.Write(Alpha);
            bs.Write(Width);
            bs.Write(Height);
            bs.Write(PictureType);
            bs.Write(HasMipmaps ? 1 : 0);
            int LastOffset = -1, LastLength = -1;
            long OffsetPos = bs.Position;
            bs.WriteEmpty(0x80);
            using (ByteStream Offset = new ByteStream())
            using (ByteStream Length = new ByteStream())
            {
                switch (Compression)
                {
                    case CompressionType.Jpeg:
                        bs.Write(_HeadData?.Length ?? 0);
                        if (_HeadData != null) bs.Write(_HeadData);
                        for (int i = 0; i < 16; i++)
                        {
                            if (Mipmaps[i] is BlpJpegMipmap mipmap)
                            {
                                Offset.Write(LastOffset = (int)bs.Position);
                                byte[] buffer = mipmap._data;
                                bs.Write(buffer);
                                Length.Write(LastLength = buffer.Length);
                            }
                            else if (Mipmaps[i] is BlpMipmap)
                            {
                                Offset.Write(LastOffset);
                                Length.Write(LastLength);
                            }
                            else break;
                        }
                        break;

                    case CompressionType.Palette:
                        foreach (var item in ColorMap)
                        {
                            bs.WriteByte(item.B);
                            bs.WriteByte(item.G);
                            bs.WriteByte(item.R);
                            bs.WriteEmpty(1);
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            if (Mipmaps[i] is BlpPalettedMipmap mipmap)
                            {
                                Offset.Write(LastOffset = (int)bs.Position);
                                byte[] buffer = mipmap.RawData;
                                bs.Write(buffer);
                                Length.Write(LastLength = buffer.Length);
                            }
                            else if (Mipmaps[i] is BlpMipmap)
                            {
                                Offset.Write(LastOffset);
                                Length.Write(LastLength);
                            }
                            else break;
                        }
                        break;
                }
                bs.Position = OffsetPos;
                Offset.SetLength(0x40);
                Length.SetLength(0x40);
                bs.Write(Offset);
                bs.Write(Length);
                Offset.Close();
                Length.Close();
            }
            return bs;
        }

        public byte[] ToArray()
        {
            using (MemoryStream ms = (MemoryStream)GetStream())
            {
                return ms.ToArray();
            }
        }

        public void WriteFile(string fileName) => WriteFile(new FileInfo(fileName));
        public void WriteFile(FileInfo file)
        {
            using (FileStream stream = file.Open(FileMode.Create, FileAccess.Write))
            {
                WriteFile(stream);
                stream.Close();
            }
        }
        public void WriteFile(Stream stream)
        {
            stream.Write(Signature);
            stream.Write((int)Compression);
            stream.Write(Alpha);
            stream.Write(Width);
            stream.Write(Height);
            stream.Write(PictureType);
            stream.Write(HasMipmaps ? 1 : 0);
            int LastOffset = -1, LastLength = -1;
            long OffsetPos = stream.Position;
            stream.WriteEmpty(0x80);
            using (ByteStream Offset = new ByteStream())
            using (ByteStream Length = new ByteStream())
            {
                switch (Compression)
                {
                    case CompressionType.Jpeg:
                        stream.Write(_HeadData?.Length ?? 0);
                        if (_HeadData != null) stream.Write(_HeadData);
                        for (int i = 0; i < 16; i++)
                        {
                            if (Mipmaps[i] is BlpJpegMipmap mipmap)
                            {
                                Offset.Write(LastOffset = (int)stream.Position);
                                byte[] buffer = mipmap._data;
                                stream.Write(buffer);
                                Length.Write(LastLength = buffer.Length);
                            }
                            else if (Mipmaps[i] is BlpMipmap)
                            {
                                Offset.Write(LastOffset);
                                Length.Write(LastLength);
                            }
                            else break;
                        }
                        break;

                    case CompressionType.Palette:
                        foreach (var item in ColorMap)
                        {
                            stream.WriteByte(item.B);
                            stream.WriteByte(item.G);
                            stream.WriteByte(item.R);
                            stream.WriteByte(0);
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            if (Mipmaps[i] is BlpPalettedMipmap mipmap)
                            {
                                Offset.Write(LastOffset = (int)stream.Position);
                                byte[] buffer = mipmap.RawData;
                                stream.Write(buffer);
                                Length.Write(LastLength = buffer.Length);
                            }
                            else if (Mipmaps[i] is BlpMipmap)
                            {
                                Offset.Write(LastOffset);
                                Length.Write(LastLength);
                            }
                            else break;
                        }
                        break;
                }
                stream.Position = OffsetPos;
                Offset.SetLength(0x40);
                Length.SetLength(0x40);
                stream.Write(Offset);
                stream.Write(Length);
                Offset.Close();
                Length.Close();
            }
            stream.Flush();
        }

#if DEBUG
        public static void ShowPixelColor(IMagickImage image, int x = 100, int y = 100)
        {
            MagickColor color = image.GetPixels()[x, y].ToColor();
            switch (image.ColorSpace)
            {
                case ColorSpace.RGB:
                case ColorSpace.sRGB:
                    Console.WriteLine($"R: {color.R}, G: {color.G}, B: {color.B}");
                    return;
                case ColorSpace.CMY:
                    Console.WriteLine($"C: {color.R}, M: {color.G}, Y: {color.B}");
                    return;
                case ColorSpace.CMYK:
                    Console.WriteLine($"C: {color.R}, M: {color.G}, Y: {color.B}, K: {color.K}");
                    return;
                case ColorSpace.Gray:
                    Console.WriteLine($"R: {color.R}");
                    return;
                default:
                    Console.WriteLine($"Unspecified Type: {image.ColorSpace}");
                    return;
            }
        }
#endif
    }

    public enum CompressionType : int
    {
        Jpeg = 0,
        Palette = 1
    }
}

