using System;
using System.Collections.Generic;
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

        #region public readonly bool UseCache
        /// <summary>
        /// 개체에 쓰기 전에 캐시에 먼저 담아둘 지의 여부를 결정합니다.
        /// </summary>
        public readonly bool UseCache;
        #endregion

        #region private Dictionary<string, object> Cache
        /// <summary>
        /// 개체에 쓰기 전에 담아두는 캐시 입니다.
        /// </summary>
        private Dictionary<string, object> Cache;
        #endregion

        #region public CryptoRegistryComponent(string RootPath, string CryptoKey)
        /// <summary>
        /// 개체를 사용자가 제공하는 키를 루트로 초기화 합니다.
        /// </summary>
        /// <param name="RootPath">개체를 초기화할 루트 키 입니다.</param>
        /// <param name="CryptoKey">개체를 암복호화할 암호키 입니다.</param>
        /// <param name="UseCache">개체에 쓰기 전에 캐시에 먼저 담아둘 지의 여부를 결정합니다.</param>
        public CryptoRegistryComponent(string RootPath, string CryptoKey, bool UseCache)
        {
            if (string.IsNullOrEmpty(RootPath)) throw new ArgumentException("키가 제공되지 않았습니다.");
            this.CryptoKey = CryptoKey; // ?? throw new ArgumentNullException("암호키가 제공되지 않았습니다.");
            string path = $"Software\\{RootPath}";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            RootKey = key ?? Registry.CurrentUser.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (this.UseCache = UseCache) Cache = new Dictionary<string, object>();
        }
        #endregion

        #region public void Clear()
        /// <summary>
        /// <see cref="RootKey"/>내에 존재하는 모든 키와 값을 재귀적으로 삭제합니다.
        /// </summary>
        public void Clear()
        {
            if (UseCache) Cache.Clear();
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

        private void SetCache(string key, object value)
        {
            if (Cache.ContainsKey(key))
                Cache[key] = value;
            else
                Cache.Add(key, value);
        }

        #region public void DeleteValue(string key, string name)
        /// <summary>
        /// 지정된 값을 지정된 하위 키에서 삭제합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">삭제할 값의 이름입니다.</param>
        public void DeleteValue(string key, string name)
        {
            if (string.IsNullOrEmpty(key))
                DeleteValue(name.HashSHA256());
            else
            {
                if (UseCache) Cache.Remove($"{key}\\{name}");
                using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), true))
                    if (regKey != null)
                        regKey.DeleteValue(name.HashSHA256(), false);
            }
        }
        #endregion
        #region public void DeleteValue(string name)
        /// <summary>
        /// 지정된 값을 이 키에서 삭제합니다.
        /// </summary>
        /// <param name="name">삭제할 값의 이름입니다.</param>
        public void DeleteValue(string name = null)
        {
            if (UseCache) Cache.Remove(name);
            RootKey.DeleteValue(name.HashSHA256(), false);
        }
        #endregion

        #region [    Get Value    ]

        #region private bool GetBoolean(RegistryKey key, string name, bool defaultValue, string CryptoKey)
        private bool GetBoolean(RegistryKey key, string name, bool defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is bool Bool && Bool == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            bool ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetBoolean(name, defaultValue);
            else if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (bool)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetBoolean(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (bool)Cache[name];
            bool ret = GetBoolean(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static byte GetByte(RegistryKey key, string name, byte defaultValue, string CryptoKey)
        private static byte GetByte(RegistryKey key, string name, byte defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is byte Byte && Byte == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            byte ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetByte(name, defaultValue);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (byte)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetByte(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (byte)Cache[name];
            byte ret = GetByte(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
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
            byte[] ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetBytes(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (byte[])Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetBytes(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (byte[])Cache[name];
            byte[] ret = GetBytes(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static char GetChar(RegistryKey key, string name, char defaultValue, string CryptoKey)
        private static char GetChar(RegistryKey key, string name, char defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is char Char && Char == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            char ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetChar(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (char)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetChar(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (char)Cache[name];
            char ret = GetChar(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static short GetInt16(RegistryKey key, string name, short defaultValue, string CryptoKey)
        private static short GetInt16(RegistryKey key, string name, short defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is short Short && Short == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            short ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetInt16(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (short)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetInt16(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (short)Cache[name];
            short ret = GetInt16(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static int GetInt32(RegistryKey key, string name, int defaultValue, string CryptoKey)
        private static int GetInt32(RegistryKey key, string name, int defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is int Int && Int == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            int ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetInt32(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (int)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetInt32(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (int)Cache[name];
            int ret = GetInt32(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static long GetInt64(RegistryKey key, string name, long defaultValue, string CryptoKey)
        private static long GetInt64(RegistryKey key, string name, long defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is long Long && Long == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            long ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetInt64(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (long)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetInt64(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (long)Cache[name];
            long ret = GetInt64(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static ushort GetUInt16(RegistryKey key, string name, ushort defaultValue, string CryptoKey)
        private static ushort GetUInt16(RegistryKey key, string name, ushort defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is ushort UShort && UShort == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            ushort ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetUInt16(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (ushort)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetUInt16(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (ushort)Cache[name];
            ushort ret = GetUInt16(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static uint GetUInt32(RegistryKey key, string name, uint defaultValue, string CryptoKey)
        private static uint GetUInt32(RegistryKey key, string name, uint defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is uint UInt && UInt == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            uint ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetUInt32(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (uint)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetUInt32(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (uint)Cache[name];
            uint ret = GetUInt32(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static ulong GetUInt64(RegistryKey key, string name, ulong defaultValue, string CryptoKey)
        private static ulong GetUInt64(RegistryKey key, string name, ulong defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is ulong ULong && ULong == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            ulong ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetUInt64(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (ulong)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetUInt64(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (ulong)Cache[name];
            ulong ret = GetUInt64(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static float GetSingle(RegistryKey key, string name, float defaultValue, string CryptoKey)
        private static float GetSingle(RegistryKey key, string name, float defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is float Float && Float == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            float ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetSingle(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (float)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetSingle(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (float)Cache[name];
            float ret = GetSingle(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static double GetDouble(RegistryKey key, string name, double defaultValue, string CryptoKey)
        private static double GetDouble(RegistryKey key, string name, double defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is double Double && Double == defaultValue) return defaultValue;
            else if (value is byte[] v)
            {
                byte[] buffer = v.AES256Decrypt(CryptoKey);
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
            double ret;
            string path = null;
            if (string.IsNullOrEmpty(key)) return GetDouble(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (double)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetDouble(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (double)Cache[name];
            double ret = GetDouble(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
        #endregion

        #region private static string GetString(RegistryKey key, string name, string defaultValue, string CryptoKey)
        private static string GetString(RegistryKey key, string name, string defaultValue, string CryptoKey)
        {
            object value = key.GetValue(name.HashSHA256(), defaultValue);
            if (value is string String && String == defaultValue) return defaultValue;
            else if (value == null && defaultValue == null) return null;
            else if (value is byte[] v) return v.AES256Decrypt(CryptoKey).GetString();
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
            string ret, path = null;
            if (string.IsNullOrEmpty(key)) return GetString(RootKey, name, defaultValue, CryptoKey);
            if (UseCache && Cache.ContainsKey(path = $"{key}\\{name}")) return (string)Cache[path];
            using (RegistryKey regKey = RootKey.OpenSubKey(key.HashSHA256(), false))
            {
                if (regKey == null) return defaultValue;
                ret = GetString(regKey, name, defaultValue, CryptoKey);
            }
            if (UseCache) SetCache(path, ret);
            return ret;
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
        {
            if (UseCache && Cache.ContainsKey(name)) return (string)Cache[name];
            string ret = GetString(RootKey, name, defaultValue, CryptoKey);
            if (UseCache) SetCache(name, ret);
            return ret;
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = new byte[] { value }.Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), new byte[] { value }.Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().Append(new byte[0x10.GetRandom()].GetRandom()).AES256Encrypt(CryptoKey));
        }
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
            bool EmptyKey = string.IsNullOrEmpty(key);
            if (UseCache)
                if (EmptyKey)
                    SetCache(name, value);
                else
                    SetCache($"{key}\\{name}", value);
            string hashedName = name.HashSHA256();
            byte[] encryptedValue = value.GetBytes().AES256Encrypt(CryptoKey);
            if (EmptyKey) RootKey.SetValue(hashedName, encryptedValue);
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
        {
            if (UseCache) SetCache(name, value);
            RootKey.SetValue(name.HashSHA256(), value.GetBytes().AES256Encrypt(CryptoKey));
        }
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