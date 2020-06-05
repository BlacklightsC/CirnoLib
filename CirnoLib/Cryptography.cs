using System;
using System.IO;
using System.Security.Cryptography;

namespace CirnoLib
{
    public static class Cryptography
    {
        public static byte[] AES256Encrypt(this string Input, string Password)
            => Input.GetBytes().AES256Encrypt(Password);
        public static byte[] AES256Encrypt(this byte[] Input, string Password)
        {
            using (RijndaelManaged RijndaelCipher = new RijndaelManaged())
            {
                byte[] CipherBytes;
                //byte[] SaltBytes = new byte[8];
                //using (RNGCryptoServiceProvider SaltRNG = new RNGCryptoServiceProvider()) SaltRNG.GetBytes(SaltBytes);
                using (PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Password.GetBytes()))
                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16)), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(Input, 0, Input.Length);
                    cryptoStream.FlushFinalBlock();

                    CipherBytes = memoryStream.ToArray();

                    cryptoStream.Close();
                    memoryStream.Close();
                }
                return CipherBytes;
            }
        }

        public static byte[] AES256Decrypt(this string Input, string Password)
            => Input.GetBytes().AES256Decrypt(Password);
        public static byte[] AES256Decrypt(this byte[] Input, string Password)
        {
            using (RijndaelManaged RijndaelCipher = new RijndaelManaged())
            {
                byte[] DecryptedBytes = new byte[Input.Length];
                int DecryptedCount;
                using (PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Password.GetBytes()))
                using (MemoryStream memoryStream = new MemoryStream(Input))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16)), CryptoStreamMode.Read))
                {
                    DecryptedCount = cryptoStream.Read(DecryptedBytes, 0, DecryptedBytes.Length);

                    cryptoStream.Close();
                    memoryStream.Close();
                }
                if (DecryptedBytes.Length != DecryptedCount) Array.Resize(ref DecryptedBytes, DecryptedCount);
                return DecryptedBytes;
            }
        }

        #region public static void GenerateRSAKey(out string pubKey, out string priKey)
        /// <summary>
        /// 무작위로 생성된 공개키와 그와 일치하는 개인키가 들어 있는 XML 문자열을 만들고 반환합니다.
        /// </summary>
        /// <param name="pubKey">공개키가 들어 있는 XML 문자열이 저장될 <see cref="string"/>입니다.</param>
        /// <param name="priKey">개인키가 들어 있는 XML 문자열이 저장될 <see cref="string"/>입니다.</param>
        public static void GenerateRSAKey(out string pubKey, out string priKey)
        {
            using (RSACryptoServiceProvider RSACipher = new RSACryptoServiceProvider())
            {
                RSAParameters priKeyParams = RSA.Create().ExportParameters(true);
                RSACipher.ImportParameters(priKeyParams);
                priKey = RSACipher.ToXmlString(true);

                RSAParameters pubKeyParams = new RSAParameters
                {
                    Modulus = priKeyParams.Modulus,
                    Exponent = priKeyParams.Exponent
                };
                RSACipher.ImportParameters(pubKeyParams);
                pubKey = RSACipher.ToXmlString(false);
            }
        }
        #endregion

        #region public static byte[] RSAEncrypt(this string input, string key, bool fOAEP)
        /// <summary>
        /// XML 문자열의 키 정보를 사용한 <see cref="RSA"/> 개체의 알고리즘으로 데이터를 암호화합니다.
        /// </summary>
        /// <param name="input">암호화할 데이터입니다.</param>
        /// <param name="key"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <param name="fOAEP">OAEP 안쪽 여백(Windows XP 이상을 실행하는 컴퓨터에서만 사용 가능)을 사용하여 직접 <see cref="RSA"/> 암호화를 수행하려면 <see langword="true"/>이고, PKCS#1 v1.5 안쪽 여백을 사용하려면 <see langword="false"/>입니다.</param>
        /// <returns>암호화된 데이터입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException"><paramref name="key"/> 매개 변수의 형식이 올바르지 않습니다.또는 <paramref name="input"/> 매개 변수의 길이가 최대 허용 길이보다 큽니다. 또는 <paramref name="fOAEP"/> 매개 변수가 true이고 OAEP가 지원되지 않습니다.</exception>
        public static byte[] RSAEncrypt(this string input, string key, bool fOAEP = false)
            => input.GetBytes().RSAEncrypt(key, fOAEP);
        #endregion
        #region public static byte[] RSAEncrypt(this byte[] input, string key, bool fOAEP)
        /// <summary>
        /// XML 문자열의 키 정보를 사용한 <see cref="RSA"/> 개체의 알고리즘으로 데이터를 암호화합니다.
        /// </summary>
        /// <param name="input">암호화할 데이터입니다.</param>
        /// <param name="key"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <param name="fOAEP">OAEP 안쪽 여백(Windows XP 이상을 실행하는 컴퓨터에서만 사용 가능)을 사용하여 직접 <see cref="RSA"/> 암호화를 수행하려면 <see langword="true"/>이고, PKCS#1 v1.5 안쪽 여백을 사용하려면 <see langword="false"/>입니다.</param>
        /// <returns>암호화된 데이터입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException"><paramref name="key"/> 매개 변수의 형식이 올바르지 않습니다.또는 <paramref name="input"/> 매개 변수의 길이가 최대 허용 길이보다 큽니다. 또는 <paramref name="fOAEP"/> 매개 변수가 true이고 OAEP가 지원되지 않습니다.</exception>
        public static byte[] RSAEncrypt(this byte[] input, string key, bool fOAEP = false)
        {
            using (RSACryptoServiceProvider RSACipher = new RSACryptoServiceProvider())
            {
                RSACipher.FromXmlString(key);
                return RSACipher.Encrypt(input, fOAEP);
            }
        }
        #endregion

        #region public static byte[] RSADecrypt(this string input, string priKey, bool fOAEP)
        /// <summary>
        /// XML 문자열의 키 정보를 사용한 <see cref="RSA"/> 개체의 알고리즘에 따라 데이터를 해독합니다.
        /// </summary>
        /// <param name="input">해독할 데이터입니다.</param>
        /// <param name="priKey"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <param name="fOAEP">OAEP 안쪽 여백(Windows XP 이상을 실행하는 컴퓨터에서만 사용 가능)을 사용하여 직접 <see cref="RSA"/> 암호화를 수행하려면 <see langword="true"/>이고, PKCS#1 v1.5 안쪽 여백을 사용하려면 <see langword="false"/>입니다.</param>
        /// <returns>해독된 데이터로, 암호화하기 전의 원래 일반 텍스트입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="priKey"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException">
        /// <paramref name="priKey"/> 매개 변수의 형식이 올바르지 않습니다.
        /// 또는 <paramref name="fOAEP"/> 매개 변수가 <see langword="true"/>이고 <paramref name="priKey"/> 매개 변수의 길이가 <see cref="RSACryptoServiceProvider.KeySize"/>보다 큽니다.
        /// 또는 <paramref name="fOAEP"/> 매개 변수가 <see langword="true"/>이고 OAEP가 지원되지 않습니다.
        /// 또는 키가 암호화된 데이터와 일치하지 않습니다. 그러나 예외 표현이 정확하지 않을 수 있습니다.
        /// 예를 들어 Not enough storage is available to process this command로 나타날 수 있습니다.
        /// </exception>
        public static byte[] RSADecrypt(this string input, string priKey, bool fOAEP = false)
            => input.GetBytes().RSADecrypt(priKey, fOAEP);
        #endregion
        #region  public static byte[] RSADecrypt(this byte[] input, string priKey, bool fOAEP)
        /// <summary>
        /// XML 문자열의 키 정보를 사용한 <see cref="RSA"/> 개체의 알고리즘에 따라 데이터를 해독합니다.
        /// </summary>
        /// <param name="input">해독할 데이터입니다.</param>
        /// <param name="priKey"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <param name="fOAEP">OAEP 안쪽 여백(Windows XP 이상을 실행하는 컴퓨터에서만 사용 가능)을 사용하여 직접 <see cref="RSA"/> 암호화를 수행하려면 <see langword="true"/>이고, PKCS#1 v1.5 안쪽 여백을 사용하려면 <see langword="false"/>입니다.</param>
        /// <returns>해독된 데이터로, 암호화하기 전의 원래 일반 텍스트입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="priKey"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException">
        /// <paramref name="priKey"/> 매개 변수의 형식이 올바르지 않습니다.
        /// 또는 <paramref name="fOAEP"/> 매개 변수가 <see langword="true"/>이고 <paramref name="priKey"/> 매개 변수의 길이가 <see cref="RSACryptoServiceProvider.KeySize"/>보다 큽니다.
        /// 또는 <paramref name="fOAEP"/> 매개 변수가 <see langword="true"/>이고 OAEP가 지원되지 않습니다.
        /// 또는 키가 암호화된 데이터와 일치하지 않습니다. 그러나 예외 표현이 정확하지 않을 수 있습니다.
        /// 예를 들어 Not enough storage is available to process this command로 나타날 수 있습니다.
        /// </exception>
        public static byte[] RSADecrypt(this byte[] input, string priKey, bool fOAEP = false)
        {
            using (RSACryptoServiceProvider RSACipher = new RSACryptoServiceProvider())
            {
                RSACipher.FromXmlString(priKey);
                return RSACipher.Decrypt(input, fOAEP);
            }
        }
        #endregion

        #region public static byte[] RSASignData(this string input, string priKey)
        /// <summary>
        /// <see cref="SHA512"/> 해시 알고리즘을 사용하여 지정된 바이트 배열의 해시 값을 계산하고 결과 해시 값을 서명합니다.
        /// </summary>
        /// <param name="input">해시를 계산하기 위한 입력 데이터입니다.</param>
        /// <param name="priKey"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <returns>지정된 데이터의 <see cref="RSA"/> 서명입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="priKey"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException"><paramref name="priKey"/> 매개 변수의 형식이 올바르지 않습니다.</exception>
        public static byte[] RSASignData(this string input, string priKey)
            => input.GetBytes().RSASignData(priKey);
        #endregion
        #region public static byte[] RSASignData(this byte[] input, string priKey)
        /// <summary>
        /// <see cref="SHA512"/> 해시 알고리즘을 사용하여 지정된 바이트 배열의 해시 값을 계산하고 결과 해시 값을 서명합니다.
        /// </summary>
        /// <param name="input">해시를 계산하기 위한 입력 데이터입니다.</param>
        /// <param name="priKey"><see cref="RSA"/> 키 정보가 들어 있는 XML 문자열입니다.</param>
        /// <returns>지정된 데이터의 <see cref="RSA"/> 서명입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="priKey"/> 매개 변수가 <see langword="null"/>이거나 <paramref name="input"/>이 <see langword="null"/>입니다.</exception>
        /// <exception cref="CryptographicException"><paramref name="priKey"/> 매개 변수의 형식이 올바르지 않습니다.</exception>
        public static byte[] RSASignData(this byte[] input, string priKey)
        {
            using (RSACryptoServiceProvider RSACipher = new RSACryptoServiceProvider())
            {
                RSACipher.FromXmlString(priKey);
                using (SHA512CryptoServiceProvider SHACipher = new SHA512CryptoServiceProvider())
                    return RSACipher.SignData(input, SHACipher);
            }
        }
        #endregion

        public static bool RSAVerifyData(this string input, string key, byte[] signature)
            => input.GetBytes().RSAVerifyData(key, signature);
        public static bool RSAVerifyData(this byte[] input, string key, byte[] signature)
        {
            using (RSACryptoServiceProvider RSACipher = new RSACryptoServiceProvider())
            {
                RSACipher.FromXmlString(key);
                using (SHA512CryptoServiceProvider SHACipher = new SHA512CryptoServiceProvider())
                    return RSACipher.VerifyData(input, SHACipher, signature);
            }
        }

        public static string HashMD5(this string Text)
            => Text.GetBytes().HashMD5();
        public static string HashMD5(this byte[] Data)
        {
            using (var MD5 = new MD5CryptoServiceProvider())
                return MD5.ComputeHash(Data).GetText();
        }

        public static string HashSHA256(this string Text)
            => Text.GetBytes().HashSHA256();
        public static string HashSHA256(this byte[] Data)
        {
            using (var SHA256 = new SHA256Managed())
                return SHA256.ComputeHash(Data).GetText();
        }

        public static string HashSHA384(this string Text)
            => Text.GetBytes().HashSHA384();
        public static string HashSHA384(this byte[] Data)
        {
            using (var SHA384 = new SHA384Managed())
                return SHA384.ComputeHash(Data).GetText();
        }

        public static string HashSHA512(this string Text)
            => Text.GetBytes().HashSHA512();
        public static string HashSHA512(this byte[] Data)
        {
            using (var SHA512 = new SHA512Managed())
                return SHA512.ComputeHash(Data).GetText();
        }
    }
}
