using ImageMagick;
using System.Drawing;

namespace CirnoLib.Format.BLPLib
{
    public static class ImageMagickExtension
    {
        public static Color ToColor(this MagickColor color)
            => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
