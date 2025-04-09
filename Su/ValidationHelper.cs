using Su;
using System.Reflection;

namespace Su
{
    public static class ValidationHelper
    {
        /// <summary>
        /// name 優先，若 name 為 null 或空白，試著找 ValidationName Attribute，若存在，則回傳它的 name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static string? GetDisplayName(string? name, PropertyInfo propertyInfo)
        {
            if(string.IsNullOrEmpty(name))
            {
                var nameAttribute = propertyInfo.GetCustomAttribute<ValidationAttributes.ValidationName>();
                if (nameAttribute != null)
                {
                    return nameAttribute.Name;
                }
            }

            return name;
        }

        public static bool CheckDays(string? days)
        {
            if (string.IsNullOrEmpty(days))
            {
                return false;
            }

            var dayList = days.Split(',');
            foreach (var day in dayList)
            {
                if (!day.IsDate(IsDateOnly: true))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 不允許空白或 null
        /// 沒填寫或每一個都要符合規則
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        public static bool CheckMutipleTimePeriod(string? periods)
        {
            if (string.IsNullOrEmpty(periods))
            {
                return false;
            }

            //任一個不符規定就回傳 false
            return !periods.Split(",").Any(x => !IsTimePeriod(x));
        }

        /// <summary>
        /// 不允許空白
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        public static bool CheckMutipleDayPeriod(string? periods)
        {
            if (string.IsNullOrEmpty(periods))
            {
                return false;
            }

            //任一個不符規定就回傳 false
            return !periods.Split(",").Any(x => !IsDayPeriod(x));
        }

        /// <summary>
        /// 不允許空白
        /// 檢查是否為日期區間，格式為 yyyy-mm-dd~yyyy-mm-dd
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        public static bool IsDayPeriod(string? periods)
        {
            if (string.IsNullOrEmpty(periods))
            {
                return false;
            }

            periods = periods.Replace(" ", "");
            var periodArray = periods.Split('~');
            if (periodArray.Length == 1 && periodArray[0].IsDate(true))//單日
            {
                return true;
            }

            //以下為多日
            if (periodArray.Length != 2)
            {
                return false;
            }

            if (!periodArray[0].IsDate(true) || !periodArray[1].IsDate(true))
            {
                return false;
            }

            if (periodArray[0].ToDate() > periodArray[1].ToDate())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 不允許空白或 null
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        public static bool IsTimePeriod(string? periods)
        {
            if(string.IsNullOrEmpty(periods))
            {
                return false;
            }

            periods = periods.Replace(" ", "");

            var periodArray = periods.Split('~');
            if(periodArray.Length != 2 )
            {
                return false;
            }

            if (! periodArray[0].IsValidHhmm() || !periodArray[1].IsValidHhmm())
            {
                return false;
            }

            if (periodArray[0].ToTimeOnly() >= periodArray[1].ToTimeOnly()) {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 檢查是否符合 hh:mm 的格式
        /// </summary>
        /// <param name="hhmm"></param>
        /// <returns></returns>
        public static bool IsValidHhmm(this string hhmm)
        {
            return Su.TextFns.IsValidHhmm(hhmm);
        }

        /// <summary>
        /// 檢查是否符合 hh:mm 的格式
        /// </summary>
        /// <param name="hhmm"></param>
        /// <returns></returns>
        public static int HhmmToMinutes(this string hhmm)
        {
            System.TimeOnly time = System.TimeOnly.Parse(hhmm);
            return time.Hour * 60 + time.Minute;
        }
    }
}
