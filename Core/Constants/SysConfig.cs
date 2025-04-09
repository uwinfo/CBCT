namespace Core.Constants
{
    public static class SysConfig
    {
        /// <summary>
        /// 取得全部參數的字典
        /// </summary>
        /// <returns></returns>
        static Dictionary<string, string> GetDictionary()
        {
            string key = "SysConfig_Dictionary";
            if (!Su.PgSqlCache.HasCache(key))
            {
                var dic = Core.Helpers.SysConfigHelper.GetDictionaryFromDb();
                Su.PgSqlCache.AddCache(Su.PgSql.DefaultDbId, key, dic, "sys_config");
            }

            return Su.PgSqlCache.GetCache<Dictionary<string, string>>(key);
        }

        /// <summary>
        /// 這裡暫時不考慮錯誤的問題
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static string GetConfigFromCache(string name)
        {
            var dic = GetDictionary();
            return dic[name];
        }
    }
}