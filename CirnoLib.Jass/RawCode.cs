using System;
using System.Text;

namespace CirnoLib.Jass
{
    /// <summary>
    /// Warcraft III 의 원시코드에 관련된 함수를 가지고 있습니다.
    /// </summary>
    public static class RawCode
    {
        /// <summary>
        /// 원시코드를 정수로 표현합니다.
        /// </summary>
        /// <param name="array">정수로 표현할 1, 4 자리의 바이트 배열입니다.</param>
        /// <returns>정수로 표현된 원시코드입니다.</returns>
        public static int Numberize(this byte[] array)
        {
            switch (array.Length)
            {
                default: throw new Exception("입력받은 값은 지원하지 않는 길이입니다.");
                case 4:
                    byte[] temp = array.Copy();
                    for (int i = 0; i < 4; i++)
                    {
                        if (temp[i] >= 0x80 && i > 0) temp[i - 1]--;
                        else if (temp[i] == 0) throw new Exception("정상적인 정수로 변환할 수 없는 값입니다. (null)");
                        else if (temp[i] == 0xFF) throw new Exception("워크래프트에서 지원하지 않는 값입니다. (0xFF)");
                    }
                    return BitConverter.ToInt32(temp.ReverseCopy(), 0);
                case 1: return array[0];
            }
        }

        /// <summary>
        /// 원시코드를 정수로 표현합니다.
        /// </summary>
        /// <param name="text">정수로 표현할 1, 4 자리의 ASCII문자열입니다.</param>
        /// <returns>정수로 표현된 원시코드입니다.</returns>
        public static int Numberize(this string text) => text.GetBytes().Numberize();

        /// <summary>
        /// 정수를 원시코드로 표현합니다.
        /// </summary>
        /// <param name="hash">원시코드로 표현할 정수입니다.</param>
        /// <returns>1, 2, 4 자리의 ASCII로 표현된 정수입니다.</returns>
        public static string GetRawString(this int hash)
        {
            string text = Encoding.UTF8.GetString(hash.GetRawCode());
            if (Encoding.UTF8.GetByteCount(text) > 4) throw new Exception("정상적인 원시코드로 변환할 수 없는 정수입니다. (String)");
            return text;
            #region Old Code
            //int RawCodeLength = 1;
            //for (int i = 4; i > 0; i--)
            //{
            //    switch (i)
            //    {
            //        case 3: continue;
            //        case 1: goto LengthCheckBreak;
            //    }
            //    if (hash > Math.Pow(256, i - 1))
            //    {
            //        RawCodeLength = i;
            //        break;
            //    }
            //}
            //LengthCheckBreak:
            //StringBuilder text = new StringBuilder();
            //for (int i = 0; i < RawCodeLength; i++)
            //{
            //    int size = (int)Math.Pow(256, RawCodeLength - 1 - i);
            //    text.Append((char)(hash / size));
            //    hash %= size;
            //}
            //string value = text.ToString();
            //for (int i = 0; i < value.Length; i++)
            //    if (value[i] > 0x7F)
            //        throw new Exception("정상적인 원시코드로 변환할 수 없는 정수입니다.");
            //return value;
            #endregion
        }

        public static byte[] GetRawCode(this int hash)
        {
            byte[] buffer = BitConverter.GetBytes(hash);
            if (0 < hash && hash < 0x100) return new byte[] { buffer[0] };
            bool isOverflow = false;
            for (int i = 0; i < 4; i++)
            {
                if (isOverflow)
                {
                    if (buffer[i] == 0xFF) throw new OverflowException("정상적인 원시코드로 변환할 수 없는 정수입니다. (0xFF)");
                    else if (++buffer[i] < 0x80) isOverflow = false;
                    if (buffer[i] == 0xFF) throw new OverflowException("워크래프트에서 지원하지 않는 값입니다. (0xFE)");
                }
                else if (buffer[i] >= 0x80) isOverflow = true;
                else if (buffer[i] == 0) throw new OverflowException("정상적인 원시코드로 변환할 수 없는 정수입니다. (null)");
            }
            return buffer.ReverseCopy();
        }

        public static int[] GetNumberizeList(this string text)
        {
            string[] Codes = text.Split(',');
            foreach (var item in Codes)
                if (item.GetBytes().Length != 4)
                    return null;
            int[] Hashs = new int[Codes.Length];
            for (int i = 0; i < Hashs.Length; i++)
                Hashs[i] = Codes[i].Numberize();
            return Hashs;
        }

        public static int[] GetNumberizeList(this byte[] text)
        {
            if (text.Length < 4) return null;
            byte[][] Codes = text.Split(44);
            foreach (var item in Codes)
                if (item.Length != 4)
                    return null;
            int[] Hashs = new int[Codes.Length];
            for (int i = 0; i < Hashs.Length; i++)
                Hashs[i] = Codes[i].Numberize();
            return Hashs;
        }

        public static byte[] GetRawCodeList(this int[] hashs)
        {
            using (ByteStream bs = new ByteStream())
            {
                for (int i = 0; i < hashs.Length; i++)
                {
                    if (i != 0) bs.Write(',');
                    bs.Write(hashs[i].GetRawCode());
                }
                return bs.ToArray();
            }
        }

        public static string GetRawStringList(this int[] hashs)
        {
            StringBuilder Builder = new StringBuilder();
            for (int i = 0; i < hashs.Length; i++)
            {
                if (i != 0) Builder.Append(',');
                Builder.Append(hashs[i].GetRawString());
            }
            return Builder.ToString();
        }

        //public static string ToInt(string Line, int RawSize = 4)
        //{
        //    StringBuilder Builder = new StringBuilder();
        //    const char tile = '\'';
        //    for (int index = 0; index < Line.Length; index++)
        //    {
        //        switch (Line[index])
        //        {
        //            case tile:
        //                List<byte> Buffer = new List<byte>();
        //                byte[] Part = Encoding.UTF8.GetBytes(Line.Substring(++index, Line.Length - index));
        //                int innerIndex = 0;
        //                while (true)
        //                {
        //                    if (Part[innerIndex] > 127 || Buffer.Count > RawSize)
        //                    {
        //                        Builder.Append(tile);
        //                        goto Continue;
        //                    }
        //                    else if (Part[innerIndex] == tile)
        //                    {
        //                        if (Buffer.Count == 3)
        //                        {
        //                            Builder.Append(tile);
        //                            goto Continue;
        //                        }
        //                        break;
        //                    }
        //                    Buffer.Add(Part[innerIndex]);
        //                }
        //                string value = Encoding.UTF8.GetString(Buffer.ToArray());
        //                index += value.Length + 2;
        //                Builder.Append(Numberize(value));
        //                continue;
        //            case '$':
        //                StringBuilder innerBuilder = new StringBuilder();
        //                while (true)
        //                {
        //                    if (!(char.IsNumber(Line[++index])
        //                       || "0123456789ABCDEFabcdef".IndexOf(Line[index]) != -1))
        //                    {
        //                        index--;
        //                        if (innerBuilder.Length == 0)
        //                        {
        //                            Builder.Append('$');
        //                            goto Continue;
        //                        }
        //                        break;
        //                    }
        //                    innerBuilder.Append(Line[index]);
        //                }
        //                Builder.Append(Convert.ToInt32(innerBuilder.ToString(), 16));
        //                continue;
        //        }
        //        Builder.Append(Line[index]);
        //        Continue:;
        //    }
        //    return Builder.ToString();
        //}
    }
}
