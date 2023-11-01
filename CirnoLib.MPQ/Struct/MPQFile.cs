using System;
using System.IO;
using System.Collections.Generic;

using CirnoLib.MPQ.CompressLib;
using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQFile : IDisposable
    {
        /// <summary>
        /// If the hash table entry is valid, this is the index into the block table of the file.
        /// Otherwise, one of the following two values:
        ///  - FFFFFFFFh: Hash table entry is empty, and has always been empty.
        ///               Terminates searches for a given file.
        ///  - FFFFFFFEh: Hash table entry is empty, but was valid at some point (a deleted file).
        ///               Does not terminate searches for a given file.
        /// </summary>
        public uint BlockIndex {
            get {
                if (Hash == null || Block == null)
                    return 0x80000000;
                else
                    return Hash.BlockIndex;
            }
            set {
                if (Hash != null && Block != null)
                    Hash.BlockIndex = value;
            }
        }

        public bool ReadOnly { get => Files?.ReadOnly ?? false; }
        public bool StreamMode { get => Files?.StreamMode ?? false; }
        internal bool Initialized { get => Files?.Initialized ?? false; }
        private MPQArchive Archive { get => Files?.Archive; }

        public MPQFiles Files { get; internal set; }
        public MPQHash Hash { get; internal set; }
        public MPQBlock Block { get; internal set; }
        /// <summary>
        /// 발견된 파일의 이름입니다. 초기화 직후에는 <see langword="null"/> 입니다.
        /// </summary>
        public byte[] FileName { get; internal set; } = null;
        private uint _Key = 0;
        public uint Key {
            get {
                if (_Key == 0)
                {
                    try
                    {
                        _Key = _File.DetectFileKeyBySectorSize(Block.FSize, (uint)Files.SectorSize);
                    }
                    catch
                    {
                        _Key = FileName.HashString(3);
                    }
                    if ((Block.Flags & MPQ_FILE_FIX_KEY) != 0)
                    {
                        _Key ^= Block.FSize;
                        _Key -= (uint)Block.FilePos;
                    }
                }
                return _Key;
            }
            set => _Key = value;
        }
        private byte[] _File;
        /// <summary>
        /// 압축이나 암호화가 이루어진 파일의 바이트 배열입니다.
        /// </summary>
        public byte[] RawFile {
            get {
                if (ReadOnly)
                {
                    uint CSize = Block.CSize;
                    if ((Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
                    {
                        int RealFilePos = Archive.MPQHeaderPos + Block.FilePos;
                        byte[] sector;
                        if (StreamMode) sector = Archive.Stream.SubArray(RealFilePos, 8);
                        else sector = Archive.Data.SubArray(RealFilePos, 8);
                        uint key = Key - 1;
                        if ((Block.Flags & MPQ_FILE_FIX_KEY) != 0)
                        {
                            key = (uint)((key + Block.FilePos) ^ Block.FSize);
                        }
                        sector.DecryptBlock(key);
                        int start = sector.ToInt32();
                        if (StreamMode) sector = Archive.Stream.SubArray(RealFilePos, start);
                        else sector = Archive.Data.SubArray(RealFilePos, start);
                        sector.DecryptBlock(key);
                        CSize = sector.ToUInt32(start - 4);
                    }
                    if (StreamMode) return Archive.Stream.SubArray(Archive.MPQHeaderPos + Block.FilePos, (int)CSize);
                    else return Archive.Data.SubArray(Archive.MPQHeaderPos + Block.FilePos, (int)CSize);
                }
                else
                    return (byte[])_File.Clone();
            }
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _File = value;
            }
        }
        /// <summary>
        /// 원본 파일의 바이트 배열입니다.
        /// </summary>
        public byte[] File {
            get {
                byte[] buffer = RawFile;
                if (Block != null && Files != null)
                {
                    if ((Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
                    {
                        buffer = Decrypt(buffer, Key);
                    }
                    if ((Block.Flags & MPQ_FILE_COMPRESS) != 0)
                    {
                        buffer = Decompress(buffer);
                    }
                }
                return buffer;
            }
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                byte[] buffer = (byte[])value.Clone();
                if (Block != null)
                {
                    Block.FSize = (uint)buffer.Length;
                    if ((Block.Flags & MPQ_FILE_COMPRESS) != 0)
                    {
                        buffer = Compress(buffer);
                    }
                    if ((Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
                    {
                        uint Key;
                        if ((Block.Flags & MPQ_FILE_FIX_KEY) != 0)
                            Block.Flags ^= MPQ_FILE_FIX_KEY;
                        if (Hash != null && (Key = this.Key) != 0)
                        {
                            buffer = Encrypt(buffer, Key);
                        }
                    }
                    Block.CSize = (uint)buffer.Length;
                }
                _File = buffer;
            }
        }
        /// <summary>
        /// 압축이나 암호화가 이루어진 파일의 크기입니다.
        /// </summary>
        public int Length { get => _File.Length; }
        public MPQFile()
        {
            _File = null;
            FileName = null;
        }

        public void Remove()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Files?.Remove(this);
            Hash?.RemoveSelfInTable();
            Block?.RemoveSelfInTable();
            Dispose(true);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.
                _File = null;
                disposedValue = true;
                Hash = null;
                Block = null;
                Files = null;
            }
        }

        // TODO: 위의 Dispose(bool disposing)에 관리되지 않는 리소스를 해제하는 코드가 포함되어 있는 경우에만 종료자를 재정의합니다.
        // ~MPQFile() {
        //   // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
        //   Dispose(false);
        // }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 위의 Dispose(bool disposing)에 정리 코드를 입력하세요.
            Dispose(true);
            // TODO: 위의 종료자가 재정의된 경우 다음 코드 줄의 주석 처리를 제거합니다.
            // GC.SuppressFinalize(this);
        }
        #endregion

        public void SetName(byte[] FileName)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            bool IsEncrypted = false;
            if ((Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
            {
                IsEncrypted = true;
                Decrypt();
            }
            Hash.SetName(this.FileName = FileName);
            if (IsEncrypted) Encrypt();
        }
        public void SetName(string FileName) => SetName(FileName.GetBytes());

        public void Compress()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if ((Block.Flags & MPQ_FILE_COMPRESS) != 0) return;
            _File = Compress(_File, Files.SectorSize);
            Block.Flags |= MPQ_FILE_COMPRESS;
        }
        public byte[] Compress(byte[] data) => Compress(data, Files.SectorSize);
        public static byte[] Compress(byte[] data, int SectorSize)
        {
            int OffsetPos = 0, FileSize = data.Length;
            int OffsetSize = (((FileSize - 1) / SectorSize) + 2) * 4;
            List<byte> Offset = new List<byte>(OffsetSize);
            List<byte> Sector = new List<byte>(FileSize);
            Offset.AddRange(BitConverter.GetBytes(OffsetSize));
            for (int i = 0; i < FileSize; i += SectorSize)
            {
                int length = FileSize - i < SectorSize
                           ? FileSize - i
                           : SectorSize;

                byte[] temp = ZLibHelper.Deflate(data.SubArray(i, length));
                Offset.AddRange(BitConverter.GetBytes(OffsetSize + (OffsetPos += temp.Length + 1)));
                Sector.Add(0x02);
                Sector.AddRange(temp);
            }
            Sector.InsertRange(0, Offset.ToArray());
            return Sector.ToArray();
        }

        public void Decompress()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if ((Block.Flags & MPQ_FILE_COMPRESS) == 0) return;
            _File = Decompress(_File);
            Block.Flags ^= MPQ_FILE_COMPRESS;
        }
        public byte[] Decompress(byte[] data) => Decompress(data, (uint)Files.SectorSize, Block.FSize);
        public static byte[] Decompress(byte[] data, uint SectorSize, uint FileSize = 0)
        {
            int Start = BitConverter.ToInt32(data, 0);
            int End = BitConverter.ToInt32(data, 4);
            int SectorCount = Start / 4 - 1;
            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < SectorCount; i++)
                {
                    byte[] Buffer;
                    try
                    {
                        Buffer = data.SubArray(Start, End - Start);
                    }
                    catch
                    {
                        Buffer = data.SubArray(Start, data.Length - Start);
                    }
                    byte[] Sector = DecompressType(Buffer, ms.Length + SectorSize < FileSize ? SectorSize : (FileSize - (uint)ms.Length));
                    ms.Write(Sector, 0, Sector.Length);
                    Start = End;
                    try
                    {
                        End = BitConverter.ToInt32(data, (i + 2) * 4);
                    }
                    catch
                    {
                        break;
                    }
                }
                return ms.ToArray();
            }
        }
        private static byte[] DecompressType(byte[] data, uint fileSize)
        {
            if (data == null || data.Length < 2) return new byte[0];
            byte[] Compressed = data.SubArray(1);
            switch (data[0])
            {
                default: return data;
                case 0x01: return Huffman.Decompress(Compressed);
                case 0x02: return ZLibHelper.Inflate(Compressed);
                case 0x08: return PKWARE.DecompressBlock(Compressed, 0, Compressed.Length, new byte[fileSize]);
                case 0x40: return IMA_ADPCM.Decompress(Compressed, 1);
                case 0x80: return IMA_ADPCM.Decompress(Compressed, 2);
                case 0x41:
                    Compressed = Huffman.Decompress(Compressed);



                    return IMA_ADPCM.Decompress(Compressed, 1);
                case 0x48:
                    Compressed = PKWARE.DecompressBlock(Compressed, 0, Compressed.Length, new byte[fileSize]);
                    return IMA_ADPCM.Decompress(Compressed, 1);
                case 0x81:
                    Compressed = Huffman.Decompress(Compressed);
                    return IMA_ADPCM.Decompress(Compressed, 2);
                case 0x88:
                    Compressed = PKWARE.DecompressBlock(Compressed, 0, Compressed.Length, new byte[fileSize]);
                    return IMA_ADPCM.Decompress(Compressed, 2);
            }
        }

        public void Encrypt(bool IsFixKey = false)
        {
            if (Key == 0) throw new KeyNotFoundException("Key가 존재하지 않습니다.");
            Encrypt(_Key, IsFixKey);
        }
        public void Encrypt(string FileName, bool IsFixKey = false) => Encrypt(FileName.HashString(3), IsFixKey);
        public void Encrypt(byte[] FileName, bool IsFixKey = false) => Encrypt(FileName.HashString(3), IsFixKey);
        public void Encrypt(uint key, bool IsFixKey = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if ((Block.Flags & MPQ_FILE_ENCRYPTED) != 0 || _File == null) return;
            _File = Encrypt(_File, key, IsFixKey);
            Block.Flags |= MPQ_FILE_ENCRYPTED;
            if (IsFixKey) Block.Flags |= MPQ_FILE_FIX_KEY;
        }
        public byte[] Encrypt(byte[] data, uint key, bool IsFixKey = false) => Encrypt(data, key, Block, Files.SectorSize, IsFixKey);
        public static byte[] Encrypt(byte[] data, uint key, MPQBlock block, int sectorSize, bool IsFixKey = false)
        {
            if (IsFixKey) key = (uint)((key + block.FilePos) ^ block.FSize);
            int Start = BitConverter.ToInt32(data, 0);
            int End = BitConverter.ToInt32(data, 4);
            int FinalSize = 0, SectorCount = Start / 4 - 1;
            List<byte> Offset = new List<byte>();
            List<byte> Sector = new List<byte>();
            for (int i = 0; i < SectorCount; i++)
            {
                Offset.AddRange(BitConverter.GetBytes(End));
                byte[] Buffer;
                try
                {
                    Buffer = data.SubArray(Start, End - Start);
                }
                catch
                {
                    Buffer = data.SubArray(Start, data.Length - Start);
                }
                Buffer.EncryptBlock((uint)(key + i));
                Sector.AddRange(Buffer);
                if (block.FSize - FinalSize <= sectorSize)
                {
                    Offset.InsertRange(0, BitConverter.GetBytes(i * 4 + 8));
                    Buffer = Offset.ToArray();
                    Buffer.EncryptBlock(key - 1);
                    Sector.InsertRange(0, Buffer);
                    return Sector.ToArray();
                }
                FinalSize += sectorSize;
                Start = End;
                try
                {
                    End = BitConverter.ToInt32(data, i * 4 + 8);
                }
                catch
                {
                    break;
                }
            }
            return null;
        }

        public void Decrypt()
        {
            if ((Block.Flags & MPQ_FILE_ENCRYPTED) == 0 || _File == null) return;
            _File = Decrypt(_File, Key);
            Block.Flags ^= MPQ_FILE_ENCRYPTED;
            if ((Block.Flags & MPQ_FILE_FIX_KEY) != 0) Block.Flags ^= MPQ_FILE_FIX_KEY;
        }
        public void Decrypt(string FileName) => Decrypt(FileName.HashString(3));
        public void Decrypt(byte[] FileName) => Decrypt(FileName.HashString(3));
        public void Decrypt(uint key)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if ((Block.Flags & MPQ_FILE_ENCRYPTED) == 0) return;
            _File = Decrypt(_File, key);
            Block.Flags ^= MPQ_FILE_ENCRYPTED;
            if ((Block.Flags & MPQ_FILE_FIX_KEY) != 0) Block.Flags ^= MPQ_FILE_FIX_KEY;
        }
        public byte[] Decrypt(byte[] data, uint key) => Decrypt(data, key, Block, Files.SectorSize);
        public static byte[] Decrypt(byte[] data, uint key, MPQBlock block, int sectorSize)
        {
            if ((block.Flags & MPQ_FILE_ENCRYPTED) == 0) return null;
            if ((block.Flags & MPQ_FILE_FIX_KEY) != 0) key = (uint)((key + block.FilePos) ^ block.FSize);
            byte[] Decrypted = (byte[])data.Clone();
            Decrypted.DecryptBlock(key - 1);
            int Start = BitConverter.ToInt32(Decrypted, 0);
            int End = BitConverter.ToInt32(Decrypted, 4);
            if (Start > data.Length) return data;
            int FinalSize = 0, SectorCount = Start / 4 - 1;
            List<byte> Buffer = new List<byte>(BitConverter.GetBytes(Start));
            for (int i = 0; i < SectorCount; i++)
            {
                Buffer.InsertRange(i * 4 + 4, BitConverter.GetBytes(End));
                byte[] temp;
                try
                {
                    temp = data.SubArray(Start, End - Start);
                }
                catch
                {
                    temp = data.SubArray(Start, data.Length - Start);
                }
                temp.DecryptBlock((uint)(key + i));
                Buffer.AddRange(temp);
                if (block.FSize - FinalSize <= sectorSize) break;
                FinalSize += sectorSize;
                Start = End;
                try
                {
                    End = BitConverter.ToInt32(Decrypted, i * 4 + 8);
                }
                catch
                {
                    break;
                }
            }
            return Buffer.ToArray();
        }

    }
}
