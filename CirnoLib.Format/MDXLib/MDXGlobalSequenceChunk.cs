using System.Collections.Generic;

namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXGlobalSequenceChunk : List<MDXGlobalSequence>, IArrayable
    {
        public const uint Header = 0x534C4247;  // GBLS
        public int ChunkSize { get => Count * 4; }

        public new byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Header);
                bs.Write(ChunkSize);

                foreach (var item in this)
                    bs.Write(item);

                return bs.ToArray();
            }
        }
    }

    public sealed class MDXGlobalSequence
    {
        public int Duration;

        public static implicit operator int(MDXGlobalSequence GBLS) => GBLS.Duration;
        public static implicit operator MDXGlobalSequence(int Duration) => new MDXGlobalSequence { Duration = Duration };
        public byte[] ToArray() => Duration.GetBytes();
    }
}