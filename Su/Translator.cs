using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;
using System.Data;

namespace Su
{
    public class Translator
    {
        /// <summary>
        /// 用逗號隔開, 例如 "key,en,jp", 第一個是 Key
        /// </summary>
        public static string Languages;

        public static string DictionaryPath;

        public const string CacheKey = "Translator_Dictionary";

        public static Dictionary<string, Dictionary<string, string>> Dictionaries
        {
            get
            {
                if (!(MemoryCache.Default[CacheKey] is Dictionary<string, Dictionary<string, string>> dics))
                {
                    lock (LockerProvider.GetLocker(CacheKey))
                    {
                        dics = MemoryCache.Default[CacheKey] as Dictionary<string, Dictionary<string, string>>;

                        if (dics == null)
                        {
                            var langs = Languages.Split(",");
                            var dt = Su.ExcelNPOI.ExcelToDT(DictionaryPath);
                            dics = new Dictionary<string, Dictionary<string, string>>();
                            foreach (string l in langs)
                            {
                                dics[l] = new Dictionary<string, string>();
                            }

                            var keyLang = langs[0];

                            foreach (DataRow oRow in dt.Rows)
                            {
                                if (!Convert.IsDBNull(oRow[keyLang]) && !dics[keyLang].ContainsKey(oRow[keyLang].ToString().Trim()))
                                {
                                    string key = oRow[keyLang].ToString().Trim();
                                    //Su.WU.DebugWriteLine("key: " + key);
                                    foreach (string lang in langs)
                                    {
                                        //Su.WU.DebugWriteLine("lang: " + lang);

                                        if ((Convert.IsDBNull(oRow[lang]) || string.IsNullOrEmpty(oRow[lang].ToString().Trim())))
                                        {
                                            //Su.WU.DebugWriteLine("null or empty");

                                            if (key.StartsWith("!@") && key.EndsWith("@!"))
                                            {
                                                dics[lang][key] = (lang + "_" + key[2..^2]);
                                            }
                                            else
                                            {
                                                dics[lang][key] = (lang + "_" + key);
                                            }
                                        }
                                        else
                                        {
                                            //Su.WU.DebugWriteLine("not null or empty: " + oRow[lang].ToString().Trim());

                                            dics[lang][key] = oRow[lang].ToString().Trim();
                                        }
                                    }
                                }
                            }

                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.ChangeMonitors.Add(new HostFileChangeMonitor(DictionaryPath.Split('|'))); // | 應該不會出現在檔名中
                            MemoryCache.Default.Set(CacheKey, dics, policy);
                        }
                    }
                }

                return dics;
            }
        }

        public static Dictionary<string, string> GetDictionary(string lang)
        {
            return Dictionaries[lang];
        }
    }
}
