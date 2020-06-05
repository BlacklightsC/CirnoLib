using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace CirnoLib.Format.BLPLib
{
    public sealed class BlpPalettedMipmap : BlpMipmap
    {
        internal byte[] _indexList;
        internal byte[] _alphaList;
        public int Width { get; }
        public int Height { get; }

        internal BlpPalettedMipmap(BlpTexture BaseTexture, int Width, int Height, bool AutoSet = true) : base(BaseTexture)
        {
            this.Width = Width;
            this.Height = Height;
            if (AutoSet)
            {
                _indexList = new byte[Width * Height];
                if (BaseTexture.Alpha != 8)
                    _alphaList = new byte[(Width * Height * BaseTexture.Alpha + 7) / 8];
            }
        }

        public override byte[] RawData {
            get {
                using (ByteStream bs = new ByteStream())
                {
                    bs.Write(_indexList);
                    if (_alphaList != null)
                        bs.Write(_alphaList);
                    return bs.ToArray();
                }
            }
        }

        public override byte[] File {
            get {
                using (MemoryStream ms = (MemoryStream)CreateStream())
                    return ms.ToArray();
            }
        }

        public override Stream CreateStream()
        {
            using (Bitmap bitmap = new Bitmap(Width, Height))
            {
                //https://github.com/triggerhappy187/PHP-BLP/blob/master/blp.php
                //https://www.hiveworkshop.com/threads/blp-specifications-wc3.279306/
                for (int y = 0, i = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++, i++)
                    {
                        int alpha;
                        if (_alphaList == null) alpha = 255;
                        else switch (BaseTexture.Alpha)
                            {
                                case 8: alpha = _alphaList[i]; break;
                                case 4: alpha = i % 2 > 0 ? _alphaList[i] >> 4 : _alphaList[i] & 15; break;
                                case 1: alpha = _alphaList[i / 8] & (1 << (i % 8)); break;
                                default: alpha = 255; break;
                            }
                        bitmap.SetPixel(x, y, Color.FromArgb(alpha, BaseTexture.ColorMap[_indexList[i]]));
                    }
                MemoryStream ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                return ms;
            }
        }
    }
}
