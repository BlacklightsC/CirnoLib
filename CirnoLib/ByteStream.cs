using System;
using System.IO;

namespace CirnoLib
{
    public sealed class ByteStream : MemoryStream
    {
        private readonly BinaryReader Reader;
        private readonly BinaryWriter Writer;
        private bool _IsBigEndian = !BitConverter.IsLittleEndian;
        public bool IsBigEndian
        {
            get => _IsBigEndian == BitConverter.IsLittleEndian;
            set => _IsBigEndian = value;
        }

        public ByteStream()
        {
            Reader = new BinaryReader(this, Global.DefaultEncoding, true);
            Writer = new BinaryWriter(this, Global.DefaultEncoding, true);
        }

        public ByteStream(int capacity) : base(capacity)
        {
            Reader = new BinaryReader(this, Global.DefaultEncoding, true);
            Writer = new BinaryWriter(this, Global.DefaultEncoding, true);
        }

        public ByteStream(byte[] array) : base(array)
        {
            Reader = new BinaryReader(this, Global.DefaultEncoding, true);
            Writer = new BinaryWriter(this, Global.DefaultEncoding, true);
        }

        public ByteStream(byte[] array, object Null) : this()
        {
            Write(array);
            Seek(0, SeekOrigin.Begin);
        }

        public byte[] ReadNullTerminate(byte endByte = 0, bool rewindLastByte = false)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    byte piece = ReadByte();
                    if (piece == endByte) break;
                    ms.WriteByte(piece);
                }
                if (rewindLastByte) Seek(-1, SeekOrigin.Current);
                return ms.ToArray();
            }
        }
        public string ReadNullTerminateString(byte endByte = 0, bool rewindLastByte = false)
            => ReadNullTerminate(endByte, rewindLastByte).GetString();

        #region [    Read Variables    ]
        public sbyte ReadSByte(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                sbyte value = Reader.ReadSByte();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadSByte();
            }
        }
        public byte ReadByte(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                byte value = Reader.ReadByte();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadByte();
            }
        }
        public byte[] ReadBytes(int length, int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                byte[] value = Reader.ReadBytes(length);
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadBytes(length);
            }
        }
        public char ReadChar(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                char value = Reader.ReadChar();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadChar();
            }
        }
        public char[] ReadChars(int length, int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                char[] value = Reader.ReadChars(length);
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadChars(length);
            }
        }
        public bool ReadBoolean(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                bool value = Reader.ReadBoolean();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadBoolean();
            }
        }
        public short ReadInt16(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                short value = IsBigEndian ? Reader.ReadBytes(2).ReverseCopy().ToInt16() : Reader.ReadInt16();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(2).ReverseCopy().ToInt16() : Reader.ReadInt16();
        }
        public int ReadInt32(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                int value = IsBigEndian ? Reader.ReadBytes(4).ReverseCopy().ToInt32() : Reader.ReadInt32();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(4).ReverseCopy().ToInt32() : Reader.ReadInt32();
        }
        public long ReadInt64(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                long value = IsBigEndian ? Reader.ReadBytes(8).ReverseCopy().ToInt64() : Reader.ReadInt64();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(8).ReverseCopy().ToInt64() : Reader.ReadInt64();
        }
        public ushort ReadUInt16(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                ushort value = IsBigEndian ? Reader.ReadBytes(2).ReverseCopy().ToUInt16() : Reader.ReadUInt16();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(2).ReverseCopy().ToUInt16() : Reader.ReadUInt16();
        }
        public uint ReadUInt32(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                uint value = IsBigEndian ? Reader.ReadBytes(4).ReverseCopy().ToUInt32() : Reader.ReadUInt32();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(4).ReverseCopy().ToUInt32() : Reader.ReadUInt32();
        }
        public ulong ReadUInt64(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                ulong value = IsBigEndian ? Reader.ReadBytes(8).ReverseCopy().ToUInt64() : Reader.ReadUInt64();
                base.Position = Position;
                return value;
            }
            else
                return IsBigEndian ? Reader.ReadBytes(8).ReverseCopy().ToUInt64() : Reader.ReadUInt64();
        }
        public float ReadSingle(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                float value = Reader.ReadSingle();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadSingle();
            }
        }
        public double ReadDouble(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                double value = Reader.ReadDouble();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadDouble();
            }
        }
        public decimal ReadDecimal(int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                decimal value = Reader.ReadDecimal();
                base.Position = Position;
                return value;
            }
            else
            {
                return Reader.ReadDecimal();
            }
        }
        public string ReadString(byte endByte = 0, int index = -2)
        {
            if (index >= -1)
            {
                long Position = base.Position;
                if (index >= 0) base.Position = index;
                string value = ReadNullTerminateString(endByte);
                base.Position = Position;
                return value;
            }
            else
            {
                return ReadNullTerminateString(endByte);
            }
        }
        #endregion
        #region [    Read by Property    ]
        public sbyte SByte => ReadSByte(-1);
        public byte Byte => ReadByte(-1);
        public char Char => ReadChar(-1);
        public bool Bool => ReadBoolean(-1);
        public short Int16=> ReadInt16(-1);
        public int Int32 => ReadInt32(-1);
        public long Int64 => ReadInt64(-1);
        public ushort UInt16 => ReadUInt16(-1);
        public uint UInt32 => ReadUInt32(-1);
        public ulong UInt64 => ReadUInt64(-1);
        public float Float => ReadSingle(-1);
        public double Double => ReadDouble(-1);
        public decimal Decimal => ReadDecimal(-1);
        public string String => ReadString(0, -1);
        #endregion
        #region [    Write    ]
        public void Write(sbyte value) => Writer.Write(value);
        public void Write(params byte[] buffer) => Writer.Write(buffer);
        public void Write(params char[] chars) => Writer.Write(chars);
        public void Write(bool value) => Writer.Write(value);
        public void Write(short value) => Writer.Write(value);
        public void Write(int value) => Writer.Write(value);
        public void Write(long value) => Writer.Write(value);
        public void Write(ushort value) => Writer.Write(value);
        public void Write(uint value) => Writer.Write(value);
        public void Write(ulong value) => Writer.Write(value);
        public void Write(float value) => Writer.Write(value);
        public void Write(double value) => Writer.Write(value);
        public void Write(decimal value) => Writer.Write(value);
        public void Write(string value, bool IsNullTerminate = true)
        {
            Writer.Write(value.GetBytes());
            if (IsNullTerminate) WriteByte(0);
        }
        public void Write(string value, int MaxLength)
        {
            byte[] buffer = value.GetBytes();
            if (buffer.Length > MaxLength)
                Writer.Write(buffer.SubArray(0, MaxLength));
            else if (buffer.Length == MaxLength)
                Writer.Write(buffer);
            else
            {
                Writer.Write(buffer);
                WriteEmpty(MaxLength - buffer.Length);
            }
        }
        public void Write(IArrayable value) => Writer.Write(value.ToArray());
        public void Write(IArrayable[] value) => Writer.Write(value.ToArray());
        #endregion
        public void WriteEmpty(int Length)
        {
            if (Length <= 0) return;
            SetLength(base.Length + Length);
            Seek(0, SeekOrigin.End);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reader.Dispose();
                Writer.Dispose();
            }
            base.Dispose(disposing);
        }
        public void Clear() => SetLength(0);
        public void Skip(int Length) => Seek(Length, SeekOrigin.Current);
        public IParsable Parse(IParsable obj) => obj.Parse(ToArray());
    }
}