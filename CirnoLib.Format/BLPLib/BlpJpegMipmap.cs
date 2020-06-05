using System.IO;
using ImageMagick;

namespace CirnoLib.Format.BLPLib
{
    public sealed class BlpJpegMipmap : BlpMipmap
    {
        internal byte[] _data { get; set; }

        internal BlpJpegMipmap(BlpTexture BaseTexture, byte[] data) : base(BaseTexture) { _data = data; }

        public override byte[] RawData {
            get {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(BaseTexture._HeadData ?? new byte[0]);
                    bs.Write(_data ?? new byte[0]);

                    return bs.ToArray();
                }
            }
        }

        private MagickImage GetImage()
        {
            MagickImage image = new MagickImage(RawData);
            image.ColorSpace = ColorSpace.sRGB;
            MagickColorMatrix matrix = new MagickColorMatrix(3);
            matrix.SetRow(0, 0, 0, 1);
            matrix.SetRow(1, 0, 1, 0);
            matrix.SetRow(2, 1, 0, 0);
            image.ColorMatrix(matrix);
            return image;
        }

        public override byte[] File {
            get {
                using (MagickImage image = GetImage())
                    return image.ToByteArray();
            }
        }

        public override Stream CreateStream()
        {
            using (MagickImage image = GetImage())
            {
                MemoryStream stream = new MemoryStream();
                image.Write(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }
    }
}
