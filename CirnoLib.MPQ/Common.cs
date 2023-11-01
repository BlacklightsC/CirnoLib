using System;
using System.Text;
using System.Collections.Generic;

namespace CirnoLib.MPQ
{
    public static class Common
    {
        public static int ShuffleLoopCount = 0x8000;
        private static readonly uint[] cryptTable = BuildEncryptionTable();
        private static uint[] BuildEncryptionTable()
        {
            uint seed = 0x00100001;

            uint[] cryptTable = new uint[0x500];

            for (uint idx1 = 0; idx1 < 0x100; idx1++)
                for (uint idx2 = idx1, i = 0; i < 5; i++, idx2 += 0x100)
                    for (int j = 1; j >= 0; j--)
                    {
                        seed = (seed * 125 + 3) % 0x2AAAAB;
                        cryptTable[idx2] |= (seed & 0xFFFF) << (j * 0x10);
                    }

            return cryptTable;
        } 

        #region [    Hash String    ]
        public static uint HashString(this string Text, uint HashType)
        {
            uint seed1 = 0x7FED7FED, seed2 = 0xEEEEEEEE;
            Text = Text.ToUpper();
            
            if (HashType == 3)
            {
                int index = Text.LastIndexOf('\\');
                if (index != -1) Text = Text.Substring(index + 1);
            }
            byte[] bytes = Text.GetBytes();
            for (int i = 0; i < bytes.Length; i++)
            unchecked
            {
                seed1 = cryptTable[(HashType << 8) + bytes[i]] ^ (seed1 + seed2);
                seed2 += bytes[i] + seed1 + (seed2 << 5) + 3;
            }
            return seed1;
        }
        public static uint HashString(this byte[] Array, uint HashType) => HashString(Encoding.UTF8.GetString(Array), HashType);
        public static uint HashString(this List<byte> List, uint HashType) => HashString(Encoding.UTF8.GetString(List.ToArray()), HashType);
        #endregion
        #region [    Get HashTable Index    ]
        public static int GetHashTableIndex(uint FileOffset, int TableSize, int LoopCount)
        {
            if (TableSize <= 0) throw new Exception("크기가 너무 작습니다!");
            int Index = (int)(FileOffset % TableSize);
            for (int i = 1; i < LoopCount; i++)
                Index = ++Index % TableSize;
            return Index;
        }
        public static int GetHashTableIndex(this byte[] FileName, int TableSize, int LoopCount) => GetHashTableIndex(FileName.HashString(0), TableSize, LoopCount);
        public static int GetHashTableIndex(this string FileName, int TableSize, int LoopCount) => GetHashTableIndex(FileName.HashString(0), TableSize, LoopCount);
        public static int GetHashTableIndex(this List<byte> FileName, int TableSize, int LoopCount) => GetHashTableIndex(FileName.HashString(0), TableSize, LoopCount);
        #endregion
        #region [    Encrypt Block    ]
        public static void EncryptBlock(this byte[] Block, uint Key)
        {
            if (Block == null) throw new ArgumentNullException("Block");
            uint seed = 0xEEEEEEEE;
            int sequence = Block.Length >> 2;
            for (var i = 0; i < sequence; i++)
            {
                seed += cryptTable[0x400 + (Key & 0xFF)];
                uint value = BitConverter.ToUInt32(Block, i << 2);
                Array.ConstrainedCopy(BitConverter.GetBytes(value ^ (Key + seed)), 0, Block, i << 2, 4);
                Key = ((~Key << 0x15) + 0x11111111) | (Key >> 0xB);
                seed += value + (seed << 5) + 3;
            }
        }
        public static void EncryptBlock(this byte[] Block, string Key) => EncryptBlock(Block, HashString(Key, 3));
        public static void EncryptBlock(this byte[] Block, byte[] key) => EncryptBlock(Block, HashString(key, 3));
        public static void EncryptBlock(this byte[] Block, List<byte> key) => EncryptBlock(Block, HashString(key, 3));
        #endregion
        #region [    Decrypt Block    ]
        public static void DecryptBlock(this byte[] Block, uint Key)
        {
            if (Block == null) throw new ArgumentNullException("Block");
            uint seed = 0xEEEEEEEE;
            int sequence = Block.Length >> 2;
            for (int i = 0; i < sequence; i++)
            {
                seed += cryptTable[0x400 + (Key & 0xFF)];
                uint value = BitConverter.ToUInt32(Block, i << 2) ^ (Key + seed);
                Block.Write(i << 2, value);
                Key = ((~Key << 0x15) + 0x11111111) | (Key >> 0xB);
                seed += value + (seed << 5) + 3;
            }
        }
        public static void DecryptBlock(this byte[] Block, string Key) => DecryptBlock(Block, HashString(Key, 3));
        public static void DecryptBlock(this byte[] Block, byte[] key) => DecryptBlock(Block, HashString(key, 3));
        public static void DecryptBlock(this byte[] Block, List<byte> key) => DecryptBlock(Block, HashString(key, 3));
        #endregion

