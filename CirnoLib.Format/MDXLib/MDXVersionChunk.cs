namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXVersionChunk : IArrayable
    {
        public const uint Header = 0x53524556;  // VERS
        public const int ChunkSize = 4;

        public int Version = 800;

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Header);
                bs.Write(ChunkSize);

                bs.Write(Version);

                return bs.ToArray();
            }
        }
    }
}