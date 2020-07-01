using System;
using System.IO;
using System.Text;
using System.Security;

namespace CirnoLib
{
    public static class Global
    {
        #region [    Random    ]
        private static readonly Random Random = new Random();
        /// <summary>
        /// 지정된 최대값보다 작은 음수가 아닌 임의의 정수를 반환합니다.
        /// </summary>
        /// <param name="maxValue">단독의 상한값 난수를 생성 합니다. maxValue보다 크거나 0 이어야 합니다.</param>
        public static byte GetRandom(this byte maxValue) => (byte)Random.Next(maxValue);
        /// <summary>
        /// 지정된 범위 내의 임의의 정수를 반환합니다.
        /// </summary>
        /// <param name="minValue">임의의 수의 경계값 반환 됩니다.</param>
        /// <param name="maxValue">반환 되는 임의의 수의 단독 상한입니다. maxValue보다 크거나 해야 minValue합니다.</param>
        public static byte GetRandom(this byte minValue, byte maxValue) => (byte)Random.Next(minValue, maxValue);
        /// <summary>
        /// 지정된 최대값보다 작은 음수가 아닌 임의의 정수를 반환합니다.
        /// </summary>
        /// <param name="maxValue">단독의 상한값 난수를 생성 합니다. maxValue보다 크거나 0 이어야 합니다.</param>
        public static int GetRandom(this int maxValue) => Random.Next(maxValue);
        /// <summary>
        /// 지정된 범위 내의 임의의 정수를 반환합니다.
        /// </summary>
        /// <param name="minValue">임의의 수의 경계값 반환 됩니다.</param>
        /// <param name="maxValue">반환 되는 임의의 수의 단독 상한입니다. maxValue보다 크거나 해야 minValue합니다.</param>
        public static int GetRandom(this int minValue, int maxValue) => Random.Next(minValue, maxValue);
        /// <summary>
        /// 0.0보다 크거나 같고 1.0보다 작은 부동 소수점 난수입니다.
        /// </summary>
        /// <returns>0.0보다 크거나 같고 1.0보다 작은 배정밀도 부동 소수점 숫자입니다.</returns>
        public static double GetRandom(this double Dummy) => Random.NextDouble();
        /// <summary>
        /// 바이트 배열의 요소를 난수로 채웁니다.
        /// </summary>
        /// <param name="buffer">임의의 숫자를 포함 하는 바이트의 배열입니다.</param>
        public static byte[] GetRandom(this byte[] buffer)
        {
            Random.NextBytes(buffer);
            return buffer;
        }
        #endregion

        #region [    Convert    ]
        internal static readonly Encoding DefaultEncoding = Encoding.UTF8;
        #region public static int GetByteCount(this string text)
        /// <summary>
        /// 파생 클래스에서 재정의되면 지정된 문자열의 문자를 인코딩하여 생성되는 바이트 수를 계산합니다.
        /// </summary>
        /// <param name="text">인코딩할 문자 집합이 포함된 문자열입니다.</param>
        /// <returns>지정한 문자를 인코딩할 경우 생성되는 바이트 수입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/>가 null인 경우</exception>
        /// <exception cref="EncoderFallbackException">대체가 발생했습니다(전체 설명은 .NET Framework의 문자 인코딩 참조). 및 <see cref="EncoderFallback"/>이 <see cref="EncoderExceptionFallback"/>로 설정됩니다.</exception>
        public static int GetByteCount(this string text) => DefaultEncoding.GetByteCount(text);
        #endregion

