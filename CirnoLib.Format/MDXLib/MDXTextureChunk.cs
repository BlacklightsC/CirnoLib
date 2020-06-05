using System.Collections.Generic;

namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXTextureChunk : List<Texture>, IArrayable
    {
        public const uint Header = 0x53584554;
        public int ChunkSize { get => Count * 268; }

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

    public sealed class Texture : IArrayable
    {
        public int ReplaceableId;

        public string FileName;
        public int Flags;           //#1 - WrapWidth
                                    //#2 - WrapHeight

        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(ReplaceableId);

                bs.Write(FileName, 260);
                bs.Write(Flags);

                return bs.ToArray();
            }
        }
    }
}