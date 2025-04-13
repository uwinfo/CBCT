using Core.Helpers;
using Su;

namespace Core.Constants
{
    public class SysConfig
    {
        private readonly PgSqlCache _pgSqlCache;
        public SysConfig(PgSqlCache pgSqlCache)
        {
            _pgSqlCache = pgSqlCache;
        }

        /// <summary>
        /// 取得全部參數的字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetDictionary()
        {
            string key = "SysConfig_Dictionary";
            if (!_pgSqlCache.HasCache(key))
            {
                var dic = SysConfigHelper.GetDictionaryFromDb();
                _pgSqlCache.AddCache(PgSql.DefaultDbId, key, dic, "sys_config");
            }

            return _pgSqlCache.GetCache<Dictionary<string, string>>(key);
        }

        /// <summary>
        /// 這裡暫時不考慮錯誤的問題
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetConfigFromCache(string name)
        {
            var dic = GetDictionary();
            return dic[name];
        }
    }
}