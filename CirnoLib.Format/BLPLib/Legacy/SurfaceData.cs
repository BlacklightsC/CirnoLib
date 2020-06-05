using System;

namespace CirnoLib.Format.BLPLib.Legacy
{
	public struct SurfaceData
	{
		public readonly IntPtr DataPointer;
		public readonly int Stride;
		public readonly int Width;
		public readonly int Height;

		public SurfaceData(int width, int height, IntPtr dataPointer, int stride)
		{
			Width = width;
			Height = height;
			DataPointer = dataPointer;
			Stride = stride;
		}
	}
}
