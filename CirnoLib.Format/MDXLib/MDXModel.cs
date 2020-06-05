namespace CirnoLib.Format.MDXLib
{
    public sealed class MDXModel : IArrayable
    {
        public const uint Header = 0x584C444D;  // MDLX

        public MDXVersionChunk VersionChunk = new MDXVersionChunk();
        public MDXModelChunk ModelChunk = new MDXModelChunk();
        public MDXSequenceChunk SequenceChunk = new MDXSequenceChunk();
        public MDXGlobalSequenceChunk GlobalSequenceChunk = new MDXGlobalSequenceChunk();
        public MDXTextureChunk TextureChunk = new MDXTextureChunk();
        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(Header);
                bs.Write(VersionChunk);
                bs.Write(ModelChunk);
                return bs.ToArray();
            }
        }
    }
}
