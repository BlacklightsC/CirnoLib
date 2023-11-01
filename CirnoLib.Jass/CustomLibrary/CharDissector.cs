namespace CirnoLib.Jass.CustomLibrary
{
    public static class CharDissector
    {
        //private static int[] SH2UTF8;
        //private static string[] CHAR;
        //private static string[] BYTE;
        //private static int[] CHAR_BASE;
        //private static int[] CHAR_MULTIPLY;

        public static int[] FLOOR_CONSONANT;
        public static int[] FLOOR_VOWEL;
        public static int[] FLOOR_FINALCONSONANT;

        //private const int CORRECTION_CHAR_START = 3072;
        //private const int CORRECTION_SINGLE_START    = 28367;
        //private const int CORRECTION_SINGLE_JUNCTION = 30;

        //private const int CNT_CONSONANT = 19;
        //private const int CNT_VOWEL = 21;
        //private const int CNT_FINALCONSONANT = 28;

        //private const int CHAR_CONSONANT = 0;
        //private const int CHAR_VOWEL = CHAR_CONSONANT + CNT_CONSONANT;
        //private const int CHAR_FINALCONSONANT = CHAR_VOWEL + CNT_VOWEL;
        //private const int CHAR_ERROR = CHAR_FINALCONSONANT + CNT_FINALCONSONANT;
        //private const int CHAR_SINGLE = CHAR_ERROR + 1;

        //private static string sChar = null;


        //private static int iConsonant = 0;
        //private static int iVowel = 0;
        //private static int iFinalConsonant = 0;


        //private static int[] CharDisassemblyInt;
        //private static int[] CharReassemblyInt;

        static CharDissector()
        {
            FLOOR_CONSONANT = new int[0x100];
            FLOOR_VOWEL = new int[0x100];
            FLOOR_FINALCONSONANT = new int[0x100];

            FLOOR_CONSONANT[2] = 0;
            FLOOR_CONSONANT[146] = 1;
            FLOOR_CONSONANT[174] = 2;
            FLOOR_CONSONANT[107] = 3;
            FLOOR_CONSONANT[27] = 4;
            FLOOR_CONSONANT[75] = 5;
            FLOOR_CONSONANT[168] = 6;
            FLOOR_CONSONANT[165] = 7;
            FLOOR_CONSONANT[123] = 8;
            FLOOR_CONSONANT[147] = 9;
            FLOOR_CONSONANT[87] = 10;
            FLOOR_CONSONANT[13] = 11;
            FLOOR_CONSONANT[212] = 12;
            FLOOR_CONSONANT[22] = 13;
            FLOOR_CONSONANT[112] = 14;
            FLOOR_CONSONANT[56] = 15;
            FLOOR_CONSONANT[71] = 16;
            FLOOR_CONSONANT[192] = 17;
            FLOOR_CONSONANT[88] = 18;

            FLOOR_VOWEL[121] = 0;
            FLOOR_VOWEL[103] = 1;
            FLOOR_VOWEL[20] = 2;
            FLOOR_VOWEL[172] = 3;
            FLOOR_VOWEL[185] = 4;
            FLOOR_VOWEL[1] = 5;
            FLOOR_VOWEL[26] = 6;
            FLOOR_VOWEL[63] = 7;
            FLOOR_VOWEL[70] = 8;
            FLOOR_VOWEL[105] = 9;
            FLOOR_VOWEL[104] = 10;
            FLOOR_VOWEL[162] = 11;
            FLOOR_VOWEL[37] = 12;
            FLOOR_VOWEL[12] = 13;
            FLOOR_VOWEL[52] = 14;
            FLOOR_VOWEL[129] = 15;
            FLOOR_VOWEL[17] = 16;
            FLOOR_VOWEL[54] = 17;
            FLOOR_VOWEL[188] = 18;
            FLOOR_VOWEL[45] = 19;
            FLOOR_VOWEL[126] = 20;

            FLOOR_FINALCONSONANT[0] = 0;
            FLOOR_FINALCONSONANT[2] = 1;
            FLOOR_FINALCONSONANT[146] = 2;
            FLOOR_FINALCONSONANT[167] = 3;
            FLOOR_FINALCONSONANT[174] = 4;
            FLOOR_FINALCONSONANT[73] = 5;
            FLOOR_FINALCONSONANT[25] = 6;
            FLOOR_FINALCONSONANT[107] = 7;
            FLOOR_FINALCONSONANT[75] = 8;
            FLOOR_FINALCONSONANT[204] = 9;
            FLOOR_FINALCONSONANT[187] = 10;
            FLOOR_FINALCONSONANT[194] = 11;
            FLOOR_FINALCONSONANT[10] = 12;
            FLOOR_FINALCONSONANT[186] = 13;
            FLOOR_FINALCONSONANT[128] = 14;
            FLOOR_FINALCONSONANT[59] = 15;
            FLOOR_FINALCONSONANT[168] = 16;
            FLOOR_FINALCONSONANT[165] = 17;
            FLOOR_FINALCONSONANT[138] = 18;
            FLOOR_FINALCONSONANT[147] = 19;
            FLOOR_FINALCONSONANT[87] = 20;
            FLOOR_FINALCONSONANT[13] = 21;
            FLOOR_FINALCONSONANT[212] = 22;
            FLOOR_FINALCONSONANT[112] = 23;
            FLOOR_FINALCONSONANT[56] = 24;
            FLOOR_FINALCONSONANT[71] = 25;
            FLOOR_FINALCONSONANT[192] = 26;
            FLOOR_FINALCONSONANT[88] = 27;
        }

    }
}
