using System;

namespace Su
{
    public class MathUtil
    {
        /// <summary>
        /// 回傳大於等於 0 小於 1 (0~0.999999...)
        /// </summary>
        /// <returns></returns>
        public static double GetRandomDouble()
        {
            return Convert.ToDouble(System.Security.Cryptography.RandomNumberGenerator.GetInt32(Int32.MaxValue)) / Int32.MaxValue;
        }

        /// <summary>
        /// 使用 System.Security.Cryptography.RandomNumberGenerator
        /// 回傳值介於 min ~ (max - 1)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomInt(int min, int max)
        {
            if (max < min)
            {
                throw new Exception("最大值不可小於最小值");
            }

            if (min < 0)
            {
                throw new Exception("最小值不可小於 0");
            }

            return min + System.Security.Cryptography.RandomNumberGenerator.GetInt32(max - min);
        }

        /// <summary>
        /// 產生 0 ~ (max - 1) 的亂數
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomInt(int max)
        {
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(max);
        }

        /// <summary>
        /// 產生 0 ~ (int.MaxValue - 1) 的亂數
        /// </summary>
        /// <returns></returns>
        public static int GetRandomInt()
        {
            return GetRandomInt(int.MaxValue);
        }

        public static int 整數無條件進入(float F)
        {
            return Convert.ToInt32(Math.Ceiling(F));
        }

        public static int 整數無條件拾去(float F)
        {
            return Convert.ToInt32(Math.Floor(F));
        }

        /// <summary>
        /// 回傳小於 (被除數 / 除數) 的最大整數
        /// </summary>
        /// <param name="被除數"></param>
        /// <param name="除數"></param>
        /// <returns></returns>
        public static int 整數無條件拾去(int 被除數, int 除數)
        {
            if(被除數 <= 0 || 除數 <= 0)
            {
                throw new Exception("被除數 和 除數不應小於 0");
            }

            return 整數無條件拾去(Convert.ToSingle(被除數) / 除數);
        }

        public static decimal 整數四捨五入(decimal d, int digits = 0)
        {
            return Math.Round(d, digits, MidpointRounding.AwayFromZero); // 加上 MidpointRounding.AwayFromZero才會四捨五入, Reiko, 2011/05/20
        }

        //static DateTime LastRandomGenerateTime = DateTime.MinValue;
        //static Random _random = new Random();
        ///// <summary>
        ///// 取得共用的 Random 物件, 避免回傳相同的值.
        ///// </summary>
        ///// <returns></returns>
        //public static Random Random
        //{
        //    get
        //    {
        //        //曾經發生 Random 物件掛掉, 所以每分鐘重新產生一個.
        //        if(LastRandomGenerateTime < DateTime.Now.AddMinutes(-1))
        //        {
        //            _random = new Random();
        //            LastRandomGenerateTime = DateTime.Now;
        //        }

        //        return _random;
        //    }            
        //}
    }
}