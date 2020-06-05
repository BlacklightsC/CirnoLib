using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CirnoLib
{
    public static class ByteArrayExtension
    {
        #region [    Write    ]
        public static void Write(this byte[] array, int index, params byte[] source) => Array.ConstrainedCopy(source, 0, array, index, source.Length);
        public static void Write(this byte[] array, int index, byte[] source, int length) => Array.ConstrainedCopy(source, 0, array, index, length);
        public static void Write(this byte[] array, int index, bool source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 1);
        public static void Write(this byte[] array, int index, char source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 1);
        public static void Write(this byte[] array, int index, short source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 2);
        public static void Write(this byte[] array, int index, int source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 4);
        public static void Write(this byte[] array, int index, long source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 8);
        public static void Write(this byte[] array, int index, ushort source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 2);
        public static void Write(this byte[] array, int index, uint source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 4);
        public static void Write(this byte[] array, int index, ulong source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 8);
        public static void Write(this byte[] array, int index, float source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 4);
        public static void Write(this byte[] array, int index, double source) => Array.ConstrainedCopy(BitConverter.GetBytes(source), 0, array, index, 8);
        public static void Write(this byte[] array, int index, string source) => Write(array, index, Encoding.UTF8.GetBytes(source));
        public static void Write(this byte[] array, int index, IArrayable source) => Write(array, index, source.ToArray());
        public static void Write(this byte[] array, int index, IArrayable[] source) => Write(array, index, source.ToArray());
        public static void Write(this byte[] array, int index, MemoryStream source) => Write(array, index, source.ToArray());
        #endregion

        #region [    Compare    ]
        /// <summary>
        /// 바이트 배열을 비교하여, 값과 길이가 정확히 일치하는지 확인합니다.
        /// </summary>
        public static bool Compare(this byte[] A, params byte[] B) => A.SequenceEqual(B);
        /// <summary>
        /// 바이트 배열을 0부터 지정된 길이까지 비교하여 값이 정확히 일치하는지 확인합니다.
        /// </summary>
        /// <param name="Length">값을 비교할 길이입니다.</param>
        public static bool Compare(this byte[] A, byte[] B, int Length) => Compare(A, B, 0, Length);
        /// <summary>
        /// 바이트 배열을 지정된 인덱스부터 지정된 길이까지 비교하여 값이 정확히 일치하는지 확인합니다.
        /// </summary>
        /// <param name="StartIndex">비교를 시작할 인덱스입니다.</param>
        /// <param name="Length">값을 비교할 길이입니다.</param>
        public static bool Compare(this byte[] A, byte[] B, int StartIndex, int Length)
        {
            int EndIndex = StartIndex + Length;
            if (A.Length < EndIndex || B.Length < EndIndex) return false;
            for (int Index = StartIndex; Index < EndIndex; Index++)
                try
                {
                    if (A[Index] != B[Index])
                        return false;
                }
                catch
                {
                    return false;
                }
            return true;
        }
        #endregion

        #region [    Compare Part   ]
        public static bool ComparePart(this byte[] A, params byte[] B) => A.ComparePart(B, 0);
        public static bool ComparePart(this byte[] A, byte[] B, int StartIndex)
        {
            int BLen = B.Length;
            if (BLen > (A.Length - StartIndex))
                return false;

            for (int i = 0; i < BLen; i++)
                if (A[StartIndex + i] != B[i])
                    return false;

            return true;
        }
        public static bool ComparePart(this List<byte> A, params byte[] B) => A.ComparePart(B, 0);
        public static bool ComparePart(this List<byte> A, byte[] B, int StartIndex)
        {
            int BLen = B.Length;
            if (BLen > (A.Count - StartIndex))
                return false;

            for (int i = 0; i < BLen; i++)
                if (A[StartIndex + i] != B[i])
                    return false;

            return true;
        }
        #endregion

        #region [    Sub Array    ]
        /// <summary>
        /// 지정된 바이트 배열을 지정된 인덱스부터 복사하여 반환합니다.
        /// </summary>
        /// <param name="array">데이터를 받아올 바이트 배열입니다.</param>
        /// <param name="StartIndex">인덱스의 시작 위치입니다.</param>
        /// <returns>바이트 배열입니다.</returns>
        public static byte[] SubArray(this byte[] array, int StartIndex)
        {
            byte[] buffer = new byte[array.Length - StartIndex];
            Array.ConstrainedCopy(array, StartIndex, buffer, 0, array.Length - StartIndex);
            return buffer;
        }
        /// <summary>
        /// 지정된 바이트 배열을 지정된 인덱스부터 지정된 길이만큼 복사하여 반환합니다.
        /// </summary>
        /// <param name="array">데이터를 받아올 바이트 배열입니다.</param>
        /// <param name="StartIndex">인덱스의 시작 위치입니다.</param>
        /// <param name="Length">복사할 배열의 길이입니다.</param>
        /// <returns>바이트 배열입니다.</returns>
        public static byte[] SubArray(this byte[] array, int StartIndex, int Length)
        {
            byte[] buffer = new byte[Length];
            Array.ConstrainedCopy(array, StartIndex, buffer, 0, Length);
            return buffer;
        }
        #endregion

        #region [    Copy    ]
        public static byte[] Copy(this byte[] array)
        {
            byte[] buffer = new byte[array.Length];
            Array.ConstrainedCopy(array, 0, buffer, 0, array.Length);
            return buffer;
        }
        #endregion

        #region [    Search Pattern    ]
        /// <summary>
        /// 바이트 배열에서 패턴과 정확히 일치하는 위치의 시작 인덱스들을 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="Pattern">검색에 사용될 패턴입니다.</param>
        public static List<int> SearchPattern(this byte[] Array, params byte[] Pattern)
        {
            List<int> Pos = new List<int>();
            int ArrLen = Array.Length, PatLen = Pattern.Length;
            for (int i = 0; i + PatLen <= ArrLen;)
                if (Array.ComparePart(Pattern, i))
                {
                    Pos.Add(i);
                    i += PatLen;
                }
                else i++;
            return Pos;
        }
        /// <summary>
        /// 바이트 배열에서 문자열과 정확히 일치하는 위치의 시작 인덱스들을 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="Text">검색에 사용될 문자열입니다.</param>
        public static List<int> SearchPattern(this byte[] Array, string Text) => Array.SearchPattern(Text.GetBytes());
        /// <summary>
        /// 바이트 배열에서 패턴과 정확히 일치하는 위치의 시작 인덱스를 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="OrderType">True이면 오름차순, False이면 내림차순</param>
        /// <param name="Pattern">검색에 사용될 패턴입니다.</param>
        /// <returns>가장 처음 발견된 값의 시작 인덱스입니다.</returns>
        public static int SearchPattern(this byte[] Array, bool OrderType, params byte[] Pattern)
        {
            int ArrLen = Array.Length, PatLen = Pattern.Length;
            if (OrderType)
            {
                for (int i = 0; i + PatLen <= ArrLen; i++)
                    if (Array.ComparePart(Pattern, i)) return i;
            }
            else
            {
                for (int i = ArrLen - PatLen; i >= 0; i--)
                    if (Array.ComparePart(Pattern, i)) return i;
            }
            return -1;
        }
        /// <summary>
        /// 바이트 배열에서 패턴과 정확히 일치하는 위치의 시작 인덱스들을 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="Pattern">검색에 사용될 패턴입니다.</param>
        public static List<int> SearchPattern(this List<byte> list, params byte[] Pattern)
        {
            List<int> Pos = new List<int>();
            int ListCount = list.Count, PatLen = Pattern.Length;
            for (int i = 0; i + PatLen <= ListCount;)
                if (list.ComparePart(Pattern, i))
                {
                    Pos.Add(i);
                    i += PatLen;
                }
                else i++;
            return Pos;
        }
        /// <summary>
        /// 바이트 배열에서 문자열과 정확히 일치하는 위치의 시작 인덱스들을 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="Text">검색에 사용될 문자열입니다.</param>
        public static List<int> SearchPattern(this List<byte> list, string Text) => list.SearchPattern(Text.GetBytes());
        /// <summary>
        /// 바이트 배열에서 패턴과 정확히 일치하는 위치의 시작 인덱스를 찾습니다.
        /// </summary>
        /// <param name="Array">검색할 바이트 배열입니다.</param>
        /// <param name="OrderType">True이면 시작부터, False이면 끝부터</param>
        /// <param name="Pattern">검색에 사용될 패턴입니다.</param>
        /// <returns>가장 처음 발견된 값의 시작 인덱스입니다.</returns>
        public static int SearchPattern(this List<byte> list, bool OrderType, params byte[] Pattern)
        {
            int ListCount = list.Count, PatLen = Pattern.Length;
            if (OrderType)
            {
                for (int i = 0; i + PatLen <= ListCount; i++)
                    if (list.ComparePart(Pattern, i)) return i;
            }
            else
            {
                for (int i = ListCount - PatLen; i >= 0; i--)
                    if (list.ComparePart(Pattern, i)) return i;
            }
            return -1;
        }
        #endregion

        #region [    Replace    ]
        /// <summary>
        /// 현재 인스턴스의 지정된 바이트 배열이 지정된 다른 바이트 배열로 모두 바뀌는 새 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="Source">원본 인스턴스입니다.</param>
        /// <param name="oldValue">바꿀 바이트 배열입니다.</param>
        /// <param name="newValue">모든 oldValue를 바꿀 바이트 배열입니다.</param>
        /// <returns>
        /// oldValue의 모든 인스턴스를 newValue로 바꾼다는 점을 제외하고 현재 바이트 배열과 동일한 바이트 배열입니다. oldValue를 현재 인스턴스에서
        /// 찾을 수 없으면 메서드가 변경되지 않은 현재 인스턴스를 반환합니다.
        /// </returns>
        public static byte[] Replace(this byte[] Source, byte[] oldValue, byte[] newValue)
        {
            byte[] dest = null, temp = null;
            int oldLen = oldValue.Length, newLen = newValue.Length;
            List<int> idx = Source.SearchPattern(oldValue);
            for (int i = idx.Count - 1; i >= 0; i--)
            {
                temp = temp == null ? Source : dest;

                dest = new byte[temp.Length - oldLen + newLen];

                // before found array
                Buffer.BlockCopy(temp, 0, dest, 0, idx[i]);
                // repl copy
                Buffer.BlockCopy(newValue, 0, dest, idx[i], newLen);
                // rest of src array
                Buffer.BlockCopy(temp, idx[i] + oldLen, dest, idx[i] + newLen, temp.Length - (idx[i] + oldLen));
            }
            return dest ?? Source;
        }
        /// <summary>
        /// 현재 인스턴스의 지정된 바이트 배열을 지정된 다른 바이트 배열로 모두 바꿉니다.
        /// </summary>
        /// <param name="Source">원본 인스턴스입니다.</param>
        /// <param name="oldValue">바꿀 바이트 배열입니다.</param>
        /// <param name="newValue">모든 oldValue를 바꿀 바이트 배열입니다.</param>
        public static void Replace(this List<byte> Source, byte[] oldValue, byte[] newValue)
        {
            int oldLength = oldValue.Length;
            List<int> TargetIndex = Source.SearchPattern(oldValue);
            for (int i = TargetIndex.Count - 1; i >= 0; i--)
            {
                Source.RemoveRange(TargetIndex[i], oldLength);
                Source.InsertRange(TargetIndex[i], newValue);
            }
        }

        /// <summary>
        /// 현재 인스턴스의 지정된 바이트 배열을 지정된 열거자에 포함된 바이트 배열로 모두 바꿉니다.
        /// </summary>
        /// <param name="Source">원본 인스턴스입니다.</param>
        /// <param name="oldValue">바꿀 바이트 배열입니다.</param>
        /// <param name="newValue">모든 oldValue를 바꿀 바이트 배열이 포함된 열거자입니다.</param>
        public static void Replace(this List<byte> Source, byte[] oldValue, IEnumerable<byte[]> newValue)
        {
            int oldLength = oldValue.Length;
            List<int> TargetIndex = Source.SearchPattern(oldValue);
            int idx = TargetIndex.Count - 1;
            foreach (var item in newValue)
            {
                if (idx < 0) break;
                Source.RemoveRange(TargetIndex[idx], oldLength);
                Source.InsertRange(TargetIndex[idx], item);
                idx--;
            }
        }
        #endregion

        #region [    Reverse    ]
        /// <summary>
        /// 1차원 <see cref="Array"/> 전체에 있는 요소의 시퀀스를 역순으로 설정합니다.
        /// </summary>
        /// <param name="array">역순으로 바꿀 1차원 <see cref="Array"/>입니다.</param>
        public static void Reverse(this byte[] array) => Array.Reverse(array);
        /// <summary>
        /// 1차원 <see cref="Array"/>의 요소 범위에 있는 요소의 시퀀스를 역순으로 설정합니다.
        /// </summary>
        /// <param name="array">역순으로 바꿀 1차원 <see cref="Array"/>입니다.</param>
        /// <param name="index">순서를 바꿀 섹션의 시작 인덱스입니다.</param>
        /// <param name="length">순서를 바꿀 섹션에 있는 요소 수입니다.</param>
        public static void Reverse(this byte[] array, int index, int length) => Array.Reverse(array, index, length);
        public static byte[] ReverseCopy(this byte[] array, bool useRef = true)
        {
            byte[] buffer = useRef ? array : array.Copy();
            Array.Reverse(array);
            return buffer;
        }
        public static short ReverseByte(this short value) => BitConverter.ToInt16(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static int ReverseByte(this int value) => BitConverter.ToInt32(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static long ReverseByte(this long value) => BitConverter.ToInt64(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static ushort ReverseByte(this ushort value) => BitConverter.ToUInt16(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static uint ReverseByte(this uint value) => BitConverter.ToUInt32(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static ulong ReverseByte(this ulong value) => BitConverter.ToUInt64(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static float ReverseByte(this float value) => BitConverter.ToSingle(BitConverter.GetBytes(value).ReverseCopy(), 0);
        public static double ReverseByte(this double value) => BitConverter.ToDouble(BitConverter.GetBytes(value).ReverseCopy(), 0);
        #endregion

        #region [    Split    ]
        public static byte[][] Split(this byte[] array, params byte[] Pattern)
        {
            List<int> pos = array.SearchPattern(Pattern);
            if (pos.Count == 0) return new byte[][] { array };
            int PatternLength = Pattern.Length;
            List<byte[]> parts = new List<byte[]>(pos.Count + 1) { array.SubArray(0, pos[0]) };
            for (int i = 0; i < pos.Count; i++)
            {
                int startIndex = pos[i] + PatternLength;
                if (i == pos.Count - 1) parts.Add(array.SubArray(startIndex));
                else parts.Add(array.SubArray(startIndex, pos[i + 1] - startIndex));
            }
            return parts.ToArray();
        }
        #endregion

        #region [    Get Text    ]
        public static string GetText(this byte[] array)
        {
            StringBuilder Builder = new StringBuilder();
            foreach (byte b in array)
                Builder.AppendFormat("{0:X2}", b);
            return Builder.ToString();
        }
        #endregion

        #region [    Append    ]
        public static byte[] Append(this byte[] array, byte[] bytes)
        {
            int OriginalLength = array.Length;
            Array.Resize(ref array, OriginalLength + bytes.Length);
            array.Write(OriginalLength, bytes);
            return array;
        }
        #endregion

        #region [    Insert    ]
        // Unimplemented
        #endregion
    }
}