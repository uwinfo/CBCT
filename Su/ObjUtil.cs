using Microsoft.EntityFrameworkCore;
using NPOI;
using Su;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    public static class ObjUtil
    {
        /// <summary>
        /// 只會抓 Public 和實體物件的 Property (BindingFlags.Public | BindingFlags.Instance) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dic"></param>
        /// <param name="skips"></param>
        public static void CopyPropertiesToDictionary<T>(T source, Dictionary<string, object?> dic, string? skips = null)
        {
            var sourceProps = source!.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead).ToList();
            
            IEnumerable<string>? skipNames = null;
            if (skips != null)
            {
                skipNames = skips.ToLower().Split(",").Select(x => x.Trim());
            }

            foreach (var srcItem in sourceProps)
            {
                if ((skipNames == null || !skipNames.Contains(srcItem.Name.ToLower())))
                {
                    if (dic.ContainsKey(srcItem.Name))
                    {
                        dic[srcItem.Name] = srcItem.GetValue(source, null);
                    }
                    else
                    {
                        dic.Add(srcItem.Name, srcItem.GetValue(source, null));
                    }
                }
            }
        }

        /// <summary>
        /// 把所有物件用 .ToString 之後，存入 dictionary 之。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dic"></param>
        /// <param name="skips"></param>
        public static Dictionary<string, string?> CopyPropertiesToStringDictionary<T>(T source, string skips = null)
        {
            var dic = new Dictionary<string, string?>();

            var sourceProps = source!.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead).ToList();

            IEnumerable<string>? skipNames = null;
            if (skips != null)
            {
                skipNames = skips.ToLower().Split(",").Select(x => x.Trim());
            }

            foreach (var srcItem in sourceProps)
            {
                if ((skipNames == null || !skipNames.Contains(srcItem.Name.ToLower())))
                {
                    if (dic.ContainsKey(srcItem.Name))
                    {
                        dic[srcItem.Name] = srcItem.GetValue(source, null)?.ToString();
                    }
                    else
                    {
                        dic.Add(srcItem.Name, srcItem.GetValue(source, null)?.ToString());
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// 只會抓 Public 和實體物件的 field (BindingFlags.Public | BindingFlags.Instance) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dic"></param>
        /// <param name="skips"></param>
        public static void CopyFieldToDictionary<T>(T source, Dictionary<string, object?> dic, string? skips = null)
        {
            var sourceFields = source!.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();

            IEnumerable<string>? skipNames = null;
            if (skips != null)
            {
                skipNames = skips.ToLower().Split(",").Select(x => x.Trim());
            }

            foreach (var srcItem in sourceFields)
            {
                if ((skipNames == null || !skipNames.Contains(srcItem.Name.ToLower())))
                {
                    if (dic.ContainsKey(srcItem.Name))
                    {
                        dic[srcItem.Name] = srcItem.GetValue(source);
                    }
                    else
                    {
                        dic.Add(srcItem.Name, srcItem.GetValue(source));
                    }
                }
            }
        }

        /// <summary>
        /// 只會抓 Public 和實體物件的 field 和 property (BindingFlags.Public | BindingFlags.Instance)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="dic"></param>
        /// <param name="skips"></param>
        public static void CopyToDictionary<T>(T source, Dictionary<string, object?> dic, string? skips = null)
        {
            ObjUtil.CopyPropertiesToDictionary(source, dic, skips);
            ObjUtil.CopyFieldToDictionary(source, dic, skips);
        }

        public static TU CopyPropertiesToEntity<T, TU>(T source, TU dest,
            DbContext dbContext,
            string? skips = "Uid, Id",
            string? onlys = null)
        {
            return CopyPropertiesTo(source, dest, skips, onlys, dbContext.Entry(dest!));
        }

        /// <summary>
        /// Property 名稱不分大小寫
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="skips"></param>
        /// <param name="onlys"></param>
        /// <param name="entry">可使用這個方法取得 Entry _dbContext.Entry(oldCompany) </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static TU CopyPropertiesTo<T, TU>(T source, TU dest, 
            string? skips = null, 
            string? onlys = null, 
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entry = null)
        {
            var sourceProps = source!.GetType().GetProperties().Where(x => x.CanRead).ToList();
            var destProps = dest!.GetType().GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            List<string> copyProperties;
            if (onlys == null)
            {
                copyProperties = destProps.Select(x => x.Name.ToLower()).ToList();
            }
            else
            {
                copyProperties = onlys.Split(',').Select(x => x.Trim().ToLower()).ToList();
            }
            
            if(skips != null)
            {
                IEnumerable<string> skipNames = skips.ToLower().Split(",").Select(x => x.Trim());
                copyProperties = copyProperties.Where(x => !skipNames.Contains(x)).ToList();
            }

            foreach (var srcItem in sourceProps)
            {
                if( srcItem.GetCustomAttributes(false)
                    .Any(x => x is Su.CustomAttributes.CopyToIgnore))
                {
                    continue;
                }

                if (destProps.Any(x => x.Name.ToLower() == srcItem.Name.ToLower()) &&
                    copyProperties.Contains(srcItem.Name.ToLower()))
                {
                    var destItem = destProps.First(x => x.Name.ToLower() == srcItem.Name.ToLower());
                    try
                    {
                        destItem.SetValue(dest, srcItem.GetValue(source, null), null);
                        if (entry != null)
                        {
                            entry.Property(destItem.Name).IsModified = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("srcItem.Name: " + srcItem.Name + ", " + ex.FullInfo());
                    }
                }
            }

            return dest;
        }

        public static string? GetDisplayName(System.Type type, string propertyName)
        {
            if(type == null)
            {
                return null;
            }

            foreach (var property in type.GetProperties())
            {
                if(property.Name.ToLower() == propertyName.ToLower())
                {
                    var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                        .Cast<DisplayNameAttribute>()
                                        .FirstOrDefault();
                    if (attribute != null)
                    {
                        return attribute.DisplayName;
                    }
                }
            }

            return null;
        }

        public static TU CopyFieldsTo<T, TU>(T source, TU dest, string? skips = null)
        {
            var sourceFields = source!.GetType().GetFields().ToList();
            var destFields = dest!.GetType().GetFields()
                    .ToList();
            IEnumerable<string>? skipNames = null;
            if (skips != null)
            {
                skipNames = skips.ToLower().Split(",").Select(x => x.Trim());
            }

            foreach (var srcItem in sourceFields)
            {
                if (destFields.Any(x => x.Name == srcItem.Name) &&
                    (skipNames == null || !skipNames.Contains(srcItem.Name.ToLower())))
                {
                    var destItem = destFields.First(x => x.Name == srcItem.Name);
                    try
                    {
                        destItem.SetValue(dest, srcItem.GetValue(source));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("srcItem.Name: " + srcItem.Name + ", " + ex.FullInfo());
                    }
                }
            }

            return dest;
        }

        /// <summary>
        /// 同時呼叫 CopyPropertiesTo 和 CopyFieldsTo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="skips"></param>
        /// <returns></returns>
        public static TU CopyTo<T, TU>(T source, TU dest, string? skips = null, object? additionalProperties = null)
        {
            if(source == null)
            {
                return dest;
            }

            ObjUtil.CopyPropertiesTo(source, dest, skips);
            ObjUtil.CopyFieldsTo(source, dest, skips);

            if(additionalProperties != null)
            {
                ObjUtil.CopyPropertiesTo(additionalProperties, dest);
                ObjUtil.CopyFieldsTo(additionalProperties, dest);
            }

            return dest;
        }

        public enum Type
        {
            _int = 10,
            _double = 11,
            _decimal = 12,
            _string = 20,
            _datetime = 30,

            /// <summary>
            /// 日期(不可包含時間)
            /// </summary>
            _date = 31,

            /// <summary>
            /// yyyyMMdd
            /// </summary>
            _dateYMD = 32
        }

        /// <summary>
        /// src 是 Form 上傳 json 轉成的 ExpandoObject, 假設欄位都會是字串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="onlyFields"></param>
        /// <param name="type">目前僅 _string, _int, _datetime 有效</param>
        public static void CopyFromExpandoObject<T>(T dest, ExpandoObject src, string onlyFields = null, Type type = Type._string)
        {
            var dic = (IDictionary<string, object>)src;

            if (onlyFields != null)
            {
                onlyFields = "," + onlyFields + ",";
            }

            var fields = typeof(T).GetFields();
            var props = typeof(T).GetProperties().Where(x => x.CanWrite);

            foreach(string key in dic.Keys)
            {
                if ((onlyFields != null && !onlyFields.Contains("," + key + ",")) 
                    || dic[key] == null)
                {
                    continue;
                }

                object value = dic[key];
                switch (type)
                {
                    case Type._int:
                        value = value.ToString().ToInt32();
                        break;
                    case Type._datetime:
                        value = value.ToString().ToDate();
                        break;
                }

                try
                {
                    if (fields.Where(f => f.Name == key).FirstOrDefault() is System.Reflection.FieldInfo field)
                    {
                        field.SetValue(dest, value);
                    }
                    else if (props.Where(f => f.Name == key).FirstOrDefault() is System.Reflection.PropertyInfo prop)
                    {
                        prop.SetValue(dest, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("key: " + key + ", " + ex.FullInfo());
                }
            }
        }

        public static void CopyFromDataRow<T>(T dest, System.Data.DataRow row, string onlyFields = null)
        {
            var dt = row.Table;

            if (onlyFields != null)
            {
                onlyFields = "," + onlyFields + ",";
            }

            var fields = typeof(T).GetFields();
            foreach (var f in fields)
            {
                if(onlyFields != null && !onlyFields.Contains("," + f.Name + ",")){
                    continue;
                }

                //U2.WU.DebugWriteLine("f.Name: " + f.Name);
                if (dt.Columns.Contains(f.Name))
                {
                    if (Convert.IsDBNull(row[f.Name]))
                    {
                        //保留欄位預設值.
                    }
                    else
                    {
                        try
                        {
                            f.SetValue(dest, row[f.Name]);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("f.Name: " + f.Name + ", " + ex.FullInfo());
                        }
                    }
                }
            }

            var destProps = typeof(T).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();
            foreach (var property in destProps)
            {
                if (onlyFields != null && !onlyFields.Contains("," + property.Name + ",")){
                    continue;
                }

                if (dt.Columns.Contains(property.Name)){
                    if (Convert.IsDBNull(row[property.Name]))
                    {
                        //保留欄位預設值.
                    }
                    else
                    {
                        try
                        {
                            property.SetValue(dest, row[property.Name], null);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"property.Name: {property.Name}, {ex.FullInfo()}");
                        }
                    }
                }
            }
        }
    }
}
