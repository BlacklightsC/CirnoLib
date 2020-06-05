using System;
using System.Collections.Generic;
using System.IO;
using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQBlockTable : List<MPQBlock>, IArrayable
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
        internal bool Initialized { get => _Archive.Initialized; }

        public MPQBlockTable() { }
        public MPQBlockTable(MPQArchive archive) => _Archive = archive;
        public MPQBlockTable(MPQArchive archive, byte[] data, bool tryFindAllKey = false) : this(archive)
        {
            if (archive.Header == null) throw new NullReferenceException("MPQHeader가 초기화되지 않았습니다.");
            byte[] buffer;
            int BlockTablePos = archive.MPQHeaderPos + archive.Header.BlockTablePos;
            if (BlockTablePos + (int)archive.Header.BlockTableSize * 0x10 < data.Length)
                buffer = data.SubArray(BlockTablePos, (int)archive.Header.BlockTableSize * 0x10);
            else buffer = data.SubArray(BlockTablePos);
            buffer.DecryptBlock(MPQ_BLOCK_KEY);
            Parse(buffer, data, tryFindAllKey);
        }
        public MPQBlockTable(MPQArchive archive, Stream data, bool tryFindAllKey = false) : this(archive)
        {
            if (archive.Header == null) throw new NullReferenceException("MPQHeader가 초기화되지 않았습니다.");
            byte[] buffer;
            int BlockTablePos = archive.MPQHeaderPos + archive.Header.BlockTablePos;
            if (BlockTablePos + (int)archive.Header.BlockTableSize * 0x10 < data.Length)
                buffer = data.SubArray(BlockTablePos, (int)archive.Header.BlockTableSize * 0x10);
            else buffer = data.SubArray(BlockTablePos);
            buffer.DecryptBlock(MPQ_BLOCK_KEY);
            Parse(buffer, data, tryFindAllKey);
        }

        /// <summary>
        /// <see cref="MPQBlockTable"/>의 단순 복사본을 만듭니다.
        /// </summary>
        /// <returns><see cref="MPQBlockTable"/>의 단순 복사본입니다.</returns>
        public MPQBlockTable Clone() => MemberwiseClone() as MPQBlockTable;

        /// <summary>
        /// <see cref="MPQBlockTable"/>를 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="MPQBlockTable"/>의 바이트 배열입니다.</returns>
        public new byte[] ToArray()
        {
            if (Count <= 0) return null;
            using (MemoryStream buffer = new MemoryStream())
            {
                for (int i = 0; i < Count; i++)
                    buffer.Write(this[i].ToArray(), 0, 0x10);
                return buffer.ToArray();
            }
        }
        /// <summary>
        /// 개체를 <see cref="MPQBlockTable"/>의 끝 부분에 추가합니다.
        /// </summary>
        /// <param name="item"><see cref="MPQBlockTable"/>의 끝에 추가할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Add(MPQBlock item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Table = this;
            base.Add(item);
        }

        /// <summary>
        /// <see cref="MPQBlockTable"/>의 지정된 인덱스에 요소를 삽입합니다.
        /// </summary>
        /// <param name="Index"><paramref name="item"/> 삽입해야 하는 인덱스(0부터 시작)입니다.</param>
        /// <param name="item">삽입할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Insert(int Index, MPQBlock item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Table = this;
            base.Insert(Index, item);
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQBlockTable"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQBlockTable"/>을 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value, byte[] data = null, bool tryFindAllKey = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Clear();
            for (int i = 0; i < value.Length / 0x10; i++)
            {
                MPQBlock Block;
                if (!_Archive.HashTable.Exists((uint)i)) goto Exception;
                uint FSize = BitConverter.ToUInt32(value, i * 0x10 + 0x8);
                uint Flags = BitConverter.ToUInt32(value, i * 0x10 + 0xC);
                if (FSize >= 0x10000000 || (Flags & MPQ_FILE_EXISTS) == 0) goto Exception;
                Block = new MPQBlock{
                    FilePos = BitConverter.ToInt32(value, i * 0x10),
                    CSize = BitConverter.ToUInt32(value, i * 0x10 + 0x4),
                    FSize = FSize,
                    Flags = Flags
                };
                if (data != null)
                {
                    int RealFilePos = _Archive.MPQHeaderPos + Block.FilePos;
                    if ((Block.Flags & MPQ_FILE_ENCRYPTED) == 0
                     && (Block.Flags & MPQ_FILE_COMPRESS ) != 0)
                    {
                        try
                        {
                            Block.CSize = data.ToUInt32(RealFilePos + data.ToInt32(RealFilePos) - 4);
                        }
                        catch
                        {
                            goto Exception;
                        }
                    }
                    else if (tryFindAllKey)
                    {
                        try
                        {
                            int sectorSize = 0x200 << _Archive.Header.SectorSize;
                            byte[] sector;
                            if (data.Length < RealFilePos + sectorSize)
                                 sector = data.SubArray(RealFilePos);
                            else sector = data.SubArray(RealFilePos, sectorSize);
                            uint key = sector.DetectFileKeyBySectorSize(Block.FSize, (uint)sectorSize);
                            sector.DecryptBlock(key - 1);
                            int start = sector.ToInt32();
                            if (start < 8) goto Exception;
                            Block.CSize = sector.ToUInt32(start - 4);
                        }
                        catch
                        {
                            goto Exception;
                        }
                    }
                }
                goto AddBlock;
                Exception: Block = new MPQBlock(null);  // BlockTable은 HashTable로 인하여 Index값을 무조건 맞춰야하므로, 빈값을 집어넣어주기 위해서 goto문으로 예외처리
                AddBlock: Block.Table = this;
                Add(Block);
            }
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQBlockTable"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQBlockTable"/>을 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value, Stream data = null, bool tryFindAllKey = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Clear();
            for (int i = 0; i < value.Length / 0x10; i++)
            {
                MPQBlock Block;
                if (!_Archive.HashTable.Exists((uint)i)) goto Exception;
                uint FSize = BitConverter.ToUInt32(value, i * 0x10 + 0x8);
                uint Flags = BitConverter.ToUInt32(value, i * 0x10 + 0xC);
                if (FSize >= 0x10000000 || (Flags & MPQ_FILE_EXISTS) == 0) goto Exception;
                Block = new MPQBlock
                {
                    FilePos = BitConverter.ToInt32(value, i * 0x10),
                    CSize = BitConverter.ToUInt32(value, i * 0x10 + 0x4),
                    FSize = FSize,
                    Flags = Flags
                };
                if (data != null)
                {
                    int RealFilePos = _Archive.MPQHeaderPos + Block.FilePos;
                    if ((Block.Flags & MPQ_FILE_ENCRYPTED) == 0
                     && (Block.Flags & MPQ_FILE_COMPRESS) != 0)
                    {
                        try
                        {
                            Block.CSize = data.ToUInt32(RealFilePos + data.ToInt32(RealFilePos) - 4);
                        }
                        catch
                        {
                            goto Exception;
                        }
                    }
                    else if (tryFindAllKey)
                    {
                        try
                        {
                            int sectorSize = 0x200 << _Archive.Header.SectorSize;
                            byte[] sector;
                            if (data.Length < RealFilePos + sectorSize)
                                sector = data.SubArray(RealFilePos);
                            else sector = data.SubArray(RealFilePos, sectorSize);
                            uint key = sector.DetectFileKeyBySectorSize(Block.FSize, (uint)sectorSize);
                            sector.DecryptBlock(key - 1);
                            int start = sector.ToInt32();
                            if (start < 8) goto Exception;
                            Block.CSize = sector.ToUInt32(start - 4);
                        }
                        catch
                        {
                            goto Exception;
                        }
                    }
                }
                goto AddBlock;
            Exception: Block = new MPQBlock(null);  // BlockTable은 HashTable로 인하여 Index값을 무조건 맞춰야하므로, 빈값을 집어넣어주기 위해서 goto문으로 예외처리
            AddBlock: Block.Table = this;
                Add(Block);
            }
        }

        /// <summary>
        /// <see cref="MPQBlockTable"/>의 순서를 뒤섞습니다.
        /// </summary>
        public void Shuffle()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            for (int i = Common.ShuffleLoopCount; i >= 0; i--)
            {
                int idx1 = Count.GetRandom();
                int idx2 = Count.GetRandom();

                MPQBlock Block = this[idx1];
                this[idx1] = this[idx2];
                this[idx2] = Block;
                foreach (var Hash in _Archive.HashTable)
                    if (Hash.BlockIndex == idx1)
                        Hash.BlockIndex = (uint)idx2;
                    else if (Hash.BlockIndex == idx2)
                        Hash.BlockIndex = (uint)idx1;
            }
        }
    }
}
