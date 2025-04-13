using System.Data;
using System.Runtime.Caching;

namespace Su
{
    public class PgSqlCache
    {
        public string TableCacheKey(Sql.DbId dbId, string tableName)
        {
            return $"PgSqlCache.Table.{dbId}_{tableName.ToUpper()}";
        }

        public bool HasCache(string cacheKey)
        {
            return MemoryCache.Default.Contains(cacheKey);
        }

        public T GetCache<T>(string cacheKey)
        {
            ObjectCache cache = MemoryCache.Default;

            return (T)cache[cacheKey];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbId"></param>
        /// <param name="cacheKey"></param>
        /// <param name="obj"></param>
        /// <param name="tableNames">要監控的資料表名稱</param>
        /// <param name="expireAt"></param>
        public void AddCache(Sql.DbId dbId, string cacheKey, object obj, string tableNames, DateTime? expireAt = null)
        {
            if (dbId == null)
            {
                dbId = Su.PgSql.DefaultDbId;
            }

            if (tableNames.Split(',').Any(t => !IsTableMonitored(t, dbId)))
            {
                var notMonitoredTables = tableNames.Split(',').Where(t => !IsTableMonitored(t, dbId)).ToList();

                throw new Exception($"以下的資料表沒有被加入 cache 監控: {notMonitoredTables.ToOneString(",")}，請參考以下文章，把資料表加入監控: https://blog.uwinfo.com.tw/auth/article/bike/507");
            }

            List<string> tableCacheKeys = tableNames.Split(',').Select(t => TableCacheKey(dbId, t)).ToList();

            var policy = new CacheItemPolicy();
            policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(tableCacheKeys));
            if (expireAt != null)
            {
                policy.AbsoluteExpiration = (DateTime)expireAt;
            }

            MemoryCache.Default.Set(cacheKey, obj, policy);
        }

        /// <summary>
        /// 啟動 thread 時要檢查，在 UpdateTableCacheThread 中會更新這個變數。
        /// </summary>
        DateTime LastUpdateDate = DateTime.MinValue;
        public void StartUpdateTableCache()
        {
            lock (Su.LockerProvider.GetLocker("StartUpdateTableCache"))
            {
                //檢查是否還有另一個 Thread 在執行. 超過 10 秒末執行, 就動啟一個 Thread.
                if (LastUpdateDate > DateTime.Now.AddSeconds(-10))
                {
                    Su.Debug.WriteLine("LastUpdateDate > DateTime.Now.AddSeconds(-10), skip");
                    return;
                }

                LastUpdateDate = DateTime.Now;

                Su.Debug.WriteLine("start a new thread.");
                var T = new System.Threading.Thread(UpdateTableCacheThread);
                T.Start();
            }
        }

        ///// <summary>
        ///// 在 Program.cs 中要把需監控的 DB 加入
        ///// </summary>
        ///// <param name="dbId"></param>
        //public static void AddMonitoredDb(Sql.DbId dbId)
        //{
        //    MonitoredDbs.Add(dbId);
        //}

        List<Sql.DbId> MonitoredDbs = new List<Sql.DbId>();

        /// <summary>
        /// 在迴圈中
        /// </summary>
        int CurrentThreadId = -1;

        /// <summary>
        /// 在資料庫中被監控的 Table，只新增，不刪除。
        /// </summary>
        Dictionary<Sql.DbId, List<string>> MonitoredTablesInDbs = new Dictionary<Sql.DbId, List<string>>();

        /// <summary>
        /// 檢查這個 Table 是否被監控
        /// </summary>
        /// <param name="dbId"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>     
        public bool IsTableMonitored(string tableName, Sql.DbId? dbId = null)
        {
            dbId ??= Su.PgSql.DefaultDbId;

            List<string> tables;
            if (MonitoredTablesInDbs.TryGetValue(dbId, out List<string>? value))
            {
                tables = value;
            }
            else
            {
                tables = [];
                MonitoredTablesInDbs.Add(dbId, tables);
            }

            return tables.Contains(tableName);
        }

        public bool IsFirstRun = false;

        /// <summary>
        /// 
        /// </summary>
        public void UpdateTableCacheThread()
        {
            //標記自已是正在執行的 Thread. 讓舊的 Thread 在執行完畢之後, 應該會自動結束. 以防舊的 Thread 執行超過 10 秒.
            CurrentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            try
            {
                //確認自已是正在執行的 Thread, 重覆執行. (另一個 Thread 插入執行)
                while (CurrentThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
                {
                    LastUpdateDate = DateTime.Now;

                    foreach (var dbId in MonitoredDbs)
                    {
                        List<string> tables;
                        if (MonitoredTablesInDbs.ContainsKey(dbId))
                        {
                            tables = MonitoredTablesInDbs[dbId];
                        }
                        else
                        {
                            tables = new List<string>();
                            MonitoredTablesInDbs.Add(dbId, tables);
                        }

                        var sql = @"select * from table_monitor";

                        var dt = PgSql.DtFromSql(sql, dbId);

                        foreach (DataRow row in dt.Rows)
                        {
                            string changeId = row["update_count"].DBNullToDefault();
                            string tableName = row["table_name"].DBNullToDefault();
                            if (!tables.Contains(tableName))
                            {
                                tables.Add(tableName); //記錄這個 Table 已經被監控
                            }

                            string CacheKey = TableCacheKey(dbId, tableName);
                            ObjectCache cache = MemoryCache.Default;

                            string OldValue = (string)cache[CacheKey];

                            if (OldValue == null)
                            {
                                cache.Set(CacheKey, changeId, DateTime.MaxValue);
                            }
                            else
                            {
                                if (changeId != OldValue)
                                {
                                    cache.Remove(CacheKey);
                                    cache.Set(CacheKey, changeId, DateTime.MaxValue);
                                }
                            }
                        }
                    }

                    IsFirstRun = true;

                    //每兩秒檢查一次
                    System.Threading.Thread.Sleep(2000);
                }
            }
            catch (Exception)
            {
                //依經驗, 只要 DB 能通, 這裡幾乎不會有問題, 所以這裡暫時不處理, 未來有問題時可以考慮寫入文字檔比較好.
            }
        }
    }
}