using System;
using Microsoft.Win32;

namespace CirnoLib.Settings
{
    /// <summary>
    /// <see cref="Registry"/>를 좀 더 손쉽게 사용하기 위한 클래스입니다.
    /// </summary>
    public sealed class RegistryComponent : IDisposable
    {
        #region private readonly RegistryKey RootKey
        /// <summary>
        /// 개체가 가르키는 키의 루트입니다.
        /// </summary>
        private readonly RegistryKey RootKey;
        #endregion

        #region public RegistryComponent(string RootPath)
        /// <summary>
        /// 개체를 사용자가 제공하는 키를 루트로 초기화 합니다.
        /// </summary>
        /// <param name="RootPath">개체를 초기화할 루트 키 입니다.</param>
        public RegistryComponent(string RootPath)
        {
            if (string.IsNullOrEmpty(RootPath))
                throw new NotImplementedException("키가 제공되지 않았습니다.");
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

        #region public void DeleteValue(string name)
        /// <summary>
        /// 지정된 값을 이 키에서 삭제합니다.
        /// </summary>
        /// <param name="name">삭제할 값의 이름입니다.</param>
        public void DeleteValue(string name = null) => RootKey.DeleteValue(name, false);
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
                DeleteValue(name);
            }
            else
            {
                using (RegistryKey regKey = RootKey.OpenSubKey(key, true))
                {
                    if (regKey != null)
                    {
                        regKey.DeleteValue(name, false);
                    }
                }
            }
        }
        #endregion

        #region public object GetValue(string name, object defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다.
        /// </summary>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public object GetValue(string name, object defaultValue = null) => RootKey.GetValue(name, defaultValue);
        #endregion

        #region public object GetValue(string key, string name, object defaultValue)
        /// <summary>
        /// 지정된 이름 및 검색 옵션과 연결된 값을 검색합니다. 이름이 없으면 사용자가 제공하는 기본값을 반환합니다. 
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">검색할 값의 이름입니다. 이 문자열은 대/소문자를 구분하지 않습니다.</param>
        /// <param name="defaultValue">Name 이 존재하지 않을 경우 반환하는 값 입니다.</param>
        /// <returns><paramref name="name"/> 의 값을 반환합니다. 존재하지 않을 경우 <paramref name="defaultValue"/> 의 값을 반환합니다.</returns>
        public object GetValue(string key, string name, object defaultValue)
        {
            if (string.IsNullOrEmpty(key)) return RootKey.GetValue(name, defaultValue);
            using (RegistryKey regKey = RootKey.OpenSubKey(key, false))
            {
                if (regKey == null) return defaultValue;
                return regKey.GetValue(name, defaultValue);
            }
        }
        #endregion

        #region public void SetValue(string name, object value = null)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, object value = null) => RootKey.SetValue(name, value);
        #endregion

        #region public void SetValue(string name, bool value = null)
        /// <summary>
        /// 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string name, bool value) => RootKey.SetValue(name, value ? 1 : 0);
        #endregion

        #region public void SetValue(string key, string name, object value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, object value)
        {
            if (string.IsNullOrEmpty(key)) RootKey.SetValue(name, value);
            using (RegistryKey regKey = RootKey.OpenSubKey(key, false))
            {
                if (regKey == null)
                {
                    using (RegistryKey genKey = RootKey.CreateSubKey(key))
                    {
                        genKey.SetValue(name, value);
                    }
                }
                else
                {
                    regKey.SetValue(name, value);
                }
            }
        }
        #endregion

        #region public void SetValue(string key, string name, bool value)
        /// <summary>
        /// 지정된 키 내부의, 지정된 이름/값 쌍을 설정합니다.
        /// </summary>
        /// <param name="key">이름 또는 열려는 하위 키의 경로입니다.</param>
        /// <param name="name">저장할 값의 이름입니다.</param>
        /// <param name="value">저장할 값입니다.</param>
        public void SetValue(string key, string name, bool value) => SetValue(key, name, value ? 1 : 0);
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

    /// <summary>
    /// <see cref="RegistryComponent"/>으로 구현한 설정 예시입니다.
    /// Property를 사용하여 만드는 방식입니다.
    /// </summary>
    internal /*static*/ class RegistrySample
    {
        private /*static*/ readonly RegistryComponent com = new RegistryComponent("Sample");
        /// <summary>
        /// <see cref="string"/> 값을 가져오거나 설정하는 예제입니다.
        /// </summary>
        public /*static*/ string StringExample {
            get => com.GetValue(nameof(StringExample), string.Empty) as string;
            set => com.SetValue(nameof(StringExample), value);
        }
        /// <summary>
        /// <see cref="int"/> 값을 가져오거나 설정하는 예제입니다.
        /// </summary>
        public /*static*/ int IntExample {
            get => (int)com.GetValue(nameof(IntExample), 0);
            set => com.SetValue(nameof(IntExample), value);
        }
        /// <summary>
        /// <see cref="bool"/> 값을 가져오거나 설정하는 예제입니다.
        /// </summary>
        public /*static*/ bool BoolExample {
            get => (int)com.GetValue(nameof(BoolExample), 0) == 1;
            set => com.SetValue(nameof(BoolExample), value ? 1 : 0);
        }
        /// <summary>
        /// <see cref="float"/> 값을 가져오거나 설정하는 예제입니다.
        /// 다른 대부분의 값도 이러한 형태로 제작됩니다.
        /// </summary>
        public /*static*/ float FloatExample {
            get => float.Parse(com.GetValue(nameof(FloatExample), "0") as string);
            set => com.SetValue(nameof(FloatExample), value);
        }
    }
}