        /// <summary>
        /// <paramref name="data"/>의 파일 키를 역산해서 가져옵니다.
        /// </summary>
        /// <param name="data">파일 키를 역산할 데이터입니다.</param>
        /// <param name="FileSize"><paramref name="data"/>의 원본 파일 크기입니다.</param>
        /// <param name="SectorSize"><paramref name="data"/>의 섹터 크기입니다. (0x200 &lt;&lt; Header.SectorSize)</param>
        /// <returns>파일 키 입니다.</returns>
        public static uint DetectFileKeyBySectorSize(this byte[] data, uint FileSize, uint SectorSize)
        {
            if (SectorSize < 8) throw new KeyNotFoundException("섹터의 크기는 8바이트 이상이여야 합니다.");
            if (data == null) throw new ArgumentNullException("파일은 null이 될 수 없습니다.");

            uint Decrypted0 = ((FileSize - 1) / SectorSize + 2) * 4;
            uint Decrypted1 = Decrypted0 + SectorSize;
            uint[] EncryptedData = new uint[2];

            for (int i = 0; i < 2; i++)
                EncryptedData[i] = data.ToUInt32(i * 4);

            uint Key1PlusKey2 = (EncryptedData[0] ^ Decrypted0) - 0xEEEEEEEE;

            for (int i = 0; i < 0x100; i++)
            {
                uint Key1 = Key1PlusKey2 - cryptTable[0x400 + i];
                uint Key2 = 0xEEEEEEEE + cryptTable[0x400 + (Key1 & 0xFF)];

                if (Decrypted0 == (EncryptedData[0] ^ (Key1 + Key2)))
                {
                    uint SaveKey = Key1 + 1;

                    Key1 = ((~Key1 << 0x15) + 0x11111111) | (Key1 >> 0x0B);
                    Key2 += Decrypted0 + (Key2 << 5) + 3 + cryptTable[0x400 + (Key1 & 0xFF)];

                    if (Decrypted1 >= (EncryptedData[1] ^ (Key1 + Key2)))
                        return SaveKey;
                }
            }
            throw new KeyNotFoundException("파일 키를 발견하지 못하였습니다.");
        }

        public static int BitUpper(this int value)
        {
            if (value == 0) return 0;
            string bit = Convert.ToString(value, 2);
            string newBit = "1".PadRight(bit.Length + 1, '0');
            if (bit == newBit) return value;
            return Convert.ToInt32(newBit, 2);
        }

        #region [    Is Safe Index    ]
        /// <summary>
        /// 정수를 확인하여, 정상적인 인덱스인지 확인합니다.
        /// </summary>
        /// <param name="Index">확인할 인덱스입니다.</param>
        /// <returns>사이의 수일 경우 참을 반환합니다.</returns>
        public static bool IsSafeIndex(this uint Index)
        {
            if ((Index & 0x40000000) != 0)
                Index ^= 0x40000000;
            else if ((Index & 0x80000000) != 0)
                Index ^= 0x80000000;
            if (Index < 0x8000)
                return true;
            return false;
        }
        /// <summary>
        /// 정수를 확인하여, 정상적인 인덱스인지 확인합니다.
        /// </summary>
        /// <param name="Index">확인할 인덱스입니다.</param>
        /// <returns>사이의 수일 경우 참을 반환합니다.</returns>
        public static bool IsSafeIndex(this uint Index, out uint SafeIndex)
        {
            SafeIndex = Index;
            if ((Index & 0x80000000) != 0)
                SafeIndex ^= 0x80000000;
            if ((Index & 0x40000000) != 0)
                SafeIndex ^= 0x40000000;
            if ((Index & 0x20000000) != 0)
                SafeIndex ^= 0x20000000;
            if ((Index & 0x10000000) != 0)
                SafeIndex ^= 0x10000000;
            if (SafeIndex < 0x8000)
                return true;
            return false;
        }
        #endregion
    }
}
