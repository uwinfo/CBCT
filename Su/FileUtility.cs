using NPOI.SS.Formula.Functions;
using System.Runtime.Caching;

namespace Su
{
    public class FileUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string TimeStampFilename(string ext)
        {
            return $"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{CryptographicHelper.GetSpecificLengthRandomString(5, true)}{ext}";
        }
        
        static HashSet<string> CreatedFolders = new HashSet<string>();
        /// <summary>
        /// 整個執行期間只會建立一次. 避免重覆檢查磁碟
        /// 如果有子目錄, 會把子目錄合併成完整目錄, 並回傳.(中間會處理目錄分隔符號的問題)
        /// </summary>
        /// <param name="Folder"></param>
        /// <param name="subFolder">記得不要加 / 或 \ 結尾</param>
        public static string CreateDirectory(string Folder, string subFolder = null)
        {
            string sep = "/";
            if (! Folder.StartsWith(sep))
            {
                sep = @"\";
                subFolder = string.IsNullOrEmpty(subFolder) ? subFolder : subFolder.Replace("/", @"\");
            }
            else
            {
                subFolder = string.IsNullOrEmpty(subFolder) ? subFolder :  subFolder.Replace(@"\", "/");
            }

            if (!Folder.EndsWith(sep))
            {
                Folder += sep;
            }

            if (!string.IsNullOrEmpty(subFolder))
            {
                Folder += subFolder + sep;
            }

            if (!CreatedFolders.Contains(Folder))
            {
                lock (CreatedFolders)
                {
                    if (!CreatedFolders.Contains(Folder))
                    {
                        System.IO.Directory.CreateDirectory(Folder);
                        CreatedFolders.Add(Folder);
                    }
                }
            }

            return Folder;
        }

        static Dictionary<string, DateTime> _RemoveOldFile = new Dictionary<string, DateTime>();
        /// <summary>
        /// 刪除舊檔案, 通常用來清除過期的 Log 用.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="MaxDate">小於或等於這個日期的檔都都會被刪除</param>
        /// <param name="IsThrowException">預設無法刪除檔案時, 不要發生 Exception</param>
        /// <param name="IsOncePerDay">是否每天只跑一次(用一個 static dictionary 管理)</param>
        /// <param name="isInThread"></param>
        public static void RemoveOldFile(string folder, DateTime MaxDate, bool IsThrowException = false,
            bool IsOncePerDay = false, bool isInThread = false)
        {
            try
            {
                if (IsOncePerDay)
                {
                    if (!_RemoveOldFile.ContainsKey(folder))
                    {
                        _RemoveOldFile.Add(folder, DateTime.Now);
                    }

                    var LastUpdate = _RemoveOldFile[folder];
                    if (LastUpdate > DateTime.Now.AddDays(-1))
                    {
                        return;
                    }
                }

                var p = new RemoveFileParameter()
                {
                    Directory = folder,
                    MaxDate = MaxDate
                };

                if (isInThread)
                {
                    var T = new System.Threading.Thread(RemoveOldFileForThread);
                    T.Start(p);
                }
                else
                {
                    RemoveOldFileForThread(p);
                }

            }
            catch (Exception)
            {
                if (IsThrowException)
                {
                    throw;
                }
            }
        }

        public class RemoveFileParameter
        {
            public string Directory { get; set; }
            public DateTime MaxDate { get; set; }
        }

        /// <summary>
        /// 可以在 Thread 中執行的 Remove Old File
        /// </summary>
        /// <param name="p"></param>
        public static void RemoveOldFileForThread(object p)
        {

            RemoveFileParameter parameter = (RemoveFileParameter)p;

            try
            {
                var query = from o in Directory.GetFiles(parameter.Directory, "*.*")
                            let x = new FileInfo(o)
                            where x.CreationTime <= parameter.MaxDate
                            select o;

                foreach (var item in query)
                {
                    try //在這裡做 Try Cache, 可以避免單一檔案無法刪除, 而阻擋了所有檔案的刪除.
                    {
                        System.IO.File.Delete(item);
                    }
                    catch (Exception)
                    {
                        //在 Thread 中執行, 發生 Exception 就不丟出了. 以免 IIS 掛掉.
                    }
                }
            }
            catch (Exception)
            {
                //在 Thread 中執行, 發生 Exception 就不丟出了. 以免 IIS 掛掉.
            }
        }
        
        public static string GetFileWithCache(string Filename, string language = "")
        {
            string CacheKey = "FileUtility.GetFileWithCache_" + Filename + "@Language_" + language;
            string fileContents = MemoryCache.Default[CacheKey] as string;

            try
            {
                if (fileContents == null)
                {
                    lock (LockerProvider.GetLocker("FileUtility.GetFileWithCache"))
                    {
                        fileContents = MemoryCache.Default[CacheKey] as string;
                        if (fileContents == null)
                        {
                            CacheItemPolicy policy = new CacheItemPolicy();
                            var filePaths = new List<string>();
                            filePaths.Add(Filename);
                            policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));


                            // Fetch the file contents.
                            fileContents = File.ReadAllText(Filename);

                            if (!string.IsNullOrEmpty(language))
                            {
                                var dic = Translator.GetDictionary(language);
                                var keys = dic.Keys.OrderByDescending(x => x.Length);
                                foreach (string key in keys)
                                {
                                    fileContents = fileContents.Replace(key, dic[key]);
                                }

                                policy.ChangeMonitors.Add(MemoryCache.Default.CreateCacheEntryChangeMonitor(Translator.CacheKey.Split('|')));
                            }

                            MemoryCache.Default.Set(CacheKey, fileContents, policy);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            
            return fileContents;
        }
    }
}