        #region public static bool ToBoolean(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림의 지정된 된 위치에 바이트에서 변환 하는 부울 값을 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">내 바이트의 인덱스 <paramref name="value"/>"/>합니다.</param>
        /// <returns>true 경우에 바이트 index 에서 value 0이 아니고, 그렇지 않으면 false합니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/>가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index 가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static bool ToBoolean(this Stream stream, int index = 0) => BitConverter.ToBoolean(stream.SubArray(index, 1), 0);
        #endregion
        #region public static char ToChar(this Stream stream, int index = 0)
        /// <summary>
        /// 변환 된 스트림에 지정된 된 위치에 2 바이트에서 유니코드 문자를 반환 합니다.
        /// </summary>
        /// <param name="value">배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>2 바이트에서 시작 하 여 형성 된 문자 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static char ToChar(this Stream stream, int index = 0) => BitConverter.ToChar(stream.SubArray(index, 2), 0);
        #endregion
        #region public static short ToInt16(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에 2 바이트에서 변환 하는 16 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 2 바이트로 형성 된 16 비트 부호 있는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static short ToInt16(this Stream stream, int index = 0) => BitConverter.ToInt16(stream.SubArray(index, 2), 0);
        #endregion
        #region public static int ToInt32(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에서 4 바이트에서 변환 하는 32 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 4 바이트로 형성 된 32 비트 부호 있는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 3,-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static int ToInt32(this Stream stream, int index = 0) => BitConverter.ToInt32(stream.SubArray(index, 4), 0);
        #endregion
        #region public static long ToInt64(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에 8 바이트에서 변환 하는 64 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 8 바이트도 64 비트 부호 있는 정수로 구성 된 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static long ToInt64(this Stream stream, int index = 0) => BitConverter.ToInt64(stream.SubArray(index, 8), 0);
        #endregion
        #region public static ushort ToUInt16(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에 2 바이트에서 변환 하는 16 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 2 바이트로 형성 된 16 비트 부호 없는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static ushort ToUInt16(this Stream stream, int index = 0) => BitConverter.ToUInt16(stream.SubArray(index, 2), 0);
        #endregion
        #region public static uint ToUInt32(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에서 4 바이트에서 변환 하는 32 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 4 바이트로 형성 된 32 비트 부호 없는 정수 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 3,-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static uint ToUInt32(this Stream stream, int index = 0) => BitConverter.ToUInt32(stream.SubArray(index, 4), 0);
        #endregion
        #region public static ulong ToUInt64(this Stream stream, int index = 0)
        /// <summary>
        /// 스트림에 지정된 된 위치에 8 바이트에서 변환 하는 64 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 8 바이트도 64 비트 부호 없는 정수로 구성 된 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static ulong ToUInt64(this Stream stream, int index = 0) => BitConverter.ToUInt64(stream.SubArray(index, 8), 0);
        #endregion
        #region public static float ToSingle(this Stream stream, int index = 0)
        /// <summary>
        /// 4 바이트를 스트림의 지정된 된 위치에서 변환 된 단 정밀도 부동 소수점 숫자를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>단 정밀도 부동 소수점 숫자에서 시작 하는 4 바이트로 형성 된 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static float ToSingle(this Stream stream, int index = 0) => BitConverter.ToSingle(stream.SubArray(index, 4), 0);
        #endregion
        #region public static double ToDouble(this Stream stream, int index = 0)
        /// <summary>
        /// 변환 된 스트림에 지정된 된 위치에 8 바이트에서 2 배 정밀도 부동 소수점 숫자를 반환 합니다.
        /// </summary>
        /// <param name="stream">스트림입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>배정밀도 부동 소수점 숫자에서 시작 하는 8 바이트로 형성 된 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static double ToDouble(this Stream stream, int index = 0) => BitConverter.ToDouble(stream.SubArray(index, 8), 0);
        #endregion

