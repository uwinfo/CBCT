//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Runtime.Caching;
//using System.Text;
//using System.Threading.Tasks;

//namespace Su
//{
//    public class MsSqlCache
//    {
//        public static T GetValue<T>(string sql, string relateTables, object parameters = null, Sql.DbId dbId = null)
//        {
//            if(dbId == null)
//            {
//                dbId = MsSql.DefaultDbId;
//            }

//            string cacheKey = "MsSqlCache_" + sql.ToMsSql(parameters);

//            ObjectCache cache = MemoryCache.Default;

//            T OldValue = (T)cache[cacheKey];
//            if (OldValue != null)
//            {
//                return OldValue;
//            }

//            lock (Su.LockerProvider.GetLocker(cacheKey))
//            {
//                OldValue = (T)cache[cacheKey]; //先取值應該比用 contains 安全
//                if (OldValue != null)
//                {
//                    return OldValue;
//                }

//                var dt = Su.MsSql.DtFromSql(sql.ToMsSql(parameters), dbId);
//                if (dt.Rows.Count > 0)
//                {
//                    var res = (T)dt.Rows[0][0];
//                    AddCache(dbId, cacheKey, res, relateTables);
//                    return res;
//                }

//                return default;
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="relateTables"></param>
//        /// <returns></returns>
//        public static DataTable GetDataTable(string sql, string relateTables, Sql.DbId dbId )
//        {
//            if(dbId == null)
//            {
//                dbId = MsSql.DefaultDbId;
//            }

//            string cacheKey = $"Cache_GetDataTable_{dbId}_{sql}";

//            ObjectCache cache = MemoryCache.Default;

//            var OldValue = (DataTable)cache[cacheKey];
//            if (OldValue != null)
//            {
//                return OldValue;
//            }

//            lock (Su.LockerProvider.GetLocker(cacheKey))
//            {
//                OldValue = (DataTable)cache[cacheKey];
//                if (OldValue != null)
//                {
//                    return OldValue;
//                }
//                var dt = Su.MsSql.DtFromSql(sql, dbId);
//                AddCache(dbId, cacheKey, dt, relateTables);
//                return dt;
//            }
//        }

//        public static string TableCacheKey(Sql.DbId dbId, string tableName)
//        {
//            return $"MsSqlCache.Table.{dbId}_{tableName.ToUpper()}";
//        }

//        public static bool HasCache(string cacheKey)
//        {
//            return MemoryCache.Default.Contains(cacheKey);
//        }

//        public static T GetCache<T>(string cacheKey)
//        {
//            ObjectCache cache = MemoryCache.Default;

//            return (T)cache[cacheKey];
//        }

//        public static void AddCache(Sql.DbId dbId, string cacheKey, object obj, string tableNames, DateTime? expireAt = null)
//        {
//            if (dbId == null)
//            {
//                dbId = MsSql.DefaultDbId;
//            }

//            List<string> ltTables = tableNames.Split(',').Select(t => TableCacheKey(dbId, t)).ToList();

//            var policy = new CacheItemPolicy();
//            policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(ltTables));
//            if (expireAt != null)
//            {
//                policy.AbsoluteExpiration = (DateTime)expireAt;
//            }
            
//            MemoryCache.Default.Set(cacheKey, obj, policy);
//        }

//        static DateTime LastUpdateDate = DateTime.MinValue;
//        public static void StartUpdateTableCache()
//        {
//            //檢查是否還有另一個 Thread 在執行. 超過 10 秒末執行, 就動啟一個 Thread.
//            if (LastUpdateDate > DateTime.Now.AddSeconds(-10))
//            {
//                Su.Debug.WriteLine("LastUpdateDate > DateTime.Now.AddSeconds(-10), skip");
//                return;
//            }

//            Su.Debug.WriteLine("start a new thread.");
//            var T = new System.Threading.Thread(UpdateTableCache);
//            T.Start();
//        }

//        public static List<Sql.DbId> CachedDbs = new List<Sql.DbId>();

//        static int CurrentThreadId = -1;

//        /// <summary>
//        /// 
//        /// </summary>
//        public static void UpdateTableCache()
//        {
//            //標記自已是正在執行的 Thread. 讓舊的 Thread 在執行完畢之後, 應該會自動結束. 以防舊的 Thread 執行超過 10 秒.
//            CurrentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

//            try
//            {
//                //確認自已是正在執行的 Thread, 重覆執行. (另一個 Thread 插入執行)
//                while (CurrentThreadId == System.Threading.Thread.CurrentThread.ManagedThreadId)
//                {
//                    LastUpdateDate = DateTime.Now;

//                    foreach(var dbId in CachedDbs)
//                    {
//                        Su.Debug.WriteLine($"UpdateTableCache for {dbId}");
//                        var sql = @"select * from AspNet_SqlCacheTablesForChangeNotification";

//                        var dt = Su.MsSql.DtFromSql(sql, dbId);

//                        foreach (DataRow row in dt.Rows)
//                        {
//                            string changeId = row["changeId"].ToString();
//                            string CacheKey = TableCacheKey(dbId, row["tableName"].ToString());
//                            ObjectCache cache = MemoryCache.Default;
//                            string OldValue = cache[CacheKey] as string;

//                            if (OldValue == null)
//                            {
//                                cache.Set(CacheKey, changeId, DateTime.MaxValue);
//                            }
//                            else
//                            {
//                                if (changeId != OldValue)
//                                {
//                                    cache.Remove(CacheKey);
//                                    cache.Set(CacheKey, changeId, DateTime.MaxValue);
//                                }
//                            }
//                        }
//                    }

//                    //每兩秒檢查一次
//                    System.Threading.Thread.Sleep(2000);
//                }
//            }
//            catch (Exception)
//            {
//                //依經驗, 只要 DB 能通, 這裡幾乎不會有問題, 所以這裡暫時不處理, 未來有問題時可以考慮寫入文字檔比較好.
//            }
//        }
//    }
//}
