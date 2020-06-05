using System.Runtime.InteropServices;

namespace CirnoLib.Format.BLPLib.Legacy
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ArgbColor
	{
		public byte B;
		public byte G;
		public byte R;
		public byte A;

		public ArgbColor(ushort color)
		{
			// Optimization used here:
			// x / 31 = x * 8457 >> 18 (for 0 ≤ x ≤ 31 * 255)
			// x / 63 = x * 16645 >> 20 (for 0 ≤ x ≤ 63 * 255)
			// Since a multiplication is needed anyway, it is probably useless to try optimizing the * 255…
			B = (byte)((color & 0x1F) * 0x20E7F7 >> 18);
			G = (byte)(((color & 0x7E0) >> 5) * 0x40C3FB >> 20);
			R = (byte)((color >> 11) * 0x20E7F7 >> 18);
			A = 255;
		}
		
		public ArgbColor(byte r, byte g, byte b)
			: this(r, g, b, 0xFF) { }

		public ArgbColor(byte r, byte g, byte b, byte a)
		{
			B = b;
			G = g;
			R = r;
			A = a;
		}

		/// <summary>Copies an <see cref="ArgbColor"/> into another, with forced opaque alpha.</summary>
		/// <param name="destination">The destination color.</param>
		/// <param name="source">The source color.</param>
		internal static unsafe void CopyOpaque(ArgbColor* destination, ArgbColor* source)
		{
			destination->B = source->B;
			destination->G = source->G;
			destination->R = source->R;
			destination->A = 255;
		}

		/// <summary>Copies an <see cref="ArgbColor"/> into another, with forced opaque alpha.</summary>
		/// <param name="destination">The destination color.</param>
		/// <param name="source">The source color.</param>
		internal static unsafe void CopyWithAlpha(ArgbColor* destination, ArgbColor* source, byte alpha)
		{
			destination->B = source->B;
			destination->G = source->G;
			destination->R = source->R;
			destination->A = alpha;
		}
	}
}
