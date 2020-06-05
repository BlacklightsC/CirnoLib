using System;

using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class MPQBlock : IArrayable
    {
        private int _FilePos;
        /// <summary>
        /// <see cref="MPQHeader"/>의 시작 부분을 기준으로 파일 데이터의 시작 오프셋입니다.
        /// </summary>
        public int FilePos {
            get => _FilePos;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");
                
                _FilePos = value;
            }
        }

        private uint _CSize;
        /// <summary>
        /// 압축된 파일의 크기
        /// </summary>
        public uint CSize {
            get => _CSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _CSize = value;
            }
        }

        private uint _FSize;
        /// <summary>
        /// 압축되지 않은 파일의 크기
        /// </summary>
        public uint FSize {
            get => _FSize;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _FSize = value;
            }
        }

        private uint _Flags;
        /// <summary>
        /// 파일에 대한 설정
        /// </summary>
        public uint Flags {
            get => _Flags;
            set {
                if (Initialized && ReadOnly) throw new AccessViolationException("읽기 전용으로 초기화되었으므로 쓰기 작업을 진행할 수 없습니다.");

                _Flags = value;
            }
        }

        public bool ReadOnly { get => Table == null ? false : Table.ReadOnly; }
        internal bool Initialized { get => Table == null ? false : Table.Initialized; }

        /// <summary>
        /// 이 인스턴트를 소유하고 있는 <see cref="MPQBlockTable"/>인스턴스입니다.
        /// </summary>
        public MPQBlockTable Table { get; internal set; }

        public MPQBlock()
        {
            FilePos = 0x00000000;
            CSize = 0x00000000;
            FSize = 0x00000000;
            Flags = MPQ_FILE_EXISTS;
        }
        public MPQBlock(byte[] value)
        {
            if (value == null)
            {
                FilePos = 0x00000000;
                CSize = 0x00000000;
                FSize = 0x00000000;
                Flags = 0x00000000;
            }
            else Parse(value);
        }

        /// <summary>
        /// <see cref="MPQBlock"/>을 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="MPQBlock"/>의 바이트 배열입니다.</returns>
        public byte[] ToArray()
        {
            byte[] buffer = new byte[0x10];
            buffer.Write(0x0, FilePos);
            buffer.Write(0x4, CSize);
            buffer.Write(0x8, FSize);
            buffer.Write(0xC, Flags);
            return buffer;
        }

        /// <summary>
        /// 바이트 배열에서 <see cref="MPQBlock"/>에 대한 정보를 읽어옵니다.
        /// </summary>
        /// <param name="value"><see cref="MPQBlock"/>을 포함하고 있는 바이트 배열입니다.</param>
        public void Parse(byte[] value)
        {
            FilePos = BitConverter.ToInt32(value, 0);
            CSize = BitConverter.ToUInt32(value, 0x4);
            FSize = BitConverter.ToUInt32(value, 0x8);
            Flags = BitConverter.ToUInt32(value, 0xC);
        }

        /// <summary>
        /// <see cref="MPQBlock"/>의 단순 복사본을 만듭니다.
        /// </summary>
        /// <returns><see cref="MPQBlock"/>의 단순 복사본입니다.</returns>
        public MPQBlock Clone() => MemberwiseClone() as MPQBlock;

        public void RemoveSelfInTable()
        {
            if (Table != null)
            {
                FilePos = 0x00000000;
                CSize = 0x00000000;
                FSize = 0x00000000;
                Flags = 0x00000000;
            }
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
