using System;
using System.Text;

namespace CirnoLib.Jass
{
    public static class Hex
    {
        /// <summary>
        /// 16진수를 표현하는데 사용할 수 있는 ASCII배열입니다.
        /// </summary>
        private static readonly string hex = "0123456789ABCDEFabcdef";

        public static bool IsHexValue(this byte value)
        {
            foreach (var item in hex)
                if (value == item)
                    return true;
            return false;
        }
        public static bool IsHexValue(this byte[] value)
        {
            foreach (var item in value)
                if (!IsHexValue(item))
                    return false;
            return true;
        }

        public static bool IsHexValue(this string value) => value.GetBytes().IsHexValue();

        public static string ToInt(string Line)
        {
            StringBuilder Builder = new StringBuilder();
            for (int index = 0; index < Line.Length; index++)
            {
                if (Line[index] == '$')
                {
                    StringBuilder innerBuilder = new StringBuilder();
                    while (true)
                    {
                        if (!(char.IsNumber(Line[++index])
                           || hex.IndexOf(Line[index]) != -1))
                        {
                            index--;
                            if (innerBuilder.Length == 0)
                            {
                                Builder.Append('$');
                                goto Continue;
                            }
                            break;
                        }
                        innerBuilder.Append(Line[index]);
                    }
                    Builder.Append(Convert.ToInt32(innerBuilder.ToString(), 16));
                    continue;
                }
                Builder.Append(Line[index]);
                Continue:;
            }
            return Builder.ToString();
        }
    }
}