        #region public static bool ToBoolean(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열의 지정된 된 위치에 바이트에서 변환 하는 부울 값을 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">내 바이트의 인덱스 <paramref name="value"/>"/>합니다.</param>
        /// <returns>true 경우에 바이트 index 에서 value 0이 아니고, 그렇지 않으면 false합니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/>가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index 가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static bool ToBoolean(this byte[] value, int index = 0) => BitConverter.ToBoolean(value, index);
        #endregion
        #region public static char ToChar(this byte[] value, int index = 0)
        /// <summary>
        /// 변환 된 바이트 배열에 지정된 된 위치에 2 바이트에서 유니코드 문자를 반환 합니다.
        /// </summary>
        /// <param name="value">배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>2 바이트에서 시작 하 여 형성 된 문자 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static char ToChar(this byte[] value, int index = 0) => BitConverter.ToChar(value, index);
        #endregion
        #region public static short ToInt16(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에 2 바이트에서 변환 하는 16 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 2 바이트로 형성 된 16 비트 부호 있는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static short ToInt16(this byte[] value, int index = 0) => BitConverter.ToInt16(value, index);
        #endregion
        #region public static int ToInt32(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에서 4 바이트에서 변환 하는 32 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 4 바이트로 형성 된 32 비트 부호 있는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 3,-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static int ToInt32(this byte[] value, int index = 0) => BitConverter.ToInt32(value, index);
        #endregion
        #region public static long ToInt64(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에 8 바이트에서 변환 하는 64 비트 부호 있는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 8 바이트도 64 비트 부호 있는 정수로 구성 된 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static long ToInt64(this byte[] value, int index = 0) => BitConverter.ToInt64(value, index);
        #endregion
        #region public static ushort ToUInt16(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에 2 바이트에서 변환 하는 16 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 2 바이트로 형성 된 16 비트 부호 없는 정수 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 같으면 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static ushort ToUInt16(this byte[] value, int index = 0) => BitConverter.ToUInt16(value, index);
        #endregion
        #region public static uint ToUInt32(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에서 4 바이트에서 변환 하는 32 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 4 바이트로 형성 된 32 비트 부호 없는 정수 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 3,-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static uint ToUInt32(this byte[] value, int index = 0) => BitConverter.ToUInt32(value, index);
        #endregion
        #region public static ulong ToUInt64(this byte[] value, int index = 0)
        /// <summary>
        /// 바이트 배열에 지정된 된 위치에 8 바이트에서 변환 하는 64 비트 부호 없는 정수를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>시작 하는 8 바이트도 64 비트 부호 없는 정수로 구성 된 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        public static ulong ToUInt64(this byte[] value, int index = 0) => BitConverter.ToUInt64(value, index);
        #endregion
        #region public static float ToSingle(this byte[] value, int index = 0)
        /// <summary>
        /// 4 바이트를 바이트 배열의 지정된 된 위치에서 변환 된 단 정밀도 부동 소수점 숫자를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>단 정밀도 부동 소수점 숫자에서 시작 하는 4 바이트로 형성 된 startIndex합니다.</returns>
        /// <exception cref="ArgumentException">startIndex길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">startIndex가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static float ToSingle(this byte[] value, int index = 0) => BitConverter.ToSingle(value, index);
        #endregion
        #region public static double ToDouble(this byte[] value, int index = 0)
        /// <summary>
        /// 변환 된 바이트 배열에 지정된 된 위치에 8 바이트에서 2 배 정밀도 부동 소수점 숫자를 반환 합니다.
        /// </summary>
        /// <param name="value">바이트 배열입니다.</param>
        /// <param name="index">value 내의 시작 위치입니다.</param>
        /// <returns>배정밀도 부동 소수점 숫자에서 시작 하는 8 바이트로 형성 된 index합니다.</returns>
        /// <exception cref="ArgumentException">index길이 보다 크거나 value 7-가의 길이 보다 작거나 같음 및 value 1을 뺀 값입니다.</exception>
        /// <exception cref="ArgumentNullException">value가 null인 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">index가 0 보다 작거나의 길이 보다 큰 value 1을 뺀 값입니다.</exception>
        [SecuritySafeCritical]
        public static double ToDouble(this byte[] value, int index = 0) => BitConverter.ToDouble(value, index);
        #endregion
        #region  public static string GetString(this byte[] bytes)
        /// <summary>
        /// 파생 클래스에서 재정의되면 지정한 바이트 배열의 모든 바이트를 문자열로 디코딩합니다.
        /// </summary>
        /// <param name="bytes">디코딩할 바이트 시퀀스를 포함하는 바이트 배열입니다.</param>
        /// <returns>지정된 바이트 시퀀스에 대한 디코딩 결과가 포함된 문자열입니다.</returns>
        /// <exception cref="ArgumentException">잘못 된 유니코드 코드 포인트를 포함 하는 바이트 배열.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/>가 null인 경우</exception>
        /// <exception cref="DecoderFallbackException">대체가 발생했습니다(전체 설명은 .NET Framework의 문자 인코딩 참조). 및 <see cref="DecoderFallback"/>이 <see cref="DecoderExceptionFallback"/>로 설정됩니다.</exception>
        public static string GetString(this byte[] bytes) => DefaultEncoding.GetString(bytes);
        #endregion
        #region  public static string GetString(this byte[] bytes, Encoding encoding)
        /// <summary>
        /// 파생 클래스에서 재정의되면 지정한 바이트 배열의 모든 바이트를 문자열로 디코딩합니다.
        /// </summary>
        /// <param name="bytes">디코딩할 바이트 시퀀스를 포함하는 바이트 배열입니다.</param>
        /// <param name="encoding">디코딩에 사용될 문자 인코더입니다.</param>
        /// <returns>지정된 바이트 시퀀스에 대한 디코딩 결과가 포함된 문자열입니다.</returns>
        /// <exception cref="ArgumentException">잘못 된 유니코드 코드 포인트를 포함 하는 바이트 배열.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/>가 null인 경우</exception>
        /// <exception cref="DecoderFallbackException">대체가 발생했습니다(전체 설명은 .NET Framework의 문자 인코딩 참조). 및 <see cref="DecoderFallback"/>이 <see cref="DecoderExceptionFallback"/>로 설정됩니다.</exception>
        public static string GetString(this byte[] bytes, Encoding encoding) => encoding.GetString(bytes);
        #endregion

