using System.Security.Cryptography;
using System.Text;
using SHA3.Net;

namespace Su
{
    public class CryptographicHelper
    {
        public static string GetSpecificLengthRandomString(int size, bool isLowerCase = false)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for(int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (isLowerCase)
            {
                return builder.ToString().ToLower();
            }
            return builder.ToString();
        }

        public static string GetRandomNumber(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for(int i = 0; i < size; i++)
            {
                builder.Append(random.Next(10).ToString());
            }

            return builder.ToString();
        }

        public static string GetSalt(int maxSize)
        {
            return Su.TextFns.GetRandomString(maxSize);
        }

        /// <summary>
        /// 用 secret 和 salt 產生 hash
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GetSecretHash(string salt, string secret)
        {
            return GetSHA256Hash(salt + secret);
        }

        //public static string GetSHA256Hash(string secret,string salt)
        //{
        //    return GetSHA256Hash(salt + secret);
        //}

        public static string GetSHA256Hash(string source)
        {
            using(SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public static string GetSHA3_512Hash(string source)
        {
            using(var shaAlg = Sha3.Sha3512())
            {
                byte[] bytes = shaAlg.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
