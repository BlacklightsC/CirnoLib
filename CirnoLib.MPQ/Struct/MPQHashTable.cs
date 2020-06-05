using System;
using System.IO;
using System.Collections.Generic;

using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQHashTable : List<MPQHash>, IArrayable
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

        public MPQHashTable() { }
        public MPQHashTable(MPQArchive archive) => _Archive = archive;
        public MPQHashTable(MPQArchive archive, byte[] data) : this(archive)
        {
            if (archive.Header == null) throw new NullReferenceException("MPQHeader가 초기화되지 않았습니다.");
            byte[] buffer;
            int HashTablePos = archive.MPQHeaderPos + archive.Header.HashTablePos;
            if (HashTablePos + (int)archive.Header.HashTableSize * 0x10 < data.Length)
                buffer = data.SubArray(HashTablePos, (int)archive.Header.HashTableSize * 0x10);
            else buffer = data.SubArray(HashTablePos);
            buffer.DecryptBlock(MPQ_HASH_KEY);
            Parse(buffer);
        }
        public MPQHashTable(MPQArchive archive, Stream data) : this(archive)
        {
            if (archive.Header == null) throw new NullReferenceException("MPQHeader가 초기화되지 않았습니다.");
            byte[] buffer;
            int HashTablePos = archive.MPQHeaderPos + archive.Header.HashTablePos;
            if (HashTablePos + (int)archive.Header.HashTableSize * 0x10 < data.Length)
                buffer = data.SubArray(HashTablePos, (int)archive.Header.HashTableSize * 0x10);
            else buffer = data.SubArray(HashTablePos);
            buffer.DecryptBlock(MPQ_HASH_KEY);
            Parse(buffer);
        }

        /// <summary>
        /// <see cref="MPQHashTable"/>의 단순 복사본을 만듭니다.
        /// </summary>
        /// <returns><see cref="MPQHashTable"/>의 단순 복사본입니다.</returns>
        public MPQHashTable Clone() => MemberwiseClone() as MPQHashTable;

        /// <summary>
        /// <see cref="MPQHashTable"/>를 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="MPQHashTable"/>의 바이트 배열입니다.</returns>
        public new byte[] ToArray()
        {
            if (Count <= 0) return null;
            using (MemoryStream buffer = new MemoryStream())
            {
                foreach (var item in this)
                    buffer.Write(item.ToArray(), 0, 0x10);
                return buffer.ToArray();
            }
        }

        /// <summary>
        /// 개체를 <see cref="MPQHashTable"/>의 끝 부분에 추가합니다.
        /// </summary>
        /// <param name="item"><see cref="MPQHashTable"/>의 끝에 추가할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Add(MPQHash item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Table = this;
            base.Add(item);
        }

        /// <summary>
        /// <see cref="MPQHashTable"/>의 지정된 인덱스에 요소를 삽입합니다.
        /// </summary>
        /// <param name="Index"><paramref name="item"/> 삽입해야 하는 인덱스(0부터 시작)입니다.</param>
        /// <param name="item">삽입할 개체입니다. 참조 형식에 대해 값은 null이 될 수 있습니다.</param>
        public new void Insert(int Index, MPQHash item)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            item.Table = this;
            base.Insert(Index, item);
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHashTable"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQHashTable"/>을 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value, bool ignoreEmpty = false)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Clear();
            using (ByteStream bs = new ByteStream(value))
                for (int i = 0, length = value.Length / 0x10; i < length; i++)
                {
                    uint Name1 = bs.ReadUInt32();
                    uint Name2 = bs.ReadUInt32();
                    ushort Locale = bs.ReadUInt16();
                    ushort Platform = bs.ReadUInt16();
                    uint BlockIndex = bs.ReadUInt32() % 0x1000000;
                    MPQHash DuplicateHash = Find(Name1, Name2);
                    if (ignoreEmpty && !BlockIndex.IsSafeIndex()) continue;
                    if (DuplicateHash != null)
                        DuplicateHash.SetEmpty();
                    Add(new MPQHash {
                        Name1 = Name1,
                        Name2 = Name2,
                        Locale = Locale,
                        Platform = Platform,
                        BlockIndex = BlockIndex,
                        Table = this
                    });
                }
            #region [    Old Code    ]
            //for (int i = (value.Length / 0x10) - 1; i >= 0; i--)
            //{
            //    uint Name1 = BitConverter.ToUInt32(value, i * 0x10);
            //    uint Name2 = BitConverter.ToUInt32(value, i * 0x10 + 0x4);
            //    uint BlockIndex = BitConverter.ToUInt32(value, i * 0x10 + 0xC) % 0x1000000;
            //    bool Exist = Exists(Name1, Name2);
            //    if (ignoreEmpty && (!BlockIndex.IsSafeIndex() || Exist)) continue;
            //    else if (Exist) goto Exception;
            //    MPQHash Hash = new MPQHash()
            //    {
            //        Name1 = Name1,
            //        Name2 = Name2,
            //        Locale = BitConverter.ToUInt16(value, i * 0x10 + 0x8),
            //        Platform = BitConverter.ToUInt16(value, i * 0x10 + 0xA),
            //        BlockIndex = BlockIndex
            //    };
            //    goto AddHash;
            //    Exception: Hash = new MPQHash(null);    // BlockTable은 HashTable로 인하여 Index값을 무조건 맞춰야하므로, 빈값을 집어넣어주기 위해서 goto문으로 예외처리
            //    AddHash: Hash.Table = this;
            //    Insert(0, Hash);
            //}
            #endregion
        }

        public bool Exists(uint BlockIndex) => Exists(Hash => Hash.BlockIndex == BlockIndex);
        public bool Exists(string Name) => Exists(Name.HashString(1), Name.HashString(2));
        public bool Exists(byte[] Name) => Exists(Name.HashString(1), Name.HashString(2));
        public bool Exists(uint Name1, uint Name2) => Exists(item => item.Name1 == Name1 && item.Name2 == Name2);
        public bool Exists(MPQHash Hash) => Exists(item => item == Hash);

        public MPQHash Find(uint BlockIndex)
        {
            MPQHash Hash = FindLast(item => item.BlockIndex == BlockIndex);
            return Hash?.Table == null ? null : Hash;
        }
        public MPQHash Find(string Name) => Find(Name.HashString(1), Name.HashString(2));
        public MPQHash Find(byte[] Name) => Find(Name.HashString(1), Name.HashString(2));
        public MPQHash Find(uint Name1, uint Name2)
        {
            MPQHash Hash = FindLast(item => item.Name1 == Name1 && item.Name2 == Name2);
            return Hash?.Table == null ? null : Hash;
        }

        /// <summary>
        /// 기본 비교자를 사용하여 전체 <see cref="MPQHashTable"/>의 요소를 정렬합니다.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// 기본 비교자 <see cref="Comparer{T}"/>가 <see cref="IComparable{T}"/> 제네릭
        /// 인터페이스의 구현이나 <see cref="MPQHash"/> 형식의 <see cref="IComparable"/> 인터페이스를 찾지 못한 경우
        /// </exception>
        public new void Sort()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            Sort((A, B) => A.BlockIndex.CompareTo(B.BlockIndex));
        }

        /// <summary>
        /// <see cref="MPQHashTable"/>에서 데이터가 존재하지 않는 배열을 삭제합니다.
        /// </summary>
        public void Wipe()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            RemoveAll(Hash => (Hash.Name1 == 0xFFFFFFFF
                           &&  Hash.Name2 == 0xFFFFFFFF)
                           || !Hash.BlockIndex.IsSafeIndex());
        }

        /// <summary>
        /// <see cref="MPQHashTable"/>에 포함된 데이터의 순서를 무작위로 섞습니다.
        /// </summary>
        public void Shuffle()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            for (int i = Common.ShuffleLoopCount; i >= 0; i--)
            {
                int idx1 = Count.GetRandom();
                int idx2 = Count.GetRandom();
                MPQHash Hash = this[idx1];
                this[idx1] = this[idx2];
                this[idx2] = Hash;
            }
        }

        public void Fill(int Size = -1)
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if (Size == -1)
                Size = Count.BitUpper();
            if (Count < Size)
                while (Size != Count)
                    Add(new MPQHash(null));
        }
    }
}