        #region public static byte[] GetBytes(this bool value)
        /// <summary>
        /// 바이트 배열로 지정된 된 부울 값을 반환합니다.
        /// </summary>
        /// <param name="value">부울 값입니다.</param>
        /// <returns>길이가 1 바이트 배열입니다.</returns>
        public static byte[] GetBytes(this bool value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this char value)
        /// <summary>
        /// 바이트의 배열로 지정된 된 유니코드 문자 값을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 문자입니다.</param>
        /// <returns>배열 길이 2 바이트입니다.</returns>
        public static byte[] GetBytes(this char value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this short value)
        /// <summary>
        /// 지정 된 16 비트 부호 있는 정수 값으로 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>배열 길이 2 바이트입니다.</returns>
        [SecuritySafeCritical]
        public static byte[] GetBytes(this short value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this int value)
        /// <summary>
        /// 바이트의 배열로 지정 된 32 비트 부호 있는 정수 값을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>길이가 4 사용 하 여 바이트 배열입니다.</returns>
        [SecuritySafeCritical]
        public static byte[] GetBytes(this int value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this long value)
        /// <summary>
        /// 지정된 64비트 부호 있는 정수 값을 바이트 배열로 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>길이가 8인 바이트 배열입니다.</returns>
        [SecuritySafeCritical]
        public static byte[] GetBytes(this long value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this ushort value)
        /// <summary>
        /// 지정된 된 16 비트 부호 없는 정수 값으로 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>배열 길이 2 바이트입니다.</returns>
        public static byte[] GetBytes(this ushort value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this uint value)
        /// <summary>
        /// 지정된 된 32 비트 부호 없는 정수 값으로 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>길이가 4 사용 하 여 바이트 배열입니다.</returns>
        public static byte[] GetBytes(this uint value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this ulong value)
        /// <summary>
        /// 지정된 된 64 비트 부호 없는 정수 값으로 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>길이가 8인 바이트 배열입니다.</returns>
        public static byte[] GetBytes(this ulong value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this float value)
        /// <summary>
        /// 지정된 된 단 정밀도 부동 소수점 값으로 바이트 배열을 반환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>길이가 4 사용 하 여 바이트 배열입니다.</returns>
        [SecuritySafeCritical]
        public static byte[] GetBytes(this float value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this double value)
        /// <summary>
        /// 지정된 된 64 비트 부호 있는 정수를 이중 정밀도 부동 소수점 숫자로 변환합니다.
        /// </summary>
        /// <param name="value">변환할 숫자입니다.</param>
        /// <returns>값이에 해당 하는 두 자리 부동 소수점 숫자 value합니다.</returns>
        [SecuritySafeCritical]
        public static byte[] GetBytes(this double value) => BitConverter.GetBytes(value);
        #endregion
        #region public static byte[] GetBytes(this string text)
        /// <summary>
        /// 파생 클래스에서 재정의되면 지정한 문자열의 모든 문자를 바이트 시퀀스로 인코딩합니다.
        /// </summary>
        /// <param name="text">인코딩할 문자가 포함된 문자열입니다.</param>
        /// <returns>지정한 문자 집합을 인코딩한 결과가 포함된 바이트 배열입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/>가 null인 경우</exception>
        /// <exception cref="EncoderFallbackException">대체가 발생했습니다(전체 설명은 .NET Framework의 문자 인코딩 참조). 및 <see cref="EncoderFallback"/>이 <see cref="EncoderExceptionFallback"/>로 설정됩니다.</exception>
        public static byte[] GetBytes(this string text) => text == null ? new byte[0] :DefaultEncoding.GetBytes(text);
        #endregion
        #region public static byte[] GetBytes(this string text, Encoding encoding)
        /// <summary>
        /// 파생 클래스에서 재정의되면 지정한 문자열의 모든 문자를 바이트 시퀀스로 인코딩합니다.
        /// </summary>
        /// <param name="text">인코딩할 문자가 포함된 문자열입니다.</param>
        /// <param name="encoding">인코딩에 사용될 문자 인코더입니다.</param>
        /// <returns>지정한 문자 집합을 인코딩한 결과가 포함된 바이트 배열입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/>가 null인 경우</exception>
        /// <exception cref="EncoderFallbackException">대체가 발생했습니다(전체 설명은 .NET Framework의 문자 인코딩 참조). 및 <see cref="EncoderFallback"/>이 <see cref="EncoderExceptionFallback"/>로 설정됩니다.</exception>
        public static byte[] GetBytes(this string text, Encoding encoding) => encoding.GetBytes(text);
        #endregion
        #region public static byte[] GetBytes(this object value)
        public static byte[] GetBytes(this object value)
        {
            switch (value.GetType().ToString())
            {
                case "System.Boolean": return GetBytes((bool)value);
                case "System.Char": return GetBytes((char)value);
                case "System.Int16": return GetBytes((short)value);
                case "System.Int32": return GetBytes((int)value);
                case "System.Int64": return GetBytes((long)value);
                case "System.UInt16": return GetBytes((ushort)value);
                case "System.UInt32": return GetBytes((uint)value);
                case "System.UInt64": return GetBytes((ulong)value);
                case "System.Single": return GetBytes((float)value);
                case "System.Double": return GetBytes((double)value);
                case "System.String": return GetBytes((string)value);
                default:
                    if (value is IArrayable arrayable) return arrayable.ToArray();
                    throw new NotImplementedException("바이트 배열로 직렬화할 수 없는 형식입니다.");
            }
        }
        #endregion

