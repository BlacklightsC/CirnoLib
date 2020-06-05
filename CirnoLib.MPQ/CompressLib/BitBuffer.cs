using System;
using System.Runtime.InteropServices;

namespace CirnoLib.MPQ.CompressLib
{
    internal unsafe struct BitBuffer : IDisposable
    {
        private readonly GCHandle bufferHandle;
        private readonly byte* bufferPointer;
        private int pos, count, length;
        private byte b;

        internal BitBuffer(byte[] buffer, int index, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (index < 0 || index > buffer.Length) throw new ArgumentOutOfRangeException("index");
            if (count < 0 || checked(index + count) > buffer.Length) throw new ArgumentOutOfRangeException("count");

            this.bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            this.bufferPointer = (byte*)this.bufferHandle.AddrOfPinnedObject();
            this.count = 8;
            this.pos = index;
            this.length = index + count;
            this.b = buffer[this.pos++];
        }

        public void Dispose() { this.bufferHandle.Free(); }

        public int GetBit()
        {
            int r;

            if (count-- == 0)
            {
                if (pos < length) b = bufferPointer[pos++];
                else return 0;
                count = 7;
            }

            r = b & 0x1;
            b >>= 1;

            return r;
        }

        public int GetBits(int count)
        {
            int r = 0, n = 0, d;

            while (count > 0)
            {
                d = this.count - count;
                if (d >= 0)
                    do
                    {
                        r = r | ((b & 0x1) << n++);
                        b >>= 1;
                        count--;
                        this.count--;
                    } while (count > 0);
                else
                    while (this.count > 0)
                    {
                        r = r | ((b & 0x1) << n++);
                        b >>= 1;
                        count--;
                        this.count--;
                    }
                if (this.count == 0)
                {
                    if (pos < length)
                        b = bufferPointer[pos++];
                    else
                        return r;
                    this.count = 8;
                }
            }

            return r;
        }

        public byte GetByte() { return (byte)GetBits(8); }

        public bool Eof { get { return (pos == length - 1 && count == 0) || (pos >= length); } }
    }
}
