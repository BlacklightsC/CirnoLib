namespace MPQLibrary
{
    public sealed class ADPCM_IMA
    {
        struct IMA_State
        {
            public short nStepSizeTableIdx;
            public short nPrevSampleValue;
        };

        const short constMinSampleValue = -32768;
        const short constMaxSampleValue = 32767;
        const short constMinSSTIdx = 0;
        const short constMaxSSTIdx = 88;

        short[] anStepSizeTable = new short[89]
        {
                        7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34,
                        37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143,
                        157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494,
                        544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552,
                        1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026,
                        4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442,
                        11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623,
                        27086, 29794, 32767
        };

        short[] anIndexAdjustTable = new short[16]
        {
                        -1, -1, -1, -1,                //        +0 ~ +3 : 스텝 사이즈 감소 
                        2, 4, 6, 8,                        //        +4 ~ +7 : 스텝 사이즈 증가 
                        -1, -1, -1, -1,                //        -0 ~ -3 : 스텝 사이즈 감소 
                        2, 4, 6, 8                        //        -4 ~ -7 : 스텝 사이즈 증가 
        };

        IMA_State m_sIMAState;

        public ADPCM_IMA()
        {
            Clear();
        }

        public void Clear()
        {
            m_sIMAState.nStepSizeTableIdx = 0;
            m_sIMAState.nPrevSampleValue = 0;
        }

        public byte Encode_ADPCM_IMA(short nSample)
        {
            int nDiff = nSample - m_sIMAState.nPrevSampleValue;
            short nStep = anStepSizeTable[m_sIMAState.nStepSizeTableIdx];
            byte byResult = 0;


            //        부호 처리 
            if (nDiff < 0)
            {
                byResult = 0x08;
                nDiff = (short)-nDiff;
            }


            //        인코딩 
            if (nDiff >= nStep) { byResult |= 4; nDiff -= nStep; }
            nStep >>= 1;
            if (nDiff >= nStep) { byResult |= 2; nDiff -= nStep; }
            nStep >>= 1;
            if (nDiff >= nStep) { byResult |= 1; nDiff -= nStep; }

            byResult &= 0x0f;


            //        sIMAState 갱신 
            Decode_ADPCM_IMA(byResult);


            return byResult;
        }



        public short Decode_ADPCM_IMA(byte bySample)
        {
            int nStep = anStepSizeTable[m_sIMAState.nStepSizeTableIdx];
            int nDiff = nStep >> 3;

            //        변화량 근사 추출 

            bySample &= 0x0f;

            if ((bySample & 0x01) > 0) nDiff += nStep >> 2;
            if ((bySample & 0x02) > 0) nDiff += nStep >> 1;
            if ((bySample & 0x04) > 0) nDiff += nStep;
            if ((bySample & 0x08) > 0) nDiff = -nDiff;                        //        부호 역전 


            //        샘플값 구하기 
            int nNewSample = m_sIMAState.nPrevSampleValue;
            nNewSample += nDiff;
            if (nNewSample < constMinSampleValue) nNewSample = constMinSampleValue;
            if (nNewSample > constMaxSampleValue) nNewSample = constMaxSampleValue;


            //        sIMAState 갱신 
            int nNewIndex = m_sIMAState.nStepSizeTableIdx;
            nNewIndex += anIndexAdjustTable[bySample];
            if (nNewIndex < constMinSSTIdx) nNewIndex = constMinSSTIdx;
            if (nNewIndex > constMaxSSTIdx) nNewIndex = constMaxSSTIdx;

            m_sIMAState.nPrevSampleValue = (short)nNewSample;
            m_sIMAState.nStepSizeTableIdx = (short)nNewIndex;


            return (short)nNewSample;
        }

    }        //        End of 'public class ADPCM_IMA' 
}
