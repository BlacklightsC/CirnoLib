﻿using System;
using System.IO;

namespace CirnoLib.MPQ.CompressLib
{
    internal class BitStream
    {
        private Stream _baseStream;
        private int _current;
        private int _bitCount;

        public BitStream(Stream sourceStream)
        {
            _baseStream = sourceStream;
        }

        public int ReadBits(int bitCount)
        {
            if (bitCount > 16) throw new ArgumentOutOfRangeException("BitCount", "Maximum BitCount is 16");
            if (!EnsureBits(bitCount)) return -1;
            int result = _current & (0xffff >> (16 - bitCount));
            WasteBits(bitCount);
            return result;
        }

        public int PeekByte()
        {
            if (!EnsureBits(8)) return -1;
            return _current & 0xff;
        }

        public bool EnsureBits(int bitCount)
        {
            if (bitCount <= _bitCount) return true;

            if (_baseStream.Position >= _baseStream.Length) return false;
            int nextvalue = _baseStream.ReadByte();
            _current |= nextvalue << _bitCount;
            _bitCount += 8;
            return true;
        }

        private bool WasteBits(int bitCount)
        {
            _current >>= bitCount;
            _bitCount -= bitCount;
            return true;
        }
    }
}
