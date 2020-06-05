namespace CirnoLib.Jass
{
    public static unsafe class StringHash
    {
        private static void Mix(ref uint a, ref uint b, ref uint c)
        {
            a -= b; a -= c; a ^= c >> 13;
            b -= c; b -= a; b ^= a << 8;
            c -= a; c -= b; c ^= b >> 13;
            a -= b; a -= c; a ^= c >> 12;
            b -= c; b -= a; b ^= a << 16;
            c -= a; c -= b; c ^= b >> 5;
            a -= b; a -= c; a ^= c >> 3;
            b -= c; b -= a; b ^= a << 10;
            c -= a; c -= b; c ^= b >> 15;
        }

        /// <param name="k">the key</param>
        /// <param name="length">the length of the key</param>
        /// <param name="initval">the previous hash, or an arbitrary value</param>
        private static int Hash(byte* k, uint length, uint initval)
        {
            uint a, b, c, len;

            // Set up the internal state
            len = length;
            a = b = 0x9e3779b9; // the golden ratio; an arbitrary value
            c = initval;        // the previous hash value

            //---------------------------------------- handle most of the key
            while (len >= 12)
            {
                a += k[0] + ((uint)k[1] << 8) + ((uint)k[2] << 16) + ((uint)k[3] << 24);
                b += k[4] + ((uint)k[5] << 8) + ((uint)k[6] << 16) + ((uint)k[7] << 24);
                c += k[8] + ((uint)k[9] << 8) + ((uint)k[10] << 16) + ((uint)k[11] << 24);
                Mix(ref a, ref b, ref c);
                k += 12; len -= 12;
            }

            //------------------------------------- handle the last 11 bytes
            c += length;
            switch (len)              // all the case statements fall through
            {
                case 11: goto len11;
                case 10: goto len10;
                case 9: goto len9;
                // the first byte of c is reserved for the length
                case 8: goto len8;
                case 7: goto len7;
                case 6: goto len6;
                case 5: goto len5;
                case 4: goto len4;
                case 3: goto len3;
                case 2: goto len2;
                case 1: goto len1;
                default: goto len0;
                // case 0: nothing left to add
            }
            len11: c += (uint)k[10] << 24;
            len10: c += (uint)k[9] << 16;
            len9: c += (uint)k[8] << 8;
            len8: b += (uint)k[7] << 24;
            len7: b += (uint)k[6] << 16;
            len6: b += (uint)k[5] << 8;
            len5: b += k[4];
            len4: a += (uint)k[3] << 24;
            len3: a += (uint)k[2] << 16;
            len2: a += (uint)k[1] << 8;
            len1: a += k[0];
            len0: Mix(ref a, ref b, ref c);
            //-------------------------------------------- report the result
            return unchecked((int)c);
        }

        public static int StrHash(this string text)
        {
            return StrHash(text.GetBytes());
        }
        public static int StrHash(this byte[] array)
        {
            fixed (byte* key = array)
            {
                return StrHash(key);
            }
        }
        public static int StrHash(byte* key)
        {
            byte[] buff = new byte[0x400];
            uint len = 0;
            while (*key != 0)
            {
                if (*key < 'a' || *key > 'z')
                    buff[len] = (*key == '/') ? (byte)'\\' : *key;
                else
                {
                    buff[len] = *key;
                    buff[len] -= 0x20;
                }
                key++;
                len++;
            }
            fixed (byte* buffer = buff)
            {
                return Hash(buffer, len, 0);
            }
        }
    }
}
