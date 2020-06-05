using System.IO;

using static CirnoLib.MPQ.Constant;

namespace CirnoLib.MPQ.Struct
{
    public sealed class W3MHeader : IArrayable
    {
        // .w3m and .w3x files are Warcraft III Scenario Maps, which are like MPQ Files.They both takes a 512 bytes
        // header format, but for some authentification, .w3x files takes an extra 260 bytes in the header.

        // Here is the header file of .w3m files :
        // char[4]: "HM3W"
        // int: unknown
        // string: map name
        // int: map flags
        // 0x0001: hide minimap in preview screens
        // 0x0002: modify ally priorities
        // 0x0004: melee map
        // 0x0008: playable map size was large and has never been reduced to medium
        // 0x0010: masked area are partially visible
        // 0x0020: fixed player setting for custom forces
        // 0x0040: use custom forces
        // 0x0080: use custom techtree
        // 0x0100: use custom abilities
        // 0x0200: use custom upgrades
        // 0x0400: map properties menu opened at least once since map creation
        // 0x0800: show water waves on cliff shores
        // 0x1000: show water waves on rolling shores
        // int: max number of players

        #region [    Variables    ]
        /// <summary>
        /// 워크래프트 3 맵의 이름입니다.
        /// UTF-8로 인코딩되어 있습니다.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 워크래프트 3 맵의 속성을 나타내는 값 입니다.
        /// </summary>
        public int Flags { get; set; }

        /// <summary>
        /// 워크래프트 3 맵의 최대 플레이어 수를 나타내는 값 입니다.
        /// </summary>
        public int Players { get; set; }
        #endregion

        #region [    Constructor    ]
        public W3MHeader()
        {
            Name = "";
            Flags = 0x0000;
            Players = 1;
        }

        public W3MHeader(byte[] data)
        {
            Parse(data);
        }

        public W3MHeader(Stream stream)
        {
            Parse(stream);
        }
        #endregion

        /// <summary>
        /// <see cref="W3MHeader"/>를 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="W3MHeader"/>의 바이트 배열입니다.</returns>
        public byte[] ToArray()
        {
            using (ByteStream bs = new ByteStream())
            {
                bs.Write(W3M_HEADER_SIGNATURE);
                bs.WriteEmpty(4);
                bs.Write(Name);
                bs.Write(Flags);
                bs.Write(Players);
                return bs.ToArray();
            }
            //List<byte> buffer = new List<byte>();
            //buffer.AddRange(BitConverter.GetBytes(W3M_HEADER_SIGNATURE));
            //for (int i = 0; i < 4; i++)
            //    buffer.Add(0x00);
            //buffer.AddRange(Encoding.UTF8.GetBytes(Name));
            //buffer.Add(0x00);
            //buffer.AddRange(BitConverter.GetBytes(Flags));
            //buffer.AddRange(BitConverter.GetBytes(Players));
            //return buffer.ToArray();
        }

        public void Parse(byte[] value)
        {
            using (ByteStream bs = new ByteStream(value))
            {
                bs.Skip(8);
                Name = bs.ReadString();
                Flags = bs.ReadInt32();
                Players = bs.ReadInt32();
            }
            //List<byte> buffer = new List<byte>();
            //int index = 8;
            //while (value[index] != 0x00)
            //    buffer.Add(value[index++]);
            //Name = Encoding.UTF8.GetString(buffer.ToArray());
            //Flags = BitConverter.ToInt32(value, ++index);
            //Players = BitConverter.ToInt32(value, index + 4);
        }

        public void Parse(Stream stream)
        {
            long Position = stream.Position;
            stream.Position = 8;
            Name = stream.ReadString();
            Flags = stream.ReadInt32();
            Players = stream.ReadInt32();
            stream.Position = Position;
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
