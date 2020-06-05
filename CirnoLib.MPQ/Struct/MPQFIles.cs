using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQFiles : List<MPQFile>
    {
        private MPQArchive _Archive;
        public MPQArchive Archive {
            get => _Archive;
            internal set {
                if (_Archive is MPQArchive)
                    throw new FieldAccessException("이미 MPQArchive와 연결되어 있습니다.");
                _Archive = value;
            }
        }

        public bool ReadOnly { get => _Archive.ReadOnly; }
        public bool StreamMode { get => _Archive.StreamMode; }
        internal bool Initialized { get => _Archive.Initialized; }

        /// <summary>
        /// <see cref="MPQFiles"/>와 추가적인 데이터가 차지하는 최소 크기입니다.
        /// </summary>
        public int LeastSize {
            get {
                int value = 0;
                foreach (var item in this)
                    value += item.Length;
                return value;
            }
        }    
        public int SectorSize {
            get => 0x200 << _Archive.Header.SectorSize;
            set => _Archive.Header.SectorSize = (ushort)(Convert.ToString(value, 2).Substring(10).Length);
        }
        public int[] InsertOffset { get; set; } = new int[2];

        public MPQHashTable HashTable { get => _Archive.HashTable; }

        private MPQBlockTable BlockTable { get => _Archive.BlockTable; }

        public MPQFiles(MPQArchive archive) => _Archive = archive;
        public MPQFiles(MPQArchive archive, byte[] data, bool tryFindAllKey = false) : this(archive) => Parse(data, archive.MPQHeaderPos);
        public MPQFiles(MPQArchive archive, Stream data, bool tryFindAllKey = false) : this(archive) => Parse(data, archive.MPQHeaderPos);

        /// <summary>
        /// 개체를 <see cref="MPQFiles"/>의 끝 부분에 추가합니다.
        /// </summary>
        /// <param name="item"><see cref="MPQFiles"/>의 끝에 추가할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Add(MPQFile item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Files = this;
            base.Add(item);
        }

        /// <summary>
        /// <see cref="MPQFiles"/>의 지정된 인덱스에 요소를 삽입합니다.
        /// </summary>
        /// <param name="Index"><paramref name="item"/> 삽입해야 하는 인덱스(0부터 시작)입니다.</param>
        /// <param name="item">삽입할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Insert(int Index, MPQFile item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Files = this;
            base.Insert(Index, item);
        }
        public MPQFile Find(MPQHash Hash)
        {
            MPQFile File = FindLast(item => item.Hash == Hash);
            return File == null || File.Files == null ? null : File;
        }
        public MPQFile Find(MPQBlock Block)
        {
            MPQFile File = FindLast(item => item.Block == Block);
            return File == null || File.Files == null ? null : File;
        }
        public MPQFile Find(string FileName)
        {
            uint Name1 = FileName.HashString(1);
            uint Name2 = FileName.HashString(2);
            MPQFile File = FindLast(item => item.Hash.Name1 == Name1
                                         && item.Hash.Name2 == Name2);
            if (File == null || File.Files == null) return null;
            File.FileName = FileName.GetBytes();
            return File;
        }
        public MPQFile Find(byte[] FileName) => Find(Encoding.UTF8.GetString(FileName));
        public MPQFile Find(uint Name1, uint Name2)
        {
            MPQFile File = FindLast(item => item.Hash.Name1 == Name1 
                                         && item.Hash.Name2 == Name2);
            return File == null || File.Files == null ? null : File;
        }

        public void Encrypt(string FileName) => Encrypt(Encoding.UTF8.GetBytes(FileName));
        public void Encrypt(byte[] FileName)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            MPQFile File = Find(FileName);
            if (File != null) File.Encrypt(FileName.HashString(3));
        }

        public void Insert(string FileName, byte[] data) => Insert(Encoding.UTF8.GetBytes(FileName), data);
        public void Insert(byte[] FileName, byte[] data)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            MPQFile File = Find(FileName);
            if (File != null) File.File = data;
            else
            {
                MPQHash Hash = new MPQHash();
                MPQBlock Block = new MPQBlock
                {
                    FilePos = 0x200,
                    Flags = MPQ_FILE_EXISTS | MPQ_FILE_COMPRESS | MPQ_FILE_ENCRYPTED
                };
                File = new MPQFile
                {
                    Hash = Hash,
                    Block = Block,
                    Files = this
                };
                Add(File);
                File.SetName(FileName);
                Hash.BlockIndex = (uint)BlockTable.Count;
                BlockTable.Add(Block);

                File.File = data;
                for (int i = 0; i < HashTable.Count; i++)
                    if (HashTable[i].Name1 == 0xFFFFFFFF
                     && HashTable[i].Name2 == 0xFFFFFFFF)
                    {
                        Hash.Table = HashTable;
                        HashTable[i] = Hash;
                        return;
                    }
                HashTable.Add(Hash);
            }
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQFile"/>에 대한 정보를 읽어옵니다.
        /// <see cref="MPQBlockTable"/> 클래스가 초기화 되어있어야 합니다.
        /// </summary>
        /// <param name="data"><see cref="MPQFile"/>을 포함하고 있는 바이트 배열입니다.</param>
        /// <param name="HeaderPos"><see cref="MPQHeader"/>의 시작 인덱스입니다.</param>
        public void Parse(byte[] data, int HeaderPos, bool tryFindAllKey = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Clear();
            for (int i = 0; i < BlockTable.Count; i++)
            {
                if ((BlockTable[i].Flags & MPQ_FILE_EXISTS) == 0) continue;
                try
                {
                    MPQFile File = new MPQFile
                    {
                        BlockIndex = (uint)i,
                        Block = BlockTable[i]
                    };
                    if (!ReadOnly)
                    {
                        byte[] buffer;
                        int FilePos = HeaderPos + File.Block.FilePos;
                        if ((FilePos + File.Block.CSize) >= data.Length)
                        {
                            int sectorSize = 0x200 << _Archive.Header.SectorSize;
                            if (data.Length < FilePos + sectorSize)
                                buffer = data.SubArray(FilePos);
                            else buffer = data.SubArray(FilePos, sectorSize);
                            if ((File.Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
                            {
                                uint key = buffer.DetectFileKeyBySectorSize(File.Block.FSize, (uint)sectorSize);
                                buffer.DecryptBlock(key - 1);
                            }
                            int start = buffer.ToInt32();
                            if (start < 8)
                            {
                                File.Block.RemoveSelfInTable();
                                continue;
                            }
                            File.Block.CSize = buffer.ToUInt32(start - 4);
                        }
                        buffer = data.SubArray(FilePos, (int)File.Block.CSize);
                        File.RawFile = buffer;
                    }
                    MPQHash Hash = HashTable.Find((uint)i);
                    if (Hash != null) File.Hash = Hash;
                    Add(File);
                }
                catch
                {
                    BlockTable[i].RemoveSelfInTable();
                }
            }
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQFile"/>에 대한 정보를 읽어옵니다.
        /// <see cref="MPQBlockTable"/> 클래스가 초기화 되어있어야 합니다.
        /// </summary>
        /// <param name="data"><see cref="MPQFile"/>을 포함하고 있는 스트림입니다.</param>
        /// <param name="HeaderPos"><see cref="MPQHeader"/>의 시작 인덱스입니다.</param>
        public void Parse(Stream data, int HeaderPos, bool tryFindAllKey = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Clear();
            for (int i = 0; i < BlockTable.Count; i++)
            {
                if ((BlockTable[i].Flags & MPQ_FILE_EXISTS) == 0) continue;
                try
                {
                    MPQFile File = new MPQFile
                    {
                        BlockIndex = (uint)i,
                        Block = BlockTable[i]
                    };
                    if (!ReadOnly)
                    {
                        byte[] buffer;
                        int FilePos = HeaderPos + File.Block.FilePos;
                        if ((FilePos + File.Block.CSize) >= data.Length)
                        {
                            int sectorSize = 0x200 << _Archive.Header.SectorSize;
                            if (data.Length < FilePos + sectorSize)
                                buffer = data.SubArray(FilePos);
                            else buffer = data.SubArray(FilePos, sectorSize);
                            if ((File.Block.Flags & MPQ_FILE_ENCRYPTED) != 0)
                            {
                                uint key = buffer.DetectFileKeyBySectorSize(File.Block.FSize, (uint)sectorSize);
                                buffer.DecryptBlock(key - 1);
                            }
                            int start = buffer.ToInt32();
                            if (start < 8)
                            {
                                File.Block.RemoveSelfInTable();
                                continue;
                            }
                            File.Block.CSize = buffer.ToUInt32(start - 4);
                        }
                        buffer = data.SubArray(FilePos, (int)File.Block.CSize);
                        File.RawFile = buffer;
                    }
                    MPQHash Hash = HashTable.Find((uint)i);
                    if (Hash != null) File.Hash = Hash;
                    Add(File);
                }
                catch
                {
                    BlockTable[i].RemoveSelfInTable();
                }
            }
        }


        /// <summary>
        /// <see cref="MPQFiles"/>에 포함된 데이터의 순서를 무작위로 섞습니다.
        /// </summary>
        public void Shuffle()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            for (int i = Common.ShuffleLoopCount; i >= 0; i--)
            {
                int idx1 = Count.GetRandom();
                int idx2 = Count.GetRandom();

                MPQFile File = this[idx1];
                this[idx1] = this[idx2];
                this[idx2] = File;
            }
        }

        public void Remove(MPQHash Hash) => Find(Hash)?.Remove();
        public void Remove(MPQBlock Block) => Find(Block)?.Remove();
        public void Remove(string FileName) => Remove(Encoding.UTF8.GetBytes(FileName));
        public void Remove(byte[] FileName) => Find(FileName)?.Remove();

        public void TryReadListfile(params string[] list)
        {
            string[] listfile = Find("(listfile)")?.File?.GetString().Replace("\r", string.Empty).Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (listfile != null) foreach (var item in listfile) Find(item);
            if (list != null && list.Length > 0) foreach (var item in list) Find(item);
        }

        public int AllowedFilePos(bool isMax)
        {
            int Pos = 0;
            if (isMax)
            {
                Pos = 0x80000;
                for (int i = InsertOffset[1]; i < Count; i++)
                    Pos += this[i].Length;
            }
            else
            {
                int MPQPos = 0x34;
                for (int i = 0; i < InsertOffset[1]; i++)
                {
                    if (InsertOffset[0] == i)
                    {
                        int size = 0;
                        for (int j = 1; MPQPos > size; j++)
                            size = 0x200 * j;
                        MPQPos = size + 4;
                    }
                    MPQPos += this[i].Length;
                }
                int _size = 0;
                for (int j = 1; MPQPos > _size; j++)
                    _size = 0x200 * j;
                MPQPos = _size;
                Pos = 0x34 - MPQPos;
            }
            return Pos;
        }

        public int BiggestFileSize {
            get {
                int Size = 0;
                foreach (var item in this)
                    if (Size < item.Length)
                        Size = item.Length;
                return Size;
            }
        }
    }
}
