using System;
using System.IO;
using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQHeader : IArrayable
    {
        private uint _HeaderSize;
        /// <summary>
        /// Size of the archive header
        /// </summary>
        public uint HeaderSize {
            get => _HeaderSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _HeaderSize = value;
            }
        }

        private uint _ArchiveSize;
        /// <summary>
        /// Size of MPQ archive
        /// This field is deprecated in the Burning Crusade MoPaQ format, and the size of the archive
        /// is calculated as the size from the beginning of the archive to the end of the hash table,
        /// block table, or extended block table (whichever is largest).
        /// </summary>
        public uint ArchiveSize {
            get => _ArchiveSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _ArchiveSize = value;
            }
        }

        private ushort _FormatVersion;
        /// <summary>
        /// 0 = Format 1 (up to The Burning Crusade)
        /// 1 = Format 2 (The Burning Crusade and newer)
        /// 2 = Format 3 (WoW - Cataclysm beta or newer)
        /// 3 = Format 4 (WoW - Cataclysm beta or newer)
        /// </summary>
        public ushort FormatVersion {
            get => _FormatVersion;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _FormatVersion = value;
            }
        }

        private ushort _SectorSize;
        /// <summary>
        /// Power of two exponent specifying the number of 512-byte disk sectors in each logical sector
        /// in the archive. The size of each logical sector in the archive is 512 * 2^SectorSize.
        /// </summary>
        public ushort SectorSize {
            get => _SectorSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _SectorSize = value;
            }
        }

        private int _HashTablePos;
        /// <summary>
        /// Offset to the beginning of the hash table, relative to the beginning of the archive.
        /// </summary>
        public int HashTablePos {
            get => _HashTablePos;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _HashTablePos = value;
            }
        }

        private int _BlockTablePos;
        /// <summary>
        /// Offset to the beginning of the block table, relative to the beginning of the archive.
        /// </summary>
        public int BlockTablePos {
            get => _BlockTablePos;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _BlockTablePos = value;
            }
        }

        private uint _HashTableSize;
        /// <summary>
        /// Number of entries in the hash table. Must be a power of two, and must be less than 2^16 for
        /// the original MoPaQ format, or less than 2^20 for the Burning Crusade format.
        /// </summary>
        public uint HashTableSize {
            get => _HashTableSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _HashTableSize = value;
            }
        }

        private uint _BlockTableSize;
        /// <summary>
        /// Number of entries in the block table
        /// </summary>
        public uint BlockTableSize {
            get => _BlockTableSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _BlockTableSize = value;
            }
        }

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

        public MPQHeader(MPQArchive archive)
        {
            _Archive = archive;
            HeaderSize = 0x00000020;
            ArchiveSize = 0x00000000;
            FormatVersion = 0x0000;
            SectorSize = 0x0003;
            HashTablePos = 0x00000000;
            BlockTablePos = 0x00000000;
            HashTableSize = 0x00000010;
            BlockTableSize = 0x00000010;
        }

        internal MPQHeader(MPQArchive archive, byte[] data)
        {
            _Archive = archive;
            archive.MPQHeaderPos = ParseMap(data);
        }

        internal MPQHeader(MPQArchive archive, Stream data)
        {
            _Archive = archive;
            archive.MPQHeaderPos = ParseMap(data);
        }

        /// <summary>
        /// <see cref="MPQHeader"/>를 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="MPQHeader"/>의 바이트 배열입니다.</returns>
        public byte[] ToArray()
        {
            byte[] buffer = new byte[0x20];
            buffer.Write(0x0, MPQ_HEADER_SIGNATURE);
            buffer.Write(0x4, HeaderSize);
            buffer.Write(0x8, ArchiveSize);
            buffer.Write(0xC, FormatVersion);
            buffer.Write(0xE, SectorSize);
            buffer.Write(0x10, HashTablePos);
            buffer.Write(0x14, BlockTablePos);
            buffer.Write(0x18, HashTableSize);
            buffer.Write(0x1C, BlockTableSize);
            return buffer;
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHeader"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQHeader"/>를 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value)
        {
            HeaderSize = 0x20; //BitConverter.ToUInt32(value, 0x4);
            ArchiveSize = BitConverter.ToUInt32(value, 0x8);
            FormatVersion = BitConverter.ToUInt16(value, 0xC);
            SectorSize = BitConverter.ToUInt16(value, 0xE);
            HashTablePos = BitConverter.ToInt32(value, 0x10);
            BlockTablePos = BitConverter.ToInt32(value, 0x14);
            HashTableSize = BitConverter.ToUInt32(value, 0x18);
            if (HashTableSize > 0x8000) HashTableSize = 0x8000;
            BlockTableSize = BitConverter.ToUInt32(value, 0x1C);
            if (BlockTableSize > 0x8000) BlockTableSize = 0x8000;
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHeader"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="Map"><see cref="MPQHeader"/>를 포함하고 있는 바이트 배열입니다.</param>
        /// <return><see cref="MPQHeader"/>가 바이트 배열에서 발견된 인덱스 입니다. 발견하지 못할 경우 -1을 반환합니다.</return>
        public int ParseMap(byte[] Map)
        {
            int HeaderPos = -1, maxSequence = Map.Length / 0x200;
            if (Map.Length % 0x200 >= 0x20) maxSequence++;
            for (int i = 0; i < maxSequence; i++)
                if (BitConverter.ToUInt32(Map, 0x200 * i) == MPQ_HEADER_SIGNATURE)
                {
                    HeaderPos = 0x200 * i;
                    HeaderSize = 0x20; //BitConverter.ToUInt32(Map, HeaderPos + 0x4);
                    ArchiveSize = BitConverter.ToUInt32(Map, HeaderPos + 0x8);
                    FormatVersion = BitConverter.ToUInt16(Map, HeaderPos + 0xC);
                    SectorSize = BitConverter.ToUInt16(Map, HeaderPos + 0xE);
                    HashTablePos = BitConverter.ToInt32(Map, HeaderPos + 0x10);
                    BlockTablePos = BitConverter.ToInt32(Map, HeaderPos + 0x14);
                    HashTableSize = BitConverter.ToUInt32(Map, HeaderPos + 0x18);
                    if (HashTableSize > 0x8000) HashTableSize = 0x8000;
                    BlockTableSize = BitConverter.ToUInt32(Map, HeaderPos + 0x1C);
                    if (BlockTableSize > 0x8000) BlockTableSize = 0x8000;
                    break;
                }
            return HeaderPos;
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHeader"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="Map"><see cref="MPQHeader"/>를 포함하고 있는 스트림입니다.</param>
        /// <return><see cref="MPQHeader"/>가 바이트 배열에서 발견된 인덱스 입니다. 발견하지 못할 경우 -1을 반환합니다.</return>
        public int ParseMap(Stream Map)
        {
            int HeaderPos = -1, maxSequence = (int)Map.Length / 0x200;
            if (Map.Length % 0x200 >= 0x20) maxSequence++;
            for (int i = 0; i < maxSequence; i++)
            {
                if (BitConverter.ToUInt32(Map.SubArray(HeaderPos = 0x200 * i, 4), 0) == MPQ_HEADER_SIGNATURE)
                {
                    Parse(Map.SubArray(HeaderPos, 0x20));
                    break;
                }
            }
            return HeaderPos;
        }

        /// <summary>
        /// <see cref="MPQHeader"/>의 단순 복사본을 만듭니다.
        /// </summary>
        /// <returns><see cref="MPQHeader"/>의 단순 복사본입니다.</returns>
        public MPQHeader Clone() => MemberwiseClone() as MPQHeader;

        public void UpdateSize()
        {
            HashTablePos = _Archive.Files.LeastSize + 0x20;
            HashTableSize = (uint)_Archive.HashTable.Count;
            BlockTablePos = HashTablePos + (int)HashTableSize * 0x10;
            BlockTableSize = (uint)_Archive.BlockTable.Count;
        }

#if DEBUG
        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (var item in ToArray())
                builder.AppendFormat("{0:X2} ", item);
            return builder.ToString().Trim();
        }
#endif
    }
}
