using System;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQHash : IArrayable
    {
        private uint _Name1;
        /// <summary>
        /// 전체 파일 이름의 해시 값 (파트 A)
        /// </summary>
        public uint Name1{
            get => _Name1;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _Name1 = value;
            }
        }

        private uint _Name2;
        /// <summary>
        /// 전체 파일 이름의 해시 값 (파트 B)
        /// </summary>
        public uint Name2 {
            get => _Name2;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _Name2 = value;
            }
        }

        private ushort _Locale;
        /// <summary>
        /// 파일의 언어. 이것은 Windows LANGID 데이터 유형이며 동일한 값을 사용합니다.
        /// 0은 시스템 언어를 나타내거나 파일이 언어 중립적임을 나타냅니다.
        /// </summary>
        public ushort Locale {
            get => _Locale;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _Locale = value;
            }
        }

        private ushort _Platform;
        /// <summary>
        /// 파일이 사용되는 플랫폼. 0은 기본 플랫폼을 나타냅니다.
        /// 다른 값은 관찰되지 않았습니다.
        /// </summary>
        public ushort Platform {
            get => _Platform;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _Platform = value;
            }
        }

        private uint _BlockIndex;
        /// <summary>
        /// 해시 테이블 항목이 유효할 경우 사용되는 파일의 블록 테이블에 대한 색인입니다.
        /// 그렇지 않은 경우 다음 두 값 중 하나를 선택합니다:
        ///  - 0xFFFFFFFF: 해시 테이블 항목은 비어 있으며 항상 비어 있습니다.
        ///                주어진 파일에 대한 검색을 종료합니다.
        ///  - 0xFFFFFFFE: 해시 테이블 항목은 비어 있지만 특정 시점에서 유효합니다.(삭제 된 파일)
        ///                지정된 파일에 대한 검색을 종료하지 않습니다.
        /// </summary>
        public uint BlockIndex {
            get => _BlockIndex;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _BlockIndex = value;
            }
        }

        public bool ReadOnly { get => Table == null ? false : Table.ReadOnly; }
        internal bool Initialized { get => Table == null ? false : Table.Initialized; }

        /// <summary>
        /// 이 인스턴스를 소유하고 있는 <see cref="MPQHashTable"/>인스턴스입니다.
        /// </summary>
        public MPQHashTable Table { get; internal set; }

        public bool IsExist { get => Name1 != 0xFFFFFFFF 
                                  && Name2 != 0xFFFFFFFF 
                                  && BlockIndex.IsSafeIndex(); }

        /// <summary>
        /// <see cref="MPQHash"/>의 인스턴스를 기본 값으로 초기화합니다.
        /// </summary>
        public MPQHash()
        {
            Name1 = 0xFFFFFFFF;
            Name2 = 0xFFFFFFFF;
            Locale = 0x0000;
            Platform = 0x0000;
            BlockIndex = 0xFFFFFFFE;
        }
        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHash"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQHash"/>를 포함하고 있는 바이트 배열입니다.</param>
        public MPQHash(byte[] value)
        {
            if (value == null)
                SetEmpty();
            else Parse(value);
        }

        /// <summary>
        /// <see cref="MPQHash"/>의 인스턴스를 지정하는 이름으로 초기화합니다.
        /// </summary>
        public MPQHash(string name, string Null) : this() => SetName(name);

        /// <summary>
        /// <see cref="MPQHash"/>를 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="MPQHash"/>의 바이트 배열입니다.</returns>
        public byte[] ToArray()
        {
            byte[] buffer = new byte[0x10];
            buffer.Write(0x0, Name1);
            buffer.Write(0x4, Name2);
            buffer.Write(0x8, Locale);
            buffer.Write(0xA, Platform);
            buffer.Write(0xC, BlockIndex);
            return buffer;
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQHash"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQHash"/>를 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value)
        {
            Name1 = BitConverter.ToUInt32(value, 0x0);
            Name2 = BitConverter.ToUInt32(value, 0x4);
            Locale = BitConverter.ToUInt16(value, 0x8);
            Platform = BitConverter.ToUInt16(value, 0xA);
            BlockIndex = BitConverter.ToUInt32(value, 0xC);
        }

        public void SetEmpty()
        {
            Name1 = 0xFFFFFFFF;
            Name2 = 0xFFFFFFFF;
            Locale = 0xFFFF;
            Platform = 0xFFFF;
            BlockIndex = 0xFFFFFFFE;
        }

        /// <summary>
        /// <see cref="MPQHash"/>의 단순 복사본을 만듭니다.
        /// </summary>
        /// <returns><see cref="MPQHash"/>의 단순 복사본입니다.</returns>
        public MPQHash Clone() => MemberwiseClone() as MPQHash;

        public void SetName(byte[] name)
        {
            Name1 = name.HashString(1);
            Name2 = name.HashString(2);
        }
        public void SetName(string name)
        {
            Name1 = name.HashString(1);
            Name2 = name.HashString(2);
        }

        public void RemoveSelfInTable()
        {
            if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

            if (Table != null)
                Table.Remove(this);
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
