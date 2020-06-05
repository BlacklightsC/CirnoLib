using System.Collections.Generic;

namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXSequenceChunk : List<MDXSequence>, IArrayable
    {
        public const uint Header = 0x53514553;  // SEQS
        public int ChunkSize { get => Count * 0x84; }    // Count * 132

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

    public sealed class MDXSequence : IArrayable
    {
        public string Name;

        public int IntervalStart;
        public int IntervalEnd;
        public float MoveSpeed;
        public int Flags;       // 0 - Looping
                                // 1 - NonLooping
        public float Rarity;
        public int SyncPoint;

        public float BoundsRadius;
        public Float3 MinimumExtent;
        public Float3 MaximumExtent;

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Name, 80);

                bs.Write(IntervalStart);
                bs.Write(IntervalEnd);
                bs.Write(MoveSpeed);
                bs.Write(Flags);

                bs.Write(Rarity);
                bs.Write(SyncPoint);

                bs.Write(BoundsRadius);
                bs.Write(MinimumExtent);
                bs.Write(MaximumExtent);

                return bs.ToArray();
            }
        }
    }
}