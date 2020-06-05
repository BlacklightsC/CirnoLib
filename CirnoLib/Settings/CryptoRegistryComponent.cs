using System;
using Microsoft.Win32;

namespace CirnoLib.Settings
{
    /// <summary>
    /// <see cref="Registry"/>를 좀 더 손쉽게 사용하기 위한 클래스입니다. <see cref="RegistryComponent"/>에 암호화 및 난독화가 추가된 클래스입니다.
    /// </summary>
    public sealed class CryptoRegistryComponent : IDisposable
    {
        #region private readonly RegistryKey RootKey
        /// <summary>
        /// 개체가 가르키는 키의 루트입니다.
        /// </summary>
        private readonly RegistryKey RootKey;
        #endregion

        #region private readonly string CryptoKey
        /// <summary>
        /// 개체를 암복호화하는데 사용될 암호키입니다.
        /// </summary>
        private readonly string CryptoKey;
        #endregion

        #region public CryptoRegistryComponent(string RootPath, string CryptoKey)
        /// <summary>
        /// 개체를 사용자가 제공하는 키를 루트로 초기화 합니다.
        /// </summary>
        /// <param name="RootPath">개체를 초기화할 루트 키 입니다.</param>
        /// <param name="CryptoKey">개체를 암복호화할 암호키 입니다.</param>
        public CryptoRegistryComponent(string RootPath, string CryptoKey)
        {
            if (string.IsNullOrEmpty(RootPath)) throw new ArgumentException("키가 제공되지 않았습니다.");
            this.CryptoKey = CryptoKey; // ?? throw new ArgumentNullException("암호키가 제공되지 않았습니다.");
            string path = "Software\\" + RootPath;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            RootKey = key ?? Registry.CurrentUser.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
        }
        #endregion

        #region public void Clear()
        /// <summary>
        /// <see cref="RootKey"/>내에 존재하는 모든 키와 값을 재귀적으로 삭제합니다.
        /// </summary>
        public void Clear()
        {
            foreach (var item in RootKey.GetSubKeyNames())
            {
                RootKey.DeleteSubKeyTree(item, false);
            }
            foreach (var item in RootKey.GetValueNames())
            {
                RootKey.DeleteValue(item, false);
            }
        }
        #endregion

        #region public void DeleteValue(string key, string name)
        /// <summary>
        /// 지정된 값을 지정된 하위 키에서 삭제합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">삭제할 값의 이름입니다.</param>
        public void DeleteValue(string key, string name)
        {
            if (string.IsNullOrEmpty(key))
            {
                DeleteValue(name.HashSHA256());
            }
            else
            {
                using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                {
                    if (regKey != null)
                    {
                        regKey.DeleteValue(name.HashSHA256(), false);
                    }
                }
            }
        }
        #endregion
        #region public void DeleteValue(string name)
        /// <summary>
        /// 지정된 값을 이 키에서 삭제합니다.
        /// </summary>
        /// <param name="name">삭제할 값의 이름입니다.</param>
        public void DeleteValue(string name = null) => RootKey.DeleteValue(name.HashSHA256(), false);
        #endregion

        #region [    Get Value    ]

