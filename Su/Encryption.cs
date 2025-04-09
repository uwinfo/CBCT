using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Su
{
    public class Encryption
    {
        /// <summary>
        ///     ''' Encrypt text with DES method.
        ///     ''' </summary>
        ///     ''' <param name="SourceToEncrypt">String to be encrypted.</param>
        ///     ''' <param name="Key">key with 8 digits length</param>
        ///     ''' <returns></returns>
        public static string GetDESEncrypt(string SourceToEncrypt, string Key)
        {
            if (Key.Length != 8)
                return "Error! Key Length error!";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;

            inputByteArray = Encoding.UTF8.GetBytes(SourceToEncrypt);
            des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            des.IV = ASCIIEncoding.ASCII.GetBytes(Key);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
                ret.AppendFormat("{0:X2}", b);

            return ret.ToString();
        }


        /// <summary>
        ///     ''' Encrypt text with 3DES method.
        ///     ''' </summary>
        ///     ''' <param name="SourceToEncrypt">String to be encrypted.</param>
        ///     ''' <param name="Key">key with 24 digits length</param>
        ///     ''' <returns></returns>
        public static string Get3DESEncrypt(string SourceToEncrypt, string Key)
        {
            if (Key.Length != 24)
                return "Error! Key Length error!";
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            byte[] inputByteArray;

            inputByteArray = Encoding.UTF8.GetBytes(SourceToEncrypt);
            des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            des.IV = ASCIIEncoding.ASCII.GetBytes(Key.Substring(0, 8));

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
                ret.AppendFormat("{0:X2}", b);

            return ret.ToString();
        }

        /// <summary>
        ///     ''' Decrypt text with DES method
        ///     ''' </summary>
        ///     ''' <param name="SourceToDecrypt">String to be decrypted.</param>
        ///     ''' <param name="Key">Key with 8 digits length</param>
        ///     ''' <returns></returns>
        public static string GetDESDecrypt(string SourceToDecrypt, string Key)
        {
            if (Key.Length != 8)
                return "Error! Key Length error!";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len = SourceToDecrypt.Length / 2;
            byte[] inputByteArray = new byte[len - 1 + 1];

            for (int x = 0; x <= len - 1; x++)
            {
                int i = Convert.ToInt32(SourceToDecrypt.Substring(x * 2, 2), 16);
                inputByteArray[x] = System.Convert.ToByte(i);
            }

            des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            des.IV = ASCIIEncoding.ASCII.GetBytes(Key);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);

            try
            {
                cs.FlushFinalBlock();
            }
            catch
            {
                return "Error! Key Changed! Decrypt Fail!";
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }


        /// <summary>
        ///     ''' Decrypt text with 3DES method
        ///     ''' </summary>
        ///     ''' <param name="SourceToDecrypt">String to be decrypted.</param>
        ///     ''' <param name="Key">key with 24 digits length</param>
        ///     ''' <returns></returns>
        public static string Get3DESDecrypt(string SourceToDecrypt, string Key)
        {
            if (Key.Length != 24)
                return "Error! Key Length error!";
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            int len = SourceToDecrypt.Length / 2;
            byte[] inputByteArray = new byte[len - 1 + 1];

            for (int x = 0; x <= len - 1; x++)
            {
                int i = Convert.ToInt32(SourceToDecrypt.Substring(x * 2, 2), 16);
                inputByteArray[x] = System.Convert.ToByte(i);
            }

            des.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            des.IV = ASCIIEncoding.ASCII.GetBytes(Key.Substring(0, 8));

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);
            try
            {
                cs.FlushFinalBlock();
            }
            catch
            {
                return "Error! Key Changed! Decrypt Fail!";
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }


        /// <summary>
        /// 回傳值是大寫有 -, 例如: DE-18-2B-6D-4E-BF-87-F2-84-7A-A2-50-02-BB-2F-B0
        /// </summary>
        /// <param name="source"></param>
        /// <returns>32 digits string(大寫有 -)</returns>
        public static string GetMD5HashString(string source)
        {
            byte[] data2ToHash = (new UTF8Encoding()).GetBytes(source);
            byte[] hashvalue2 = new MD5CryptoServiceProvider().ComputeHash(data2ToHash);
            return BitConverter.ToString(hashvalue2);
        }

        /// <summary>
        /// 回傳 32 位小寫的 md5 結果
        /// </summary>
        /// <param name="stringToHash"></param>
        /// <returns></returns>
        public static string MD5(string stringToHash)
        {
            byte[] data2ToHash = (new UTF8Encoding()).GetBytes(stringToHash);
            byte[] hashvalue2 = new MD5CryptoServiceProvider().ComputeHash(data2ToHash);

            return ByteToString(hashvalue2).ToLower();
        }

        /// <summary>
        /// 同 php 的 hash_hmac
        /// </summary>
        /// <param name="message"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string hash_hmac(string message, string key)
        {
            Encoding encoding = Encoding.UTF8;
            var keyByte = encoding.GetBytes(key);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                hmacsha256.ComputeHash(encoding.GetBytes(message));

                return ByteToString(hmacsha256.Hash).ToLower();
            }
        }

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }

        /// <summary>
        /// 去除 - 之後的 MD5, 字母為大寫
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetMD5HashString_NoHypen(string source)
        {
            return GetMD5HashString(source).Replace("-", "");
        }


        /// <summary>
        ///     ''' Hash text with SHA1 method (UTF8)
        ///     ''' </summary>
        ///     ''' <param name="source">String to be hashed</param>
        ///     ''' <returns>40 digits string</returns>
        public static string GetSHA1HashString(string source)
        {
            byte[] data2ToHash = (new UTF8Encoding()).GetBytes(source);
            byte[] hashvalue2 = new SHA1CryptoServiceProvider().ComputeHash(data2ToHash);
            return BitConverter.ToString(hashvalue2).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Hash text with SHA256 method (UTF8)
        /// </summary>
        /// <param name="source">String to be hashed</param>
        /// <returns>回傳 64 個字元, 小寫, 無 "-"</returns>
        public static string GetSHA256HashString(string source)
        {
            byte[] data = (new UTF8Encoding()).GetBytes(source);
            byte[] hash = new SHA256CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Hash text with SHA512 method (UTF8)
        /// </summary>
        /// <param name="source">String to be hashed</param>
        /// <returns>回傳 小寫, 無 "-"</returns>
        public static string GetSHA512HashString(string source)
        {
            byte[] data = (new UTF8Encoding()).GetBytes(source);
            byte[] hash = new SHA512CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private static string _MobileSalt = null;

        /// <summary>
        /// 這個值在專案啟動後就不可任意更改，不開放讀取。
        /// </summary>
        public static string MobileSalt { set { _MobileSalt = value; } } 

        /// <summary>
        /// 手機號碼專用的 SHA512Hash
        /// </summary>
        /// <param name="mobile">String to be hashed</param>
        /// <param name="salt"></param>
        /// <returns>回傳 小寫, 無 "-"</returns>
        public static string GetMobileSHA512HashString(string mobile, string salt = null)
        {
            if(salt== null && _MobileSalt == null)
            {
                throw new Exception("未指定 mobile salt");
            }

            return GetSHA512HashString(mobile + (salt == null ? _MobileSalt : salt));
        }

        // 預設的密碼, 不開放外部存取
        static string? _CookieKey = null;
        static string? _CookieIv = null;  //Needs to be 16 ASCII characters long

        public static void SetCookieEncryptKeyAndIv(string cookieKey, string cookieIv)
        {
            if (!string.IsNullOrEmpty(_CookieKey))
            {
                throw new Exception("_CookieKey 不可重覆設定");
            }

            if (!string.IsNullOrEmpty(_CookieIv))
            {
                throw new Exception("_CookieIv 不可重覆設定");
            }

            string pattern = @"^[A-Za-z0-9!@#$%^&*()_+\-=\{}\[\]|\\:;""'<>,.?/]{1,}$";
            if (!Regex.IsMatch(cookieIv, pattern) || cookieIv.Length != 16)
            {
                throw new Exception(@"_CookieIv 應為 16 位的英數字或常用符號 !@#$%^&*()_+-={}[]|\:;""'<>,.?/");
            }

            if (!Regex.IsMatch(cookieKey, pattern) || cookieKey.Length  < 16)
            {
                throw new Exception(@"_CookieIv 應為 16 位以上的英數字或常用符號 !@#$%^&*()_+-={}[]|\:;""'<>,.?/");
            }

            _CookieKey = cookieKey;
            _CookieIv = cookieIv;
        }

        /// <summary>
        /// 給 Cookie 專用的
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static string AesEncryptCookie(string cookie)
        {
            if (string.IsNullOrEmpty(_CookieKey) || string.IsNullOrEmpty(_CookieIv))
            {
                throw new Exception("請呼叫 SetCookieEncryptKeyAndIv 初始化 _CookieKey 和 _CookieIv");
            }
            return AesEncryptor.Encrypt(_CookieKey, _CookieIv, cookie, false);
        }

        /// <summary>
        /// 給 Cookie 專用的
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static string AesDecryptCookie(string cookie)
        {
            if (string.IsNullOrEmpty(_CookieKey) || string.IsNullOrEmpty(_CookieIv))
            {
                throw new Exception("請呼叫 SetCookieEncryptKeyAndIv 初始化 _CookieKey 和 _CookieIv");
            }
            return AesEncryptor.Decrypt(_CookieKey, _CookieIv, cookie, false);
        }

        /// <summary>
        /// 解密設定檔，注意: 本 function 不可放在 try cache 之中。
        /// </summary>
        /// <param name="decFilename">解密後的檔案位置</param>
        /// <param name="encFilename">加密檔的檔案位置</param>>
        /// <param name="testString">測試明碼字串，若 encFilename 中含有該字串，則視為明文(應該是剛做過修正)，會重新加密後回存。</param>
        /// <param name="password">加解密用密碼</param>
        /// <param name="iv">IV</param>
        /// <param name="showEncSetting">編輯設定檔時使用，若為 true，則產生解密檔，並停止執行。</param>
        /// <param name="keys">會使用這些 Keys 來建立空白 Setting 檔。</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, string> GetDecryptedSetting(string filename, 
            string testString, string password, string iv, string showEncSetting, string keys = null )
        {
            string encFilename = filename;
            if(!encFilename.EndsWith(".json"))
            {
                throw new Exception($"filename 檔名必需為 .json 檔。");
            }
            string decFilename = filename.Replace(".json", "_dec.json");

            #region Decrypt
            if (System.IO.File.Exists(decFilename))
            {
                if (! System.IO.File.Exists(encFilename))
                {
                    throw new Exception($"請參考 {decFilename}, 建立 {encFilename}。");
                }

                throw new Exception($"請刪除 {decFilename}，或是清除 appsetting.json 的 ShowEncSetting 的設定，以避免機敏資料外洩。");
            }

            if (! System.IO.File.Exists(encFilename))
            {
                if(string.IsNullOrEmpty(keys))
                {
                    keys = "Key1,Key2,Key3";
                }

                var dic = keys.Split(",").Select(x => x.Trim()).ToDictionary(x => x, x => "");

                System.IO.File.WriteAllText(decFilename, dic.Json(isIndented: true));

                throw new Exception($"請參考 {decFilename}, 建立 {encFilename}。");
            }

            var encSettingJson = System.IO.File.ReadAllText(encFilename);
            if (encSettingJson.Contains(testString))
            {
                //若是明碼，則加密後回存
                encSettingJson = Su.Encryption.AesEncryptor.Encrypt(password, iv, encSettingJson);
                System.IO.File.WriteAllText(encFilename, encSettingJson);
            }

            var settingJson = Su.Encryption.AesEncryptor.Decrypt(password, iv, encSettingJson);

            //若是在 Appsetting.Config 中有密碼，儲存解密後的內容，以便修改。
            if (showEncSetting != null && showEncSetting.ToLower() == "true")
            {
                System.IO.File.WriteAllText(decFilename, settingJson);
                throw new Exception($"請刪除 {decFilename}，並修改 appsettings.json 中的 ShowEncSetting 的設定， 以避免密碼外洩。");
            }
            #endregion

            return settingJson.JsonDeserialize<Dictionary<string, string>>();
        }

        public class AesEncryptor
        {
            /// <summary>
            /// 驗證key和iv的長度(AES只有三種長度適用)
            /// </summary>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            private static void Validate_KeyIV_Length(string key, string iv)
            {
                //驗證key和iv都必須為128bits或192bits或256bits
                List<int> LegalSizes = new List<int>() { 128, 192, 256 };
                int keyBitSize = Encoding.UTF8.GetBytes(key).Length * 8;
                int ivBitSize = Encoding.UTF8.GetBytes(iv).Length * 8;
                if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
                {
                    throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
                }
            }

            public const string SaltSeperator = "##345678123456$$";

            /// <summary>
            /// 加密後回傳base64String，相同明碼文字編碼後的base64String結果會相同(類似雜湊)，除非變更key或iv
            /// 如果key和iv忘記遺失的話，資料就解密不回來
            /// base64String若使用在Url的話，Web端記得做UrlEncode
            /// 結果同 https://www.devglan.com/online-tools/aes-encryption-decryption
            /// 本加密會自動在 plainText 前方加上 32 個字元的亂數
            /// </summary>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <param name="plainText"></param>
            /// <returns></returns>
            public static string Encrypt(string key, string iv, string plainText, bool isAddSalt = true)
            {
                Validate_KeyIV_Length(key, iv);
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;//非必須，但加了較安全
                aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全
                aes.KeySize = 256;

                ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));

                //isAddPadding 時, 前 16 碼必需為亂數，以防暴力破解。
                byte[] bPlainText = Encoding.UTF8.GetBytes((isAddSalt ? (Su.TextFns.GetRandomString(16) + SaltSeperator) : "") + plainText);//明碼文字轉byte[]
                byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length);//加密
                return Convert.ToBase64String(outputData);
            }
            /// <summary>
            /// 解密後，回傳明碼文字
            /// 若解密的結果的第 16~31 (開頭為第 0 個字元)個字元為 PaddingSeperator，回傳前會自動去除前 32 個字元
            /// </summary>
            /// <param name="key"></param>
            /// <param name="iv"></param>
            /// <param name="base64String"></param>
            /// <returns></returns>
            public static string Decrypt(string key, string iv, string base64String, bool isSalted = true)
            {
                Validate_KeyIV_Length(key, iv);
                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;//非必須，但加了較安全
                aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全
                aes.KeySize = 256;

                ICryptoTransform transform = aes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
                byte[] bEnBase64String = null;
                byte[] outputData = null;
                bEnBase64String = Convert.FromBase64String(base64String);//有可能base64String格式錯誤
                outputData = transform.TransformFinalBlock(bEnBase64String, 0, bEnBase64String.Length);//有可能解密出錯

                var res = Encoding.UTF8.GetString(outputData);

                if (isSalted)
                {
                    if(res.Length <= 32)
                    {
                        throw new Exception("密文長度不足");
                    }

                    if(res.Substring(16, 16) != SaltSeperator)
                    {
                        throw new Exception("密文驗証錯誤");
                    }

                    return res.Substring(32); // 略過 0~31
                }

                return res;
            }
        }

        /// <summary>
        /// 取得儲存在 System Environment 的 Secret
        /// 若是不存在，則會在 logDir 產生 variableName.ps1，可供執行。
        /// 系統會檢查 variableName.ps1 是否已刪除。以避免 secret 外洩。
        /// </summary>
        /// <param name="envSecret">最少 32 位元長，只會取用前 32 位元</param>
        /// <param name="envIv">最少 16 位元長，只會取用前 16 位元</param>
        /// <param name="ps1FileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static (string secret, string iv) GetSystemEnvironmentSecret(string envSecret, string envIv, string variableName, string logDir)
        {
            if (envSecret.Length < 32)
            {
                throw new Exception("envSecret 最少長度 32 位元");
            }
            envSecret = envSecret[..32];

            if (envIv.Length < 16)
            {
                throw new Exception("envSecret 最少長度 16 位元");
            }
            envIv = envIv[..16];

            //毎個專案的 Secret 各別存在 Environment Variable之中，若是有權限可以讀取 Environment Variable 則本系統已經不安全(可以確保，可以取得檔案，但無法在系統執行程式，仍無法解密)
            string encSecretAndIv = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine) ?? "";
            string ps1FileName = logDir.AddPath($"{variableName}.ps1");
            if (encSecretAndIv == "")
            {
                encSecretAndIv = Su.Encryption.AesEncryptor.Encrypt(envSecret, envIv, Su.TextFns.GetRandomString(48));
                var command = $"[Environment]::SetEnvironmentVariable('{variableName}', '{encSecretAndIv}', 'Machine')";
                System.IO.File.WriteAllText(ps1FileName.SafeFilename(), command);

                throw new Exception($"請設定環境變數 {variableName}，建議內容可參考: {ps1FileName}，執行結束後請刪除本檔案。");
            }
            else
            {
                if (System.IO.File.Exists(ps1FileName))
                {
                    throw new Exception($"請刪除 {ps1FileName}。");
                }
            }

            var secretAndIv = Su.Encryption.AesEncryptor.Decrypt(envSecret, envIv, encSecretAndIv);
            string secret = secretAndIv[..32]; // 32 位元長
            string iv = secretAndIv.Substring(32, 16);// 16 字元長

            return (secret, iv);
        }
    }
}