        #endregion

        #region [    ToArray    ]
        /// <summary>
        /// 해당 <see cref="IArrayable"/> 배열을 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="IArrayable"/> 배열의 바이트 배열입니다.</returns>
        public static byte[] ToArray(this IArrayable[] array)
        {
            if (array == null) return new byte[0];
            using (ByteStream bs = new ByteStream())
            {
                foreach (var item in array)
                    bs.Write(item);
                return bs.ToArray();
            }
        }
        ///// <summary>
        ///// 해당 <see cref="IEnumerable{IArrayable}"/>를 바이트 배열로 반환합니다.
        ///// </summary>
        ///// <returns><see cref="IEnumerable{IArrayable}"/>의 바이트 배열입니다.</returns>
        //public static byte[] ToArray(this IEnumerable<IArrayable> enumerable)
        //{
        //    using (ByteStream bs = new ByteStream())
        //    {
        //        foreach (var item in enumerable)
        //            bs.Write(item);
        //        return bs.ToArray();
        //    }
        //}
        ///// <summary>
        ///// 해당 <see cref="List{IArrayable}"/>를 바이트 배열로 반환합니다.
        ///// </summary>
        ///// <returns><see cref="List{IArrayable}"/>의 바이트 배열입니다.</returns>
        //public static byte[] ToArray(this List<IArrayable> list)
        //{
        //    using (ByteStream bs = new ByteStream())
        //    {
        //        foreach (var item in list)
        //            bs.Write(item);
        //        return bs.ToArray();
        //    }
        //}
        #endregion
    }
}