        #region private bool GetBoolean(RegistryKey key, string name, bool defaultValue, string CryptoKey)
        private bool GetBoolean(RegistryKey key, string name, bool defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is bool && (bool)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (1 <= buffer.Length && buffer.Length <= 16)
                    return buffer.ToBoolean();
            }
            throw new InvalidCastException("암호화된 Boolean 형식이 아닙니다.");
        }
        #endregion
        #region public bool GetBoolean(string key, string name, bool defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public bool GetBoolean(string key, string name, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(key)) return GetBoolean(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetBoolean(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public bool GetBoolean(string name, bool defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public bool GetBoolean(string name, bool defaultValue = false)
            => GetBoolean(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static byte GetByte(RegistryKey key, string name, byte defaultValue, string CryptoKey)
        private static byte GetByte(RegistryKey key, string name, byte defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is byte && (byte)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (1 <= buffer.Length && buffer.Length <= 16)
                    return buffer[0];
            }
            throw new InvalidCastException("암호화된 Byte 형식이 아닙니다.");
        }
        #endregion
        #region public byte GetByte(string key, string name, byte defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public byte GetByte(string key, string name, byte defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetByte(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetByte(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public byte GetByte(string name, byte defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public byte GetByte(string name, byte defaultValue = 0)
            => GetByte(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static byte[] GetBytes(RegistryKey key, string name, byte[] defaultValue, string CryptoKey)
        private static byte[] GetBytes(RegistryKey key, string name, byte[] defaultValue, string CryptoKey)
        {
            if (key.GetValue(name.HashSHA256(), defaultValue) is byte[] value)
            {
                if (value == defaultValue) return defaultValue;
                else return value.AES256Decrypt(CryptoKey);
            }
            else throw new InvalidCastException("암호화된 Byte[] 형식이 아닙니다.");
        }
        #endregion
        #region public byte[] GetBytes(string key, string name, byte[] defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public byte[] GetBytes(string key, string name, byte[] defaultValue = null)
        {
            if (string.IsNullOrEmpty(key)) return GetBytes(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetBytes(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public byte[] GetBytes(string name, byte[] defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public byte[] GetBytes(string name, byte[] defaultValue = null)
            => GetBytes(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static char GetChar(RegistryKey key, string name, char defaultValue, string CryptoKey)
        private static char GetChar(RegistryKey key, string name, char defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is char && (char)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (2 <= buffer.Length && buffer.Length <= 17) 
                    return buffer.ToChar();
            }
            throw new InvalidCastException("암호화된 Char 형식이 아닙니다.");
        }
        #endregion
        #region public char GetChar(string key, string name, char defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public char GetChar(string key, string name, char defaultValue = '\0')
        {
            if (string.IsNullOrEmpty(key)) return GetChar(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetChar(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public char GetChar(string name, char defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public char GetChar(string name, char defaultValue = '\0')
            => GetChar(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static short GetInt16(RegistryKey key, string name, short defaultValue, string CryptoKey)
        private static short GetInt16(RegistryKey key, string name, short defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is short && (short)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (2 <= buffer.Length && buffer.Length <= 17)
                    return buffer.ToInt16();
            }
            throw new InvalidCastException("암호화된 Int16 형식이 아닙니다.");
        }
        #endregion
        #region public short GetInt16(string key, string name, short defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public short GetInt16(string key, string name, short defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetInt16(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetInt16(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public short GetInt16(string name, short defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public short GetInt16(string name, short defaultValue = 0)
            => GetInt16(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static int GetInt32(RegistryKey key, string name, int defaultValue, string CryptoKey)
        private static int GetInt32(RegistryKey key, string name, int defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is int && (int)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (4 <= buffer.Length && buffer.Length <= 19)
                    return buffer.ToInt32();
            }
            throw new InvalidCastException("암호화된 Int32 형식이 아닙니다.");
        }
        #endregion
        #region public int GetInt32(string key, string name, int defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public int GetInt32(string key, string name, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetInt32(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetInt32(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public int GetInt32(string name, int defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public int GetInt32(string name, int defaultValue = 0)
            => GetInt32(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static long GetInt64(RegistryKey key, string name, long defaultValue, string CryptoKey)
        private static long GetInt64(RegistryKey key, string name, long defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is long && (long)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (8 <= buffer.Length && buffer.Length <= 23)
                    return buffer.ToInt32();
            }
            throw new InvalidCastException("암호화된 Int64 형식이 아닙니다.");
        }
        #endregion
        #region public long GetInt64(string key, string name, long defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public long GetInt64(string key, string name, long defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetInt64(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetInt64(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public long GetInt64(string name, long defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public long GetInt64(string name, long defaultValue = 0)
            => GetInt64(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static ushort GetUInt16(RegistryKey key, string name, ushort defaultValue, string CryptoKey)
        private static ushort GetUInt16(RegistryKey key, string name, ushort defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is ushort && (ushort)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (2 <= buffer.Length && buffer.Length <= 17)
                    return buffer.ToUInt16();
            }
            throw new InvalidCastException("암호화된 UInt16 형식이 아닙니다.");
        }
        #endregion
        #region public ushort GetUInt16(string key, string name, ushort defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public ushort GetUInt16(string key, string name, ushort defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetUInt16(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetUInt16(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public ushort GetUInt16(string name, ushort defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public ushort GetUInt16(string name, ushort defaultValue = 0)
            => GetUInt16(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static uint GetUInt32(RegistryKey key, string name, uint defaultValue, string CryptoKey)
        private static uint GetUInt32(RegistryKey key, string name, uint defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is uint && (uint)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (4 <= buffer.Length && buffer.Length <= 19)
                    return buffer.ToUInt32();
            }
            throw new InvalidCastException("암호화된 UInt32 형식이 아닙니다.");
        }
        #endregion
        #region public uint GetUInt32(string key, string name, uint defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public uint GetUInt32(string key, string name, uint defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetUInt32(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetUInt32(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public uint GetUInt32(string name, uint defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public uint GetUInt32(string name, uint defaultValue = 0)
            => GetUInt32(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static ulong GetUInt64(RegistryKey key, string name, ulong defaultValue, string CryptoKey)
        private static ulong GetUInt64(RegistryKey key, string name, ulong defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is ulong && (ulong)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (8 <= buffer.Length && buffer.Length <= 23)
                    return buffer.ToUInt32();
            }
            throw new InvalidCastException("암호화된 UInt64 형식이 아닙니다.");
        }
        #endregion
        #region public ulong GetUInt64(string key, string name, ulong defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public ulong GetUInt64(string key, string name, ulong defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetUInt64(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetUInt64(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public ulong GetUInt64(string name, ulong defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public ulong GetUInt64(string name, ulong defaultValue = 0)
            => GetUInt64(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static float GetSingle(RegistryKey key, string name, float defaultValue, string CryptoKey)
        private static float GetSingle(RegistryKey key, string name, float defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is float && (float)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (4 <= buffer.Length && buffer.Length <= 19)
                    return buffer.ToSingle();
            }
            throw new InvalidCastException("암호화된 Single 형식이 아닙니다.");
        }
        #endregion
        #region public float GetSingle(string key, string name, float defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public float GetSingle(string key, string name, float defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetSingle(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetSingle(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public float GetSingle(string name, float defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public float GetSingle(string name, float defaultValue = 0)
            => GetSingle(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static double GetDouble(RegistryKey key, string name, double defaultValue, string CryptoKey)
        private static double GetDouble(RegistryKey key, string name, double defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is double && (double)value == defaultValue) return defaultValue;
            else if (value is byte[])
            {
                byte[] buffer = ((byte[])value).AES256Decrypt(CryptoKey);
                if (8 <= buffer.Length && buffer.Length <= 23)
                    return buffer.ToInt32();
            }
            throw new InvalidCastException("암호화된 Int64 형식이 아닙니다.");
        }
        #endregion
        #region public double GetDouble(string key, string name, double defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public double GetDouble(string key, string name, double defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return GetDouble(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetDouble(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public double GetDouble(string name, double defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public double GetDouble(string name, double defaultValue = 0)
            => GetDouble(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #region private static string GetString(RegistryKey key, string name, string defaultValue, string CryptoKey)
        private static string GetString(RegistryKey key, string name, string defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is string && (string)value == defaultValue) return defaultValue;
            else if (value == null && defaultValue == null) return null;
            else if (value is byte[]) return ((byte[])value).AES256Decrypt(CryptoKey).GetString();
            else throw new InvalidCastException("암호화된 String 형식이 아닙니다.");
        }
        #endregion
        #region public string GetString(string key, string name, string defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public string GetString(string key, string name, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key)) return GetString(RootKey, name, defaultValue, CryptoKey);
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                return GetString(regKey, name, defaultValue, CryptoKey);
            }
        }
        #endregion
        #region public string GetString(string name, string defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public string GetString(string name, string defaultValue = "")
            => GetString(RootKey, name, defaultValue, CryptoKey);
        #endregion

        #endregion

        #region [    Set Value    ]

        #region public void SetValue(string key, string name, bool value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, bool value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, bool value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, bool value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, byte value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, byte value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = new byte[] { value }.Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, byte value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, byte value)
            => RootKey.SetValue(name.HashSHA256(), new byte[] { value }.Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, byte[] value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, byte[] value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, byte[] value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, byte[] value)
            => RootKey.SetValue(name.HashSHA256(), value.AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, char value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, char value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, char value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, char value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, short value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, short value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, short value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, short value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, int value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, int value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, int value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, int value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, long value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, long value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, long value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, long value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, ushort value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, ushort value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, ushort value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, ushort value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, uint value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, uint value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, uint value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, uint value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, ulong value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, ulong value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, ulong value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, ulong value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, float value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, float value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, float value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, float value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, double value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, double value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, double value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, double value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        #endregion

        #region public void SetValue(string key, string name, string value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, string value)
        {
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().AES256Encrypt(CryptoKey);
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(hashedName, encryptedValue);
            else using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey == null)
                        using (RegistryKey genKey = RootKey.CreateSubKey(key.HashSHA256()))
                            genKey.SetValue(hashedName, encryptedValue);
                    else regKey.SetValue(hashedName, encryptedValue);
        }
        #endregion
        #region public void SetValue(string name, string value)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, string value)
            => RootKey.SetValue(name.HashSHA256(), value.GetBytes().AES256Encrypt(CryptoKey));
        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 중복 호출을 검색하려면

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 관리되는 상태(관리되는 개체)를 삭제합니다.
                    if (RootKey is RegistryKey)
                        RootKey.Dispose();
                }

                // TODO: 관리되지 않는 리소스(관리되지 않는 개체)를 해제하고 아래의 종료자를 재정의합니다.
                // TODO: 큰 필드를 null로 설정합니다.

                disposedValue = true;
            }
        }

        // 삭제 가능한 패턴을 올바르게 구현하기 위해 추가된 코드입니다.
        public void Dispose() => Dispose(true);
        #endregion
    }

    ///// <summary>
    ///// <see cref="RegistryComponent"/>으로 구현한 설정 예시입니다.
    ///// Property를 사용하여 만드는 방식입니다.
    ///// </summary>
    //internal /*static*/ class CryptoRegistrySample
    //{
    //    private /*static*/ readonly CryptoRegistryComponent com = new CryptoRegistryComponent("Sample", "SampleKey");
    //    /// <summary>
    //    /// <see cref="string"/> 값을 가져오거나 설정하는 예제입니다.
    //    /// </summary>
    //    public /*static*/ string StringExample {
    //        get => com.GetString(nameof(StringExample));
    //        set => com.SetValue(nameof(StringExample), value);
    //    }
    //    /// <summary>
    //    /// <see cref="int"/> 값을 가져오거나 설정하는 예제입니다.
    //    /// </summary>
    //    public /*static*/ int IntExample {
    //        get => com.GetInt32(nameof(IntExample));
    //        set => com.SetValue(nameof(IntExample), value);
    //    }
    //    /// <summary>
    //    /// <see cref="bool"/> 값을 가져오거나 설정하는 예제입니다.
    //    /// </summary>
    //    public /*static*/ bool BoolExample {
    //        get => com.GetBoolean(nameof(BoolExample));
    //        set => com.SetValue(nameof(BoolExample), value);
    //    }
    //    /// <summary>
    //    /// <see cref="float"/> 값을 가져오거나 설정하는 예제입니다.
    //    /// 다른 대부분의 값도 이러한 형태로 제작됩니다.
    //    /// </summary>
    //    public /*static*/ float FloatExample {
    //        get => com.GetSingle(nameof(FloatExample));
    //        set => com.SetValue(nameof(FloatExample), value);
    //    }
    //}
}