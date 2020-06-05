namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXModelChunk : IArrayable
    {
        public const uint Header = 0x4C444F4D;  // MODL
        public const int ChunkSize = 0x174;

        public string Name;
        public string AnimationFileName;

        public float BoundsRadius;
        public Float3 MinimumExtent;
        public Float3 MaximumExtent;
        public uint BlendTime;

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Header);
                bs.Write(ChunkSize);

                bs.Write(Name, 80);
                bs.Write(AnimationFileName, 260);

                bs.Write(BoundsRadius);
                bs.Write(MinimumExtent);
                bs.Write(MaximumExtent);
                bs.Write(BlendTime);

                return bs.ToArray();
            }
        }
    }
}