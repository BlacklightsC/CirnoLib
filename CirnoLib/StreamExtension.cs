using System;
using System.IO;
using System.Text;

namespace CirnoLib
{
    public static class StreamExtension
    {
        public static string ReadNullTerminateString(this Stream stream, byte endByte = 0, bool rewindLastByte = false)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    byte piece = (byte)stream.ReadByte();
                    if (piece == endByte) break;
                    ms.WriteByte(piece);
                }
                if (rewindLastByte) stream.Seek(-1, SeekOrigin.Current);
                return ms.ToArray().GetString();
            }
        }

        #region [    Read Variables    ]
        public static sbyte ReadSByte(this Stream stream, int index = -1)
        {
            if (index >= 0)
            {
                long Position = stream.Position;
                stream.Position = index;
                sbyte value = (sbyte)stream.ReadByte();
                stream.Position = Position;
                return value;
            }
            else
            {
                return (sbyte)stream.ReadByte();
            }
        }
        public static byte ReadByte(this Stream stream, int index = -1)
        {
            if (index >= 0)
            {
                long Position = stream.Position;
                stream.Position = index;
                byte value = (byte)stream.ReadByte();
                stream.Position = Position;
                return value;
            }
            else
            {
                return (byte)stream.ReadByte();
            }
        }
        public static byte[] ReadBytes(this Stream stream, int length, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, length);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, length);
                stream.Seek(length, SeekOrigin.Current);
            }
            return value;
        }
        public static char ReadChar(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index,2);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 2);
                stream.Seek(2, SeekOrigin.Current);
            }
            return value.ToChar();
        }
        public static bool ReadBoolean(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 1);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 1);
                stream.Seek(1, SeekOrigin.Current);
            }
            return value.ToBoolean();
        }
        public static short ReadInt16(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 2);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 2);
                stream.Seek(2, SeekOrigin.Current);
            }
            return value.ToInt16();
        }
        public static int ReadInt32(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 4);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 4);
                stream.Seek(4, SeekOrigin.Current);
            }
            return value.ToInt32();
        }
        public static long ReadInt64(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 8);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 8);
                stream.Seek(8, SeekOrigin.Current);
            }
            return value.ToInt64();
        }
        public static ushort ReadUInt16(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 2);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 2);
                stream.Seek(2, SeekOrigin.Current);
            }
            return value.ToUInt16();
        }
        public static uint ReadUInt32(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 4);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 4);
                stream.Seek(4, SeekOrigin.Current);
            }
            return value.ToUInt32();
        }
        public static ulong ReadUInt64(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 8);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 8);
                stream.Seek(8, SeekOrigin.Current);
            }
            return value.ToUInt64();
        }
        public static float ReadSingle(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 4);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 4);
                stream.Seek(4, SeekOrigin.Current);
            }
            return value.ToSingle();
        }
        public static double ReadDouble(this Stream stream, int index = -1)
        {
            byte[] value;
            if (index >= 0)
            {
                value = stream.SubArray(index, 8);
            }
            else
            {
                value = stream.SubArray((int)stream.Position, 8);
                stream.Seek(8, SeekOrigin.Current);
            }
            return value.ToDouble();
        }
        public static string ReadString(this Stream stream, byte endByte = 0, int index = -1)
        {
            if (index >= 0)
            {
                long Position = stream.Position;
                stream.Position = index;
                string value = stream.ReadNullTerminateString(endByte);
                stream.Position = Position;
                return value;
            }
            else
            {
                return stream.ReadNullTerminateString(endByte);
            }
        }
        #endregion

        #region [    Write Variables    ]
        public static void Write(this Stream stream, params byte[] source) => stream.Write(source, 0, source.Length);
        public static void Write(this Stream stream, bool source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, char source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, short source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, int source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, long source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, ushort source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, uint source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, ulong source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, float source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, double source) => Write(stream, BitConverter.GetBytes(source));
        public static void Write(this Stream stream, string source) => Write(stream, Encoding.UTF8.GetBytes(source));
        public static void Write(this Stream stream, IArrayable source) => Write(stream, source.ToArray());
        public static void Write(this Stream stream, IArrayable[] source) => Write(stream, source.ToArray());
        public static void Write(this Stream stream, MemoryStream source) => Write(stream, source.ToArray());
        #endregion

        public static void Skip(this Stream stream, int Length) => stream.Seek(Length, SeekOrigin.Current);

        public static void WriteEmpty(this Stream stream, int Length)
        {
            if (Length <= 0) return;
            stream.SetLength(stream.Length + Length);
            stream.Seek(0, SeekOrigin.End);
        }

        #region [    Sub Array    ]
        /// <summary>
        /// 지정된 바이트 배열을 지정된 인덱스부터 복사하여 반환합니다.
        /// </summary>
        /// <param name="array">데이터를 받아올 바이트 배열입니다.</param>
        /// <param name="StartIndex">인덱스의 시작 위치입니다.</param>
        /// <returns>바이트 배열입니다.</returns>
        public static byte[] SubArray(this Stream stream, int StartIndex)
        {
            byte[] buffer = new byte[stream.Length - StartIndex];
            long OriginPosition = stream.Position;
            stream.Position = StartIndex;
            stream.Read(buffer, 0, buffer.Length);
            stream.Position = OriginPosition;
            return buffer;
        }
        /// <summary>
        /// 지정된 바이트 배열을 지정된 인덱스부터 지정된 길이만큼 복사하여 반환합니다.
        /// </summary>
        /// <param name="array">데이터를 받아올 바이트 배열입니다.</param>
        /// <param name="StartIndex">인덱스의 시작 위치입니다.</param>
        /// <param name="Length">복사할 배열의 길이입니다.</param>
        /// <returns>바이트 배열입니다.</returns>
        public static byte[] SubArray(this Stream stream, int StartIndex, int Length)
        {
            byte[] buffer = new byte[Length];
            long OriginPosition = stream.Position;
            stream.Position = StartIndex;
            stream.Read(buffer, 0, Length);
            stream.Position = OriginPosition;
            return buffer;
        }
        #endregion
    }
}
