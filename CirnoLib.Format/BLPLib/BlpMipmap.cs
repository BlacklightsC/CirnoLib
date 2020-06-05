using System;
using System.IO;

namespace CirnoLib.Format.BLPLib
{
    public class BlpMipmap
    {
        public BlpTexture BaseTexture { get; }

        internal BlpMipmap(BlpTexture BaseTexture)
        {
            this.BaseTexture = BaseTexture;
        }

        public virtual byte[] RawData { get; }

        public virtual byte[] File { get; }

        public virtual Stream CreateStream() => throw new NotImplementedException();
    }
}
