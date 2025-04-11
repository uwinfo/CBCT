using Newtonsoft.Json.Converters;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
//using NPOI.SS.Formula.Functions;

namespace Su
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TTarget> SelectTo<TSource, TTarget>(this IQueryable<TSource> source) where TTarget : class, new()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            // 获取源类型和目标类型的属性
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 创建参数表达式
            var parameter = Expression.Parameter(sourceType, "x");

            // 创建绑定列表，只包含同名且类型相同的属性
            var bindings = targetProperties
                .Where(tp => sourceProperties.Any(sp => sp.Name == tp.Name))
                .Select(tp =>
                {
                    var sourceProperty = sourceProperties.First(sp => sp.Name == tp.Name);

                    Expression sourcePropertyExpression = Expression.Property(parameter, sourceProperty);

                    // Handle nullability differences
                    if (sourceProperty.PropertyType != tp.PropertyType)
                    {
                        sourcePropertyExpression = Expression.Convert(sourcePropertyExpression, tp.PropertyType);
                    }

                    return Expression.Bind(tp, sourcePropertyExpression);
                })
                .ToList();

            // 检查是否有绑定
            if (!bindings.Any())
            {
                throw new InvalidOperationException("No matching properties found between source and target types.");
            }

            // 创建成员初始化表达式
            var body = Expression.MemberInit(Expression.New(targetType), bindings);

            // 创建选择表达式
            var selector = Expression.Lambda<Func<TSource, TTarget>>(body, parameter);

            //Debug.WriteLine($"selector: {selector}");

            // 返回查询
            return source.Provider.CreateQuery<TTarget>(
                Expression.Call(
                    typeof(Queryable),
                    "Select",
                    new Type[] { sourceType, targetType },
                    source.Expression,
                    Expression.Quote(selector)
                )
            );
        }
    }

    public static class ExceptionExtension
    {
        public static string FullInfo(this Exception ex)
        {
            try
            {
                if (ex.InnerException != null)
                {
                    return $"Exception: {ex};\r\nInnerException: {ex.InnerException};\r\nStackTrace: {ex.StackTrace}";
                }
                else
                {
                    return $"Exception: {ex};\r\nStackTrace: {ex.StackTrace}";
                }
            }
            catch (Exception)
            {
                return $"Exception: {ex};\r\nStackTrace:  無";
            }
        }
    }

    public static class FileUploadExtion
    {
        public static string GetText(this IFormFile fu)
        {
            if (fu.Length > 0)
            {
                using var stream = fu.OpenReadStream();
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            return "";
        }

        public static bool IsValidExt(this IFormFile fu, string validExts = "jpg,gif,png,jpeg,xls,xlsx,svg")
        {
            if (fu == null || fu.Length <= 0)
            {
                return true;
            }

            if (("," + validExts.Replace(" ", "").ToLower() + ",").Contains("," + fu.Ext().ToLower() + ","))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 回傳值不包含 ".", 會轉小寫
        /// </summary>
        /// <param name="fu"></param>
        /// <returns></returns>
        public static string Ext(this IFormFile fu)
        {
            return Su.TextFns.FileExt(fu.FileName.ToLower());
        }

        public static void SaveAs(this IFormFile fu, string fullFileName)
        {
            using (Stream fileStream = new FileStream(fullFileName, FileMode.Create))
            {
                fu.CopyTo(fileStream);
            }
        }

        public static string SaveWithDate(this IFormFile fu, string diretory, string prefix = "", bool isCreateDirectory = true)
        {
            System.Threading.Thread.Sleep(5); //避免連續叫用, 檔名重覆.
            string filename = prefix + System.DateTime.Now.Ymdhmsf() + "." + fu.Ext();
            System.IO.Directory.CreateDirectory(diretory);
            using (Stream fileStream = new FileStream(Path.Combine(diretory, filename), FileMode.Create))
            {
                fu.CopyTo(fileStream);
            }

            return filename;
        }
    }

    public static class ExpressionExtension
    {
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var combineE = Expression.AndAlso(e1.Body, Expression.Invoke(e2, e1.Parameters[0]));

            return Expression.Lambda<Func<T, bool>>(combineE, e1.Parameters);
        }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var combineE = Expression.OrElse(e1.Body, Expression.Invoke(e2, e1.Parameters[0]));

            return Expression.Lambda<Func<T, bool>>(combineE, e1.Parameters);
        }

        public static Expression<Func<T, bool>> OrElseAll<T>(this IEnumerable<Expression<Func<T, bool>>> exps)
        {
            if (exps.Count() == 1)
            {
                return exps.First();
            }

            var e0 = exps.First();

            var orExp = exps.Skip(1).Aggregate(e0.Body, (x, y) => Expression.OrElse(x, Expression.Invoke(y, e0.Parameters[0])));

            return Expression.Lambda<Func<T, bool>>(orExp, e0.Parameters);
        }

        public static Expression<Func<T, bool>> AndAlsoAll<T>(this IEnumerable<Expression<Func<T, bool>>> exps)
        {
            if (exps.Count() == 1)
            {
                return exps.First();
            }

            var e0 = exps.First();

            var orExp = exps.Skip(1).Aggregate(e0.Body, (x, y) => Expression.AndAlso(x, Expression.Invoke(y, e0.Parameters[0])));

            return Expression.Lambda<Func<T, bool>>(orExp, e0.Parameters);
        }
    }

    public static class ObjectExtension
    {
        public static bool IsDBNull(this object obj)
        {
            return Convert.IsDBNull(obj);
        }

        public static DataTable ReOrderColumn(this DataTable dt, params (string oldColumnName, string newColumnNmae)[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                dt.Columns[columns[i].oldColumnName].SetOrdinal(i);
                dt.Columns[columns[i].oldColumnName].ColumnName = columns[i].newColumnNmae;
            }

            while (dt.Columns.Count > columns.Length)
            {
                dt.Columns.RemoveAt(dt.Columns.Count - 1);
            }

            return dt;
        }

        public static DataTable ToDataTable<T>(this List<T> items, string skips = null)
        {
            return ((IEnumerable<T>)items).ToDataTable(skips);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> items, string skips = null)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //設定DataTable欄位屬性
                Type columnType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, columnType);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static Dictionary<string, object?> ToDictionary<T>(this T src, string? skips = null)
        {
            var dic = new Dictionary<string, object?>();

            ObjUtil.CopyToDictionary(src, dic, skips);

            return dic;
        }

        /// <summary>
        /// 給 PostFileAsync 使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="skips"></param>
        /// <returns></returns>
        public static Dictionary<string, string?> ToStringDictionary<T>(this T src, string skips = null)
        {
            return ObjUtil.CopyPropertiesToStringDictionary(src, skips);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="propertyName">不分大小寫</param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T SetProperty<T>(this T dest, string propertyName, object value)
        {
            var destProps = dest.GetType().GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            if (destProps.Any(x => x.Name.ToLower() == propertyName.ToLower()))
            {
                var destItem = destProps.First(x => x.Name.ToLower() == propertyName.ToLower());
                if (destItem.CanWrite)
                { // check if the property can be set or no.
                    try
                    {
                        destItem.SetValue(dest, value, null);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("can't find property: " + propertyName + ", " + ex.FullInfo());
                    }
                }
            }

            return dest;
        }

        /// <summary>
        /// 會檢查 src 是否為 null, 同時呼叫 CopyPropertiesTo 和 CopyFieldsTo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        /// <param name="skips"></param>
        /// <returns></returns>
        public static T CopyFrom<T>(this T dest, object src, string skips = null)
        {
            if (src == null)
            {
                return dest;
            }
            ObjUtil.CopyTo(src, dest, skips);
            return dest;
        }

        /// <summary>
        /// 這個版本的 Copy To, 會觸發 dest 物件的每個 property 的 set 動作.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="skips"></param>
        /// <returns></returns>
        public static T CopyTo<T>(this object src, string? skips = null)
        {
            return ObjUtil.CopyTo(src, (T)Activator.CreateInstance(typeof(T)), skips);
        }

        public static T CopyTo<T>(this object src, T dest, string? skips = null)
        {
            return ObjUtil.CopyTo(src, dest, skips);
        }

        public static T CopyPropertiesTo<T>(this object src, T dest, string? skips = null)
        {
            return ObjUtil.CopyPropertiesTo(src, dest, skips);
        }

        /// <summary>
        ///  預設會接 Response.end, 自已本身不會 Response.Write
        /// </summary>
        /// <param name="value"></param>
        /// <param name="IsEnd"></param>
        /// <param name="DateFormat"></param>
        public static void WriteJSON(this object value, bool IsEnd = true, string DateFormat = null)
        {
            Su.Wu.WriteJSON(value, IsEnd, DateFormat);
        }

        /// <summary>
        /// 盡量不要在迴圈中使用. 可改用原生的 GetProperty(propertyName).GetValue(src, null);
        /// Dynamic 物件不適用.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object? GetPropertyValue(this object src, string propertyName)
        {
            var p = src.GetType().GetProperty(propertyName);
            if (p == null)
            {
                return null;
            }

            return p.GetValue(src, null);
        }

        public static object? GetFieldValue(this object src, string propertyName)
        {
            var p = src.GetType().GetField(propertyName);
            if (p == null)
            {
                return null;
            }
            return p.GetValue(src);
        }

        /// <summary>
        /// 會先試 field 再試 property
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetValue(this object src, string propertyName)
        {
            //System.Dynamic.ExpandoObject
            if (src.GetType().ToString() == "System.Dynamic.ExpandoObject")
            {
                IDictionary<string, object> dic = (IDictionary<string, object>)src;
                if (dic.ContainsKey(propertyName))
                {
                    return dic[propertyName];
                }
                else
                {
                    return null;
                }
            }

            return src.GetFieldValue(propertyName) ?? src.GetPropertyValue(propertyName);
        }

        /// <summary>
        /// 若是 ExpandoObject 會先轉 json 字串, 再轉物件.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T? GetValue<T>(this object src, string propertyName)
        {
            //System.Dynamic.ExpandoObject
            if (src.GetType().ToString() == "System.Dynamic.ExpandoObject")
            {
                IDictionary<string, object> dic = (IDictionary<string, object>)src;
                if (dic.ContainsKey(propertyName))
                {
                    return dic[propertyName].Json().JsonDeserialize<T>();
                }
                else
                {
                    return default;
                }
            }

            var res = src.GetPropertyValue(propertyName);
            if (res != null)
            {
                return (T)res;
            }

            res = src.GetFieldValue(propertyName);
            if (res != null)
            {
                return (T)res;
            }

            return default;
        }

        public static T JsonDeserialize<T>(this object value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string)value);
        }

        /// <summary>
        /// Convert.IsDBNull(obj) || string.IsNullOrEmpty( obj.ToString())
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDbNullOrEmpty(this object obj)
        {
            return Convert.IsDBNull(obj) || string.IsNullOrEmpty(obj.ToString());
        }

        public static bool IsDbNull(this object obj)
        {
            return Convert.IsDBNull(obj);
        }

        /// <summary>
        /// 和 DBNullToDefault 類似, 會判斷 IsDBNull，defaultValue 為 null，
        /// 日期會預設回傳 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <param name="dateTimeFormate"></param>
        /// <returns></returns>
        public static string? DbNullToNullString(this object value, string? defaultValue = null, string dateTimeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            if (Convert.IsDBNull(value) || value == null)
            {
                return defaultValue;
            }
            else
            {
                if (value.GetType().ToString() == "System.DateTime")
                {
                    return ((System.DateTime)value).ToString(dateTimeFormate);
                }

                return value.ToString();
            }
        }

        public static string DBNullToDefault(this object value, string defaultValue = "", string dateTimeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            if (Convert.IsDBNull(value) || value == null)
            {
                return defaultValue;
            }
            else
            {
                if (value.GetType().ToString() == "System.DateTime")
                {
                    return ((System.DateTime)value).ToString(dateTimeFormate);
                }

                return value.ToString();
            }
        }

        /// <summary>
        /// DBNull 時會回傳 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int DbToInt32_Zero(this object value)
        {
            return value != null && !Convert.IsDBNull(value) ? Convert.ToInt32(value) : 0;
        }

        //public static string Json(this object value, string DateFormat = null)
        //{
        //    if (DateFormat == null)
        //    {
        //        return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        //    }
        //    else
        //    {
        //        return Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = DateFormat });
        //    }
        //}

        /// <summary>
        /// 要注意, 沒有 Response.End
        /// </summary>
        /// <param name="value"></param>
        public static void WriteJSON(this object value)
        {
            Wu.WriteJSON(value);
        }

        public static Microsoft.AspNetCore.Mvc.OkObjectResult SuccessJsonResultData(this object data)
        {
            return new Microsoft.AspNetCore.Mvc.OkObjectResult(data.SuccessDataJson());
            //return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = true, data });
        }

        //public static string DBNullToDefault(this Object value, string Default = "")
        //{
        //    return (Convert.IsDBNull(value) || value == null) ? Default : value.ToString();
        //}

        public static Dictionary<string, object?> CopyToDictionary(this Object value)
        {
            var dic = new Dictionary<string, object?>();
            Su.ObjUtil.CopyToDictionary(value, dic);

            return dic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dateFormat"></param>
        /// <param name="isIndented"></param>
        /// <param name="isCamelCase">使用 Su.CamelCaseContractResolver</param>
        /// <returns></returns>
        public static string Json(this Object value, string dateFormat = null, bool isIndented = false, bool isCamelCase = false)
        {
            var setting = new Newtonsoft.Json.JsonSerializerSettings();

            if (dateFormat != null)
            {
                setting.DateFormatString = dateFormat;
            }

            if (isIndented)
            {
                setting.Formatting = Newtonsoft.Json.Formatting.Indented;
            }

            if (isCamelCase)
            {
                //DefaultContractResolver contractResolver = new DefaultContractResolver
                //{
                //    NamingStrategy = new Su.CamelCaseContractResolver();
                //};

                //setting.ContractResolver = contractResolver;
                setting.ContractResolver = new Su.CamelCaseContractResolver();
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(value, setting);
        }

        public static string SuccessDataJson(this Object data, string DateFormat = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, data }, Newtonsoft.Json.Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = DateFormat });
        }
    }

    public static partial class StringExtension
    {
        /// <summary>
        /// 會把 sql 中的 [Columns] 用 parameters 的欄位取代
        /// 會把 sql 中的 [Values] 用 parameters 的欄位，加上 {} 後取代
        /// 最後再呼叫 ToMsSql
        /// 若有固定欄位可寫在前方, ex: "INSERT INTO [dbo].[Engagement](TypeSource, [Columns]) VALUES('Insider Default', [Values])"
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string AddMsSqlInsertColumns(this string sql, object parameters)
        {
            var sourceProperties = parameters.GetType().GetProperties().ToList();
            sql = sql.Replace("[Columns]", sourceProperties.Select(p => p.Name.MsSqlObj()).ToOneString(", "));
            sql = sql.Replace("[Values]", sourceProperties.Select(p => "{" + p.Name + "}").ToOneString(", ")); //這裡的 Name 都會被取代，所以不會有 injection 的問題

            return sql;
        }

        public static string ToMsSql(this string sql, Dictionary<string, object> parameters, Dictionary<string, string>? sqlObjects = null, bool isCheckDangerSQL = true, bool isRemoveCrLf = true)
        {
            //有傳入 parameters 了, 預設可以把 CRLF 拿掉, 變數應該會在 parameters 之中。
            if (isRemoveCrLf)
            {
                sql = sql.Replace("\r", " ").Replace("\n", " ");
            }

            if (isCheckDangerSQL)
            {
                MsSql.CheckDangerSQL(sql);
            }

            {
                var keys = parameters.Keys;
                foreach (string key in keys)
                {
                    sql = sql.Replace("{" + key + "}", parameters[key].MsSqlValue());
                }
            }


            if (sqlObjects != null)
            {
                var keys = sqlObjects.Keys;
                foreach (string key in keys)
                {
                    sql = sql.Replace("[" + key + "]", sqlObjects[key].MsSqlObj());
                }
            }

            return sql;
        }

        /// <summary>
        /// 組成要執行的 SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
        /// <param name="isCheckDangerSQL"></param>
        /// <param name="isRemoveCrLf"></param>
        /// <returns></returns>
        public static string ToMsSql(this string sql, object? parameters, object? sqlObjects = null, bool isCheckDangerSQL = true, bool isRemoveCrLf = true, bool isNotUnicode = false)
        {
            //有傳入 parameters 了, 預設可以把 CRLF 拿掉, 變數應該會在 parameters 之中。
            if (isRemoveCrLf)
            {
                sql = sql.Replace("\r", " ").Replace("\n", " ");
            }

            if (isCheckDangerSQL)
            {
                MsSql.CheckDangerSQL(sql);
            }

            if (parameters != null)
            {
                var sourceProperties = parameters.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("{" + srcItem.Name + "}", srcItem.GetValue(parameters).MsSqlValue(isNotUnicode));
                }
            }

            if (sqlObjects != null)
            {
                var sourceProperties = sqlObjects.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlObjects).ToString().MsSqlObj());
                }
            }

            return sql;
        }

        /// <summary>
        /// 這裡的 ColumnName 不會把 . 展開
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlColumns"></param>
        /// <returns></returns>
        public static string ReplacePgSqlColumnNames(this string sql, object sqlColumnNames = null)
        {
            var sourceProperties = sqlColumnNames.GetType().GetProperties().ToList();

            foreach (var srcItem in sourceProperties)
            {
                sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlColumnNames).ToString().PgSqlColumnName(isReplaceDot: false));
            }

            return sql;
        }

        /// <summary>
        /// 組成要執行的 SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
        /// <param name="isCheckDangerSQL"></param>
        /// <param name="isRemoveCrLf"></param>
        /// <returns></returns>
        public static string ToPgSql(this string sql, object? parameters, object sqlColumns = null, object sqlTables = null, bool isCheckDangerSQL = true, bool isRemoveCrLf = true)
        {
            //有傳入 parameters 了, 預設可以把 CRLF 拿掉, 變數應該會在 parameters 之中。
            if (isRemoveCrLf)
            {
                sql = sql.Replace("\r", " ").Replace("\n", " ");
            }

            if (isCheckDangerSQL)
            {
                PgSql.CheckDangerSQL(sql);
            }

            if (parameters != null)
            {
                var sourceProperties = parameters.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("{" + srcItem.Name + "}", srcItem.GetValue(parameters).PgSqlValue());
                }
            }

            if (sqlColumns != null)
            {
                var sourceProperties = sqlColumns.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlColumns).ToString().PgSqlColumnName());
                }
            }

            if (sqlTables != null)
            {
                var sourceProperties = sqlTables.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlTables).ToString().PgSqlTableName());
                }
            }

            return sql;
        }

        /// <summary>
        /// 使用 Converter 看來沒有明顯的時間差異, 跑 10000000 測試, 時間比在 1.052 ~ 0.902 之間
        /// </summary>
        public class Converter
        {
            public string src;
            public Converter(string src)
            {
                this.src = src;
            }

            public DateTime ToDate()
            {
                return System.DateTime.Parse(src);
            }
        }

        //public class SQLClass
        //{
        //    /// <summary>
        //    /// 這個 SQL 通常是 table name 再加上 (Nolock)
        //    /// </summary>
        //    public string sql;

        //    public SQLClass(string sql)
        //    {
        //        this.sql = sql;
        //    }

        //    /// <summary>
        //    /// 原字串可以為 Select XX, YY from [ZZ], 或是 ZZ(沒有空白時, 視為 TableName, SQL 自動變成 Select * from [TableName]
        //    /// </summary>
        //    /// <returns></returns>
        //    public SQL.SQLBuilder Builder()
        //    {
        //        return new SQL.SQLBuilder(this.sql);
        //    }

        //    /// <summary>
        //    /// "Select count(" + Fields + ") From " + TableNameFromSQL, 預設會使用Nolock
        //    /// </summary>
        //    /// <returns></returns>
        //    public SQL.SQLBuilder SelectCount(string Fields = "*", bool IsNolock = true)
        //    {
        //        _IsNolock = IsNolock;

        //        return new SQL.SQLBuilder("Select count(" + Fields + ") From " + TableNameFromSQL);
        //    }

        //    private bool _IsNolock = false;
        //    public string TableNameFromSQL
        //    {
        //        get
        //        {
        //            sql = sql.Trim();

        //            string SqlObj = "";
        //            if (sql.ToLower().EndsWith(" (nolock)"))
        //            {
        //                SqlObj = sql.Substring(0, sql.Length - 9).SqlObj() + " (NoLock)";
        //            }
        //            else
        //            {
        //                if (_IsNolock)
        //                {
        //                    SqlObj = sql.SqlObj() + " (NoLock)";
        //                }
        //                else
        //                {
        //                    SqlObj = sql.SqlObj();
        //                }
        //            }

        //            return SqlObj;
        //        }
        //    }

        //    /// <summary>
        //    /// "Select " + Fields + " From " + TableNameFromSQL + "Where Id = " + Id
        //    /// </summary>
        //    /// <returns></returns>
        //    public SQL.SQLBuilder Select(string Fields = "*", int? Id = null, int? top = null, bool IsNolock = true)
        //    {
        //        string TopClause = "";

        //        if (top != null)
        //        {
        //            TopClause = " Top " + top + " ";
        //        }

        //        _IsNolock = IsNolock;

        //        var SB = new SQL.SQLBuilder("Select " + TopClause + Fields + " From " + TableNameFromSQL);
        //        if (Id != null)
        //        {
        //            SB.Where("Id = ", Id);
        //        }
        //        return SB;
        //    }

        //    public SQL.SQLBuilder Select(int Id)
        //    {
        //        return Select("*", Id);
        //    }

        //    /// <summary>
        //    /// 會使用 Select * from TableNameFromSQL 建立 SQL Builder, 避免在大 table 使用.
        //    /// </summary>
        //    /// <returns></returns>
        //    public SQL.SQLBuilder SelectAllBuilder()
        //    {
        //        return new SQL.SQLBuilder("Select * from " + TableNameFromSQL);
        //    }

        //    public SQL.UpdateSQLBuilder UpdateBuilder(int Id)
        //    {
        //        return UpdateBuilder(false, Id: Id);
        //    }

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="IsGetStructure"></param>
        //    /// <param name="Id"></param>
        //    /// <param name="UserId">會使用 SetUser_Auto 來設定更新者的資訊</param>
        //    /// <returns></returns>
        //    public SQL.UpdateSQLBuilder UpdateBuilder(Boolean IsGetStructure = false, int? Id = null, int? UserId = null)
        //    {
        //        var oUB = new SQL.UpdateSQLBuilder(this.sql);

        //        if (IsGetStructure)
        //        {
        //            oUB.GetTableStructure();
        //        }

        //        if (Id != null)
        //        {
        //            oUB.Where("Id = ", Id);
        //        }

        //        if (UserId != null)
        //        {
        //            oUB.SetUser_Auto((int)UserId);
        //        }

        //        return oUB;
        //    }




        //    public SQL.InsertSQLBuilder InsertBuilder(Boolean IsGetStructure = false)
        //    {
        //        var oIB = new SQL.InsertSQLBuilder(this.sql);

        //        if (IsGetStructure)
        //        {
        //            oIB.GetTableStructure();
        //        }

        //        return oIB;
        //    }

        //    public SQL.DeleteSQLBuilder DeleteBuilder()
        //    {
        //        return new SQL.DeleteSQLBuilder(this.sql);
        //    }

        //    /// <summary>
        //    /// 傳入值是 Table Name
        //    /// </summary>
        //    /// <returns></returns>
        //    public bool IsTableExists(string DBC = null)
        //    {
        //        return ! Su.SQL.GetSingleValue("Select OBJECT_ID('dbo." + sql.SqlField() + "', 'U')", DBC).IsDBNULL();
        //    }

        //}



        public class ImageClass
        {
            public string filename;
            public ImageClass(string filename)
            {
                this.filename = filename;
            }

            ///// <summary>
            ///// 第一個參數 為舊的檔名
            ///// </summary>
            ///// <param name="NewFile"></param>
            ///// <param name="W"></param>
            ///// <param name="H"></param>
            ///// <param name="Quality"></param>
            ///// <param name="Interpolation"></param>
            //public void ResizeImage(string NewFile, int W = -1, int H = -1, int Quality = 80, int Interpolation = 5, double Threshold = 0.35)
            //{
            //    //因為比對過原圖日期了, 所以一率刪檔重建
            //    if (System.IO.File.Exists(NewFile))
            //    {
            //        //UW.WU.DebugWriteLine("Delete " + NewFile, false, true);
            //        System.IO.File.Delete(NewFile);
            //    }





            //    var objJpeg = new ASPJPEGLib.ASPJpeg();
            //    objJpeg.Open(filename);

            //    if (((double)new System.IO.FileInfo(filename).Length / (objJpeg.Width * objJpeg.Height)) > Threshold)
            //    {


            //        if (W != -1 && W > 0) { objJpeg.Width = W; }
            //        if (H != -1 && H > 0) { objJpeg.Height = H; }

            //        objJpeg.Quality = Quality;
            //        objJpeg.Interpolation = Interpolation;
            //        objJpeg.Progressive = 1;
            //    }
            //    objJpeg.Save(NewFile);
            //    objJpeg.Close();
            //}
        }


        public static ImageClass Image(this string value)
        {
            return new ImageClass(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">值</param>
        /// <param name="sqlObjects">欄位</param>
        /// <param name="isCheckInection"></param>
        /// <returns></returns>
        public static MsSql.Criteria ToSqlCriteria(this string sql, object parameters, object sqlObjects = null, bool isCheckInection = true)
        {
            return MsSql.Criteria.GetSinglCriteria(sql, parameters, sqlObjects, isCheckInection);
        }

        public static Converter C(this string value)
        {
            return new Converter(value);
        }

        //public static MsSql.MsSqlBuilder MsSqlBuilder(this string value)
        //{
        //    return new MsSql.MsSqlBuilder(value);
        //}

        public static IOClass IO(this string value)
        {
            return new IOClass(value);
        }

        public class IOClass
        {
            string _src = "";

            public IOClass(string src)
            {
                this._src = src;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="path"></param>
            /// <param name="enc">預設是 UTF8</param>
            public void WriteToFile(string path, Encoding enc = null)
            {
                if (enc == null)
                {
                    enc = System.Text.Encoding.UTF8;
                }
                System.IO.File.WriteAllText(path, _src, enc);
            }
        }

        public const string vbCrLf = "\r\n";

        public static string CRLFtoBR(this string value)
        {
            if (value == null)
            {
                return null;
            }
            return value
                    .Replace("\r\n", "<br>")
                    .Replace("\n", "<br>")
                    .Replace("\r", "<br>");
        }

        //public static void DebugWriteLine(this string value, bool IsHtmlEncode = false, bool IsStop = false, bool IsFlush = false, bool IsAddTimeFlag = false)
        //{
        //    WU.DebugWriteLine(value, IsHtmlEncode, IsStop, IsFlush, IsAddTimeFlag);
        //}

        public static string SuccessMessageJson(this string msg)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, msg });
        }

        public static string ErrorMessageJson(this string msg)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = false, msg });
        }

        //public static object SuccessObject(this string msg)
        //{
        //    return new { success = true, msg };
        //}

        public static Microsoft.AspNetCore.Mvc.JsonResult SuccessJsonResultMessage(this string msg)
        {
            return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = true, msg });
        }

        public static Microsoft.AspNetCore.Mvc.JsonResult ErrorJsonResultMessage(this string msg)
        {
            return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = false, msg });
        }

        public static object ErrorObject(this string msg)
        {
            return new { success = false, msg };
        }

        public static string GotoJs(this string returnURL, bool IsTop = false)
        {
            string top = "";
            if (IsTop)
            {
                top = ".top";
            }

            return "<script>window" + top + ".location.href='" + returnURL + "';\r\n</script>";
        }



        /// <summary>
        /// window alert, 有加 script tag
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="IsBack"></param>
        /// <returns></returns>
        public static string ShowMessageJs(this string Message, bool IsBack = false)
        {
            string Back = "";
            if (IsBack)
            {
                Back = "window.history.back();\r\n";
            }

            string JS = @"<script>
    window.alert('" + Message.JavaScriptString() + @"');
    " + Back + @"
</script>";

            return JS;
        }

        public static string JavaScriptString(this string value)
        {
            return System.Web.HttpUtility.JavaScriptStringEncode(value);
        }

        public static T JsonDeserialize<T>(this string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }

        public static string GetRemotePage(this string value, Encoding oE = null)
        {
            if (oE == null)
            {
                oE = Encoding.UTF8;
            }

            return Wu.GetRemotePage(value, oE);
        }

        public static string Post(this string url, NameValueCollection nvc, Encoding oE = null)
        {
            return Wu.Post(url, nvc, oE);
        }

        /// <summary>
        /// 同 urlencode, 但空的會轉為 %20
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapeDataString(this string value)
        {
            return System.Uri.EscapeDataString(value);
        }

        /// <summary>
        /// 建議改用 EscapeDataString, 避免空白變加號.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oE"></param>
        /// <returns></returns>
        public static string UrlEncode(this string value, Encoding? oE = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return System.Web.HttpUtility.UrlEncode(value, oE ?? Encoding.UTF8);
        }

        /// <summary>
        /// 建議改用 EscapeDataString, 避免空白變加號.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="oE"></param>
        /// <returns></returns>
        public static string UrlDecode(this string value, Encoding? oE = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return System.Web.HttpUtility.UrlDecode(value, oE ?? Encoding.UTF8);
        }

        public static string HtmlEncode(this string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value);
        }

        /// <summary>
        /// 主要用於 response.writefile 時的檔名.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UrlPathEncode(this string value)
        {
            return System.Web.HttpUtility.UrlPathEncode(value);
        }


        /// <summary>
        /// XMLEncode
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string XMLEncode(this string value)
        {
            return System.Security.SecurityElement.Escape(value);
        }


        /// <summary>
        /// 拿掉 !ENTITY, 避免 XXE 攻擊
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SecureXML(this string value)
        {
            return value.Replace("<!ENTITY", "<_!ENTITY");
        }

        public static XmlDocument SecureXMLDoc(this string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(value.SecureXML());
            return doc;
        }

        /// <summary>
        /// 把 ` 刪除, 再用 `` 包住字串, 把 "." 換成 "`.`"
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isOderBy"></param>
        /// <returns></returns>
        public static string MySqlObj(this string value, bool isOderBy = false)
        {
            var res = value.Trim();
            var postfix = "";

            if (isOderBy && res.ToLower().EndsWith(" desc"))
            {
                postfix = " desc";
                res = res[0..^5];
            }

            if (res.EndsWith(" +="))
            {
                res = res.Replace(" +=", "");
            }

            return "`" + res.Replace("`", "").Replace(".", "`.`") + "`" + postfix;
        }

        public static string PgSqlValue(this object value)
        {
            if (value == null)
            {
                return " null ";
            }

            if (value.GetType().ToString() != "System.String")
            {
                IEnumerable? enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    var values = new List<object>();

                    foreach (var item in enumerable)
                    {
                        if (item != null) // In 不能傳入 null
                        {
                            values.Add(item);
                        }
                    }

                    if (values.Count == 0)
                    {
                        return "()"; //這個會發生 Exception.
                    }

                    return $"({string.Join(", ", values.Select(i => i.PgSqlValue()))})";
                }
            }


            switch (value.GetType().ToString())
            {
                case "System.Char":
                case "System.String":
                    return "'" + value?.ToString()?.Replace("'", "''") + "'";
                case "System.Int32":
                    return ((int)value).ToString();
                case "System.Int64":
                    return ((Int64)value).ToString();
                case "System.Decimal":
                    return ((decimal)value).ToString();
                case "System.DateTime":
                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.ffffff") + "'";
                case "System.Single":
                    return ((Single)value).ToString();
                case "System.Double":
                    return ((Double)value).ToString();
                case "System.Int64[]":
                    {
                        var values = (Int64[])value;
                        return $"'{{{values.Select(v => v.ToString()).ToOneString(",")}}}'";
                    }
                case "System.Int32[]":
                    {
                        var values = (Int32[])value;
                        return $"'{{{values.Select(v => v.ToString()).ToOneString(",")}}}'";
                    }
                default:
                    throw new Exception("不認識的型別: " + value.GetType().ToString());
            }
        }

        public static string PgSqlValue(this object value, bool IsNotUnicode = false)
        {
            if (value == null)
            {
                return " null ";
            }

            switch (value.GetType().ToString())
            {
                case "System.Char":
                case "System.String":
                    return "'" + value.ToString().Replace("'", "''") + "'";
                case "System.Int32":
                    return ((int)value).ToString();
                case "System.Int64":
                    return ((Int64)value).ToString();
                case "System.Decimal":
                    return ((decimal)value).ToString();
                case "System.DateTime":
                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";
                case "System.Single":
                    return ((Single)value).ToString();
                case "System.Double":
                    return ((Double)value).ToString();
                default:
                    throw new Exception("不認識的型別: " + value.GetType().ToString());
            }
        }

        public static string MsSqlValue(this object? value, bool IsNotUnicode = false)
        {
            if (value == null)
            {
                return " null ";
            }

            if (Convert.IsDBNull(value))
            {
                return " null ";
            }

            if (value.GetType().ToString() != "System.String")
            {
                IEnumerable? enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    var values = new List<object>();

                    foreach (var item in enumerable)
                    {
                        if (item != null) // In 不能傳入 null
                        {
                            values.Add(item);
                        }
                    }

                    if (values.Count == 0)
                    {
                        return "()"; //這個會發生 Exception.
                    }

                    return $"({string.Join(", ", values.Select(i => i.MsSqlValue()))})";
                }
            }

            switch (value.GetType().ToString())
            {
                case "System.Char":
                case "System.String":
                    if (IsNotUnicode)
                    {
                        return "'" + value.ToString().Replace("'", "''") + "'";
                    }
                    else
                    {
                        return "N'" + value.ToString().Replace("'", "''") + "'";
                    }
                case "System.Int32":
                    return ((int)value).ToString();
                case "System.Int64":
                    return ((Int64)value).ToString();
                case "System.Decimal":
                    return ((decimal)value).ToString();
                case "System.DateTime":
                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";
                case "System.Single":
                    return ((Single)value).ToString();
                case "System.Double":
                    return ((Double)value).ToString();
                default:
                    if (value is Enum)
                    {
                        return ((int)value).ToString();
                    }
                    throw new Exception("不認識的型別: " + value.GetType().ToString());
            }
        }

        public static string ToOneString(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list);
        }

        /// <summary>
        /// 會保持 url1 和 url2 之間有一個 /
        /// </summary>
        /// <param name="url1"></param>
        /// <param name="url2"></param>
        /// <returns></returns>
        public static string CombineUrl(this string url1, string url2)
        {
            if (url1.EndsWith("/"))
            {
                if (url2.StartsWith("/"))
                {
                    return url1 + url2.Substring(1);
                }
                return url1 + url2;
            }
            else
            {
                if (url2.StartsWith("/"))
                {
                    return url1 + url2;
                }
                return url1 + "/" + url2;
            }
        }

        /// <summary>
        /// 使用 System.IO.Path.Combine(firstPath, path) 來合併 Path，後方的 Paths 會被移除前置的 / 或 \ ，並經由 System.IO.Path.IsPathRooted 檢查是否為完整目錄
        /// </summary>
        /// <param name="firstPath"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string AddPath(this string firstPath, params string[] paths)
        {
            //使用變數時，第一個參數有可能是 null
            if (string.IsNullOrEmpty(firstPath))
            {
                throw new Exception("AddPath 的第一個參數不可為 null");
            }

            string finalPath = firstPath;
            foreach (var path in paths)
            {
                if (path.Contains(".."))
                {
                    throw new Exception("路徑中不可包含 ..");
                }

                //後方的 Paths 會被移除前置的 / 或 \ ，並經由 System.IO.Path.IsPathRooted 檢查是否為完整目錄
                var subPath = path;
                while (subPath.StartsWith("/") || subPath.StartsWith(@"\"))
                {
                    subPath = subPath[1..];
                }

                if (System.IO.Path.IsPathRooted(subPath))
                {
                    throw new Exception("不可併入完整路徑 ..");
                }
                //修正Combine後台路徑，slash 在 LINUX 用錯會出錯
                finalPath = System.IO.Path.Combine(finalPath, subPath).Replace("\\", "/");
            }


            if (!finalPath.StartsWith(firstPath))
            {
                throw new Exception("合併路徑發生問題 ..");
            }

            return finalPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string AddUrl(this string url, params string[] paths)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new Exception("請指定根目錄");
            }

            foreach (var path in paths)
            {
                if (path.Contains(".."))
                {
                    throw new Exception("不允許使用 .. ");
                }

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (url.EndsWith("/") && path.StartsWith("/"))
                {
                    if (path.Length > 1)
                    {
                        url += path.Substring(1);
                    }
                }
                else if (url.EndsWith("/") || path.StartsWith("/"))
                {
                    url += path;
                }
                else
                {
                    url += "/" + path;
                }
            }
            return url;
        }

        /// <summary>
        /// 會把 parameterValue 做 EscapeDataString 之後再併到 Query String 之中。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public static string AddQueryString(this string url, string parameterName, string parameterValue)
        {
            if (parameterValue == null)
            {
                return url;
            }

            url = url.AddQueryString(parameterName + "=" + parameterValue.EscapeDataString());

            return url;
        }

        public static string AddQueryString(this string url, object parameters)
        {
            var sourceProps = parameters.GetType().GetProperties().Where(x => x.CanRead).ToList();

            foreach (var srcItem in sourceProps)
            {
                url = url.AddQueryString(srcItem.Name + "=" + srcItem.GetValue(parameters, null)?.ToString().EscapeDataString());
            }

            return url;
        }

        public static string AddQueryString(this string url, string queryString)
        {
            if (!String.IsNullOrWhiteSpace(queryString))
            {
                if (queryString[0] == '&' || queryString[0] == '?')
                {
                    queryString = queryString.Substring(1);
                }

                if (url.IndexOf("?") > 0)
                {
                    queryString = "&" + queryString;
                }
                else
                {
                    queryString = "?" + queryString;
                }
            }
            else
            {
                queryString = "";
            }

            return url + queryString;
        }

        //public static string DecryptStringFromBytes_AesCBC(this string cipherText, string Key, String IV)
        //{
        //    return Crypto.DecryptStringFromBytes_AesCBC(cipherText, Key, IV);
        //}


        /// <summary>
        /// 直接傳入的 SQL, 目前檢查是否有 "--", ";", "\r", "\n"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMsSqlInjection(this string value)
        {
            return (value.Contains("--") || value.Contains("/*") || value.Contains(";") || value.Contains("\r") || value.Contains("\n"));
        }

        const string PostgreSqlDangerWords = "exec|copy|drop|create|alter|truncate|grant|revoke|commit|rollback|savepoint|declare|execute|prepare|fetch|close|open|fetch|move|copy|listen|notify";

        /// <summary>
        /// 注意 "\\" 代表 "\", "\]" 代表 "]"
        /// Regex.Escape 遇到 "]" 會轉譯錯誤
        /// </summary>
        const string PostgreSqlValidCharacters = @" '!#$%&()*+,-./:;<=>?@[\\\]^_~ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@";

        /// <summary>
        /// 目前檢查是否有 "--", ";", "\r", "\n", "/*"
        /// 必需由 PostgreSqlValidCharacters 中的字元組成
        /// 不可包含 PostgreSqlDangerWords 中的字串
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static void CheckPgSqlInjection(this string sql)
        {
            if (sql.Contains("--") || sql.Contains("/*") || sql.Contains(";") || sql.Contains("\r") || sql.Contains("\n"))
            {
                throw new Exception("可能有 Sql Injection, 不允許 --, /*, ;, 換行 等字串");
            }

            // 建立正則表達式，檢查是否僅包含合法字符
            string pattern = $"^[{PostgreSqlValidCharacters}]+$";

            // 檢查 SQL 是否僅包含合法字符
            if(!Regex.IsMatch(sql, pattern))
            {
                var invalidChars = FindInvalidChars(sql, PostgreSqlValidCharacters);
                throw new Exception("可能有 Sql Injection, sql 中含有不合法的字元: " + string.Concat(invalidChars));
            }

            // 檢查 SQL 是否包含危險字串
            if(Regex.IsMatch(sql, @"\b(" + PostgreSqlDangerWords + @")\b", RegexOptions.IgnoreCase))
            {
                throw new Exception("可能有 Sql Injection, sql 中包含危險字串");
            }
        }

        static List<char> FindInvalidChars(string sql, string validChars)
        {
            List<char> invalidChars = new List<char>();

            // 遍歷 SQL 語句的每個字符
            foreach (char c in sql)
            {
                // 如果字符不在合法字符清單中，則加入非法字符列表
                if (validChars.IndexOf(c) == -1)
                {
                    invalidChars.Add(c);
                }
            }

            return invalidChars;
        }

        /// <summary>
        /// SQL 字串正規化(SQL 中不應該有資料，所以這裡不考慮資料問題)
        /// 1. 所有的連續空白改為一個空白 (含 \r\t\n)
        /// 2. trim
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlNormalize(this string value)
        {
            value = Regex.Replace(value, @"\s+", " ");// 替換所有連續空白為單一空白
            value = value.Trim();
            return value;
        }

        /// <summary>
        /// 直接傳入的 SQL, 目前檢查是否有 "--", ";", "#", "/*", "\r", "\n"
        /// # 有點麻煩, 很容易衝突.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsMySQLInjection(this string value)
        {
            return (value.Contains("#") || value.Contains("/*") || value.Contains("--") || value.Contains(";") || value.Contains("\r") || value.Contains("\n"));
        }

        /// <summary>
        /// "N'" + value.Replace("'", "''") + "'";
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlNChar(this string value)
        {
            return "N'" + value.Replace("'", "''") + "'";
        }

        /// <summary>
        /// "N'" + value.Replace("'", "''") + "'";
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlNCharAndTrim(this string value)
        {
            return "N'" + value.Trim().Replace("'", "''") + "'";
        }

        /// <summary>
        /// 回傳 [XXX], 假設欄位名稱中不會有 [ 或 ] 的符號
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MsSqlField(this string value)
        {
            return "[" + value.Replace("[", "").Replace("]", "") + "]";
        }

        /// <summary>
        /// 假設欄位中不會有 ` 符號
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string MySqlField(this string value)
        {
            return "`" + value.Replace("`", "").Replace("`", "") + "`";
        }

        public static string RemoveHtml(this string value)
        {
            return Regex.Replace(value, "<.*?>", string.Empty);
        }

        public static string RemoveHtmlComments(this string value)
        {
            return Regex.Replace(value, "<!--(.|\\s)*?-->", string.Empty);
        }

        /// <summary>
        /// 用來比對 Regular Expression.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="regexOptions"></param>
        /// <returns></returns>
        public static bool IsMatch(this string value, string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            Regex r = new Regex(pattern, regexOptions);

            return r.Match(value).Success;
        }

        public static string MaskName(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
            {
                return value;
            }

            return $"*{value.Substring(1, 1)}*";

            //if (value.Length == 2)
            //{
            //    return $"*value.Substring(1, 1)";
            //}
            //else
            //{
            //    return $"*{value.Substring(1, 1)}*";
            //}
        }

        public static string MaskAddress(this string value)
        {
            if (value.Contains("巷"))
            {
                return "**" + value.Substring(value.IndexOf("巷"));
            }

            if (value.Contains("段"))
            {
                return "**" + value.Substring(value.IndexOf("段"));
            }

            if (value.Contains("路"))
            {
                return "**" + value.Substring(value.IndexOf("路"));
            }

            if (value.Contains("村"))
            {
                return "**" + value.Substring(value.IndexOf("村"));
            }

            if (value.Contains("鄉"))
            {
                return "**" + value.Substring(value.IndexOf("鎮"));
            }

            if (value.Contains("鎮"))
            {
                return "**" + value.Substring(value.IndexOf("鎮"));
            }

            if (value.Contains("區"))
            {
                return "**" + value.Substring(value.IndexOf("區"));
            }

            if (value.Length > 5)
            {
                return "**" + value[^5..];
            }

            return value;
        }


        public static string BrToCr(this string value)
        {
            return Regex.Replace(
                Regex.Replace(value,
                    "(<br />|<br/>|</ br>|</br>|<br>)", "\r"),
                "(<BR />|<BR/>|</ BR>|</BR>|<BR>)", "\r");
        }

        public static string RemoveCrLf(this string value)
        {
            return value.Replace("\r", "").Replace("\n", "");
        }

        public static string[] SplitCrLf(this string value)
        {
            return value.Replace("\r\n", "\r").Replace("\n\r", "\r").Replace("\n", "\r").Split('\r');
        }

        /// <summary>
        /// 逗號切開後, 先 Trim 再拿掉空字串.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string[] SplitCommaAndTrim(this string value)
        {
            if (value == null || value == "")
            {
                return new string[] { };
            }

            var Q = value.Split(',').Select(S => S.Trim()).Where(S => S != "");
            if (Q.Count() > 0)
            {
                return Q.ToArray();
            }
            else
            {
                return new string[] { };
            }
            //return value.Split(',').Select(S => S.Trim()).Where(S => S != "").ToArray();
        }

        /// <summary>
        ///  傳入: AAA,BB,,C,,,Dxx  傳回: 'AAA','BB','C','Dxx'
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SqlListStr(this string str)
        {
            return TextFns.GetSqlStingList(str);
        }



        /// <summary>
        /// 非整數和空白自動移除. ex: 傳入: ,A,B,1,2,3,4,5,,, 傳出: 1,2,3,4,5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SqlListInt(this string str)
        {
            return TextFns.GetIntList(str);
        }

        public static bool SmallerThan(this string str, string otherStr)
        {
            return (string.Compare(str, otherStr) < 0);
        }

        public static bool SmallerThanOrEqualTo(this string str, string otherStr)
        {
            return (string.Compare(str, otherStr) <= 0);
        }

        public static bool BiggerThan(this string str, string otherStr)
        {
            return (string.Compare(str, otherStr) > 0);
        }

        public static bool BiggerThanOrEqualTo(this string str, string otherStr)
        {
            return (string.Compare(str, otherStr) >= 0);
        }

        /// <summary>
        /// 是否為數字, 包括浮點數.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string str)
        {
            //return Double.TryParse(Convert.ToString(str), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out double retNum);

            return TextFns.IsNumeric(str);
        }

        /// <summary>
        /// 判斷是否全都是數字, (new Regex(@"^\d$")).IsMatch(str)
        /// 不包含 "." 和 "-"
        /// 注意，IsNumeric 或 IsInt 有可能包含 "." 和 "-"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigitOnly(this string str)
        {
            return (new Regex("^[0-9]+$")).IsMatch(str);
        }

        /// <summary>
        /// 是否為整數. 這裡會叫用 int.TryParse, 若之後要轉換形別, 直接用 int.TryParse 比較快.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            return TextFns.IsInt(str);
        }

        /// <summary>
        /// 是否為台灣地區統編 8 位數字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTaxNo(this string idNo)
        {
            if (idNo == null)
            {
                return false;
            }
            Regex regex = new Regex(@"^\d{8}$");
            Match match = regex.Match(idNo);
            if (!match.Success)
            {
                return false;
            }
            int[] idNoArray = idNo.ToCharArray().Select(c => Convert.ToInt32(c.ToString())).ToArray();
            int[] weight = new int[] { 1, 2, 1, 2, 1, 2, 4, 1 };

            int subSum;     //小和
            int sum = 0;    //總和
            int sumFor7 = 1;
            for (int i = 0; i < idNoArray.Length; i++)
            {
                subSum = idNoArray[i] * weight[i];
                sum += (subSum / 10)   //商數
                     + (subSum % 10);  //餘數                
            }
            if (idNoArray[6] == 7)
            {
                //若第7碼=7，則會出現兩種數值都算對，因此要特別處理。
                sumFor7 = sum + 1;
            }
            return (sum % 10 == 0) || (sumFor7 % 10 == 0);
        }

        /// <summary>
        /// 是否為捐贈碼
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDonateCode(this string str)
        {
            //https://www.cetustek.com.tw/news.php?id=186
            //捐贈碼
            //總長度為3至7碼字元
            //全部由數字【0 - 9】組成

            var pattern = @"^[0-9]{3,7}$";

            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            Match m = r.Match(str);

            return (m.Success && m.Groups[0].Value == str);
        }

        /// <summary>
        /// 是否為行動載具碼
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMobileCarrierCode(this string str)
        {
            //https://www.cetustek.com.tw/news.php?id=186
            //手機條碼
            //由Code39組成，總長度為8碼字元
            //第一碼必為『/』
            //其餘七碼則由數字【0 - 9】、大寫英文【A - Z】與特殊符號【.】【-】【+】組成

            var pattern = @"^\/{1}[0-9A-Z\.\-\+]{7}$";

            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            Match m = r.Match(str);

            return (m.Success && m.Groups[0].Value == str);

            //自然人憑證條碼
            //總長度為16碼字元
            //前兩碼為大寫英文【A - Z】
            //後14碼為數字【0 - 9】
        }

        /// <summary>
        /// 是否為台灣地區手機號碼 (09 開頭 10 位數字。注意!! 不接受 +8869 開頭 13 位數字)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMobile(this string? str)
        {
            return !string.IsNullOrEmpty(str)
                && str.Length == 10
                && str.StartsWith("09")
                && str.IsDigitOnly();
        }

        /// <summary>
        /// 是否為台灣手機郵遞區號 (3~6 位數字)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTwZip(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            string pattern = @"^\d{3,6}$";

            return Regex.IsMatch(str, pattern);
        }

        /// <summary>
        /// 是否為台灣手機號碼(國際格式) (+8869 開頭, + 號後方接 12 位數字, 共 13 個字元)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTwMobile(this string? str)
        {
            return !string.IsNullOrEmpty(str)
                && str.Length == 13
                && str.StartsWith("+8869")
                && str[1..].IsDigitOnly();
        }

        /// <summary>
        /// 是否為香港地區手機號碼 (+852 開頭, + 號後方接 11 位數字, 共 12 個字元)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsHkMobile(this string? str)
        {
            return !string.IsNullOrEmpty(str)
                && str.Length == 12
                && str.StartsWith("+852")
                && str[1..].IsDigitOnly();
        }

        /// <summary>
        /// 是否為新加坡區手機號碼 (+65 開頭, + 號後方接 10 位數字, 共 11 個字元)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSgMobile(this string? str)
        {
            return !string.IsNullOrEmpty(str)
                && str.Length == 11
                && str.StartsWith("+852")
                && str[1..].IsDigitOnly();
        }

        /// <summary>
        /// 全型改半型
        /// 去除所有的空白
        /// 把開頭的 09 改為 +8869
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string? InternationalMobileNormoalize(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            str = str.FullWidthToHalfWidth().Trim().Replace(" ", "");

            if (str.StartsWith("09") && str.Length > 2)
            {
                return "+8869" + str[2..];
            }

            return str;
        }

        /// <summary>
        /// 是否為台灣地區身份証號碼
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTwIdNo(this string idNo)
        {
            var d = false;
            if (idNo.Length == 10)
            {
                idNo = idNo.ToUpper();
                if (idNo[0] >= 0x41 && idNo[0] <= 0x5A)
                {
                    var a = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
                    var b = new int[11];
                    b[1] = a[(idNo[0]) - 65] % 10;
                    var c = b[0] = a[(idNo[0]) - 65] / 10;
                    for (var i = 1; i <= 9; i++)
                    {
                        b[i + 1] = idNo[i] - 48;
                        c += b[i] * (10 - i);
                    }
                    if (((c % 10) + b[10]) % 10 == 0)
                    {
                        d = true;
                    }
                }
            }
            return d;
        }

        /// <summary>
        /// 把開頭的 +8869, +886 9, +88609 改為 09
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string? TwMobileNormalize(this string? str)
        {
            if (str == null)
            {
                return null;
            }

            str = str.Trim().Replace(" ", "").Replace("-", "");

            if (str.StartsWith("+8869"))
            {
                return str.Replace("+8869", "09"); //正規化
            }

            if (str.StartsWith("+88609"))
            {
                return str.Replace("+88609", "09"); //正規化
            }

            return str;
        }

        /// <summary>
        /// 是否為國際電話號碼 (+ 開頭, 最少 8 個字元)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsGlobalMobile(this string? str)
        {
            if (str == null)
            {
                return false;
            }

            //包括国家代码，瑞典的最小长度为9位，以色列为11位，所罗门群岛为8位。
            if (str.Length >= 8 && str.StartsWith("+") && str.Substring(1).IsDigitOnly())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// String.IsNullOrWhiteSpace(str)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string? str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// !string.IsNullOrWhiteSpace(str)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(this string? str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 都轉成小寫再比對
        /// </summary>
        /// <param name="str"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        public static bool ContainsLower(this string str, string test)
        {
            return str.ToLower().Contains(test.ToLower());
        }

        /// <summary>
        /// 會去除 - 和空白. 若不符合格式, 會回傳 null
        /// </summary>
        /// <param name="str"></param>
        /// <param name="IsAddZero"></param>
        /// <returns></returns>
        public static string ToMobile(this string str, bool IsAddZero = true)
        {
            var T = str.Replace("-", "").Replace(" ", "");

            //WU.DebugWriteLine("T: " + T);

            if (IsAddZero && T.StartsWith("9"))
            {
                T = "0" + T;
            }

            //WU.DebugWriteLine("T: " + T);

            if (T.Length != 10 || !T.StartsWith("09") || !T.IsDigitOnly())
            {
                //WU.DebugWriteLine("T is null");
                return null;
            }
            return T;
        }

        /// <summary>
        /// 1. 全形轉半型
        /// 2. 移除所有空白(前後空白和文字中間的空白)
        /// 3. 若是有逗號或分號，則取第一個逗號或分號之前的字串
        /// 4. 移除 "/", 因為 insider 會自動把 "/" 拿掉
        /// 5. 全部轉小寫
        /// 6. IsCheckEmail = true時，若非合法的 email, 則回傳 null
        /// </summary>
        /// <param name="str"></param>
        /// <param name="IsCheckEmail"></param>
        /// <returns></returns>
        public static string? EmailNormalize(this string? str, bool IsCheckEmail = false)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            str = str.FullWidthToHalfWidth()
                .Replace(" ", "")
                .Replace("/", "")
                .Trim()
                .Split(',')[0]
                .Split(';')[0]
                .ToLower();

            if (IsCheckEmail) return str.IsEmail() ? str : null;
            return str;

        }

        /// <summary>
        /// 若是台灣的手機號碼，一率正規化為 09xxxxxxxx
        /// 其它國家手機保持 +xxxxxxx
        /// 
        /// 1. 全形轉半型
        /// 2. 移除所有空白(前後空白和文字中間的空白)
        /// 3. 若是有逗號或分號，則取第一個逗號或分號之前的字串
        /// 4. 第一個字元為 + 或數字，第二個之後的字元只允許數字，不符合者皆改填空白。
        /// 5. +8869 改為 09
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string? PhoneNormalize(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            str = str.FullWidthToHalfWidth()
                .Replace(" ", "")
                .Trim()
                .Split(',')[0]
                .Split(';')[0];

            if (str.StartsWith("+8869"))
            {
                str = str.Replace("+8869", "09");
            }

            if (str.IsDigitOnly() || (str.StartsWith("+") && str.Substring(1).IsDigitOnly()))
            {
                return str;
            }

            return null;
        }

        /// <summary>
        /// 檢查易打錯的domain
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBadDomain(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var badDomains = @"gmail.com.tw;gmail.co;gmai.com;gmial.com;gamil.com;gmail.tw;gnail.com;gamil.com.tw;gmaii.com;.cpm;.vom;.vom.tw;.con.tw;.cpm.tw;.cm.tw;.con;.te;.rw;yshoo.com;yhaoo.com;yshoo.com.tw;yhaoo.com.tw;hitmail.com;hotmil.com;homail.com;hotmai.com".Split(';');
            return badDomains.Any(x => str.EndsWith(x));
        }

        /// <summary>
        /// 檢查Email合法，請先normalize再使用此功能
        /// 新註冊帳號時要檢查Domain (IsCheckDomain = true)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="IsCheckDomain">檢查是不是打錯domain</param>
        /// <returns></returns>
        public static bool IsEmail(this string? str, bool IsCheckDomain = true)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }                

            if (str != str.EmailNormalize())
            {
                return false;
            }

            if (!TextFns.IsEmail(str)) return false;

            if (IsCheckDomain)
            {
                string emailtest = str.ToLower();
                // 20240828 增加檢查domain寫錯
                if (emailtest.IsBadDomain())
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsDate(this string? str, bool IsDateOnly = false)
        {
            return !string.IsNullOrEmpty(str) && (DateTime.TryParse(str, out DateTime retDate) && ((!IsDateOnly) || retDate == retDate.Date));
        }

        /// <summary>
        /// 只有年月日的日期時間字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDay(this string? str)
        {
            return str.IsDate(IsDateOnly: true);
        }

        /// <summary>
        /// 可包含時分秒的日期時間字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDateTime(this string? str)
        {
            return str.IsDate(IsDateOnly: false);
        }

        /// <summary>
        /// 是否為 24 小時制的時間 00:00 ~ 23:59:59.9999999  
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsTime24(this string str)
        {
            return !string.IsNullOrWhiteSpace(str) && !str.StartsWith("24:") && ("2000-01-01 " + str).IsDate();
        }

        /// <summary>
        /// 全形轉半形(英文及標點, 全形空白)
        /// </summary>
        /// <param name="fullWidth"></param>
        /// <returns></returns>
        public static string FullWidthToHalfWidth(this string fullWidth)
        {
            string pattern = @"[\uff01-\uff5e]";
            Regex regex = new(pattern);
            string result = regex.Replace(fullWidth, m => ((char)(m.Value[0] - 0xfee0)).ToString());

            // 全形空白
            result = result.Replace("　", " ");

            return result;
        }

        /// <summary>
        /// 注意，空字串和 null 會回傳 0 (defaultValue)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Int32 ToInt32(this string? str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultValue;
            }

            return Int32.Parse(str);
        }

        /// <summary>
        /// 注意，空字串會回傳 0
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Int64 ToInt64(this string str, long defaultValue = 0)
        {
            if (str.Length > 0)
            {
                return Int64.Parse(str);
            }
            else
            {
                return defaultValue;
            }
        }

        public static string? LowerFirstCharacter(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToLower();
            }
            else
            {
                return str[0..1].ToLower() + str[1..];
            }
        }

        public static string UpperFirstCharacter(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            else
            {
                return str[0..1].ToUpper() + str[1..];
            }
        }

        /// <summary>
        /// 檢查是否所有檔名都是安全字元。
        /// 檔名中不可包含 ..
        /// </summary>
        /// <param name="str"></param>
        /// <param name="validFilenameCharacters"></param>
        /// <returns></returns>
        public static string SafeFilename(this string str, string validFilenameCharacters = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\-_$.@:/ ")
        {
            return Su.TextFns.GetValidFilename(str, validFilenameCharacters);
        }

        public static decimal ToDecimal(this string str, decimal defaultValue = 0)
        {
            if (str.Length > 0)
            {
                return decimal.Parse(str);
            }
            else
            {
                return defaultValue;
            }
        }

        public static float ToFloat(this string str, float defaultValue = 0)
        {
            if (str.Length > 0)
            {
                return float.Parse(str);
            }
            else
            {
                return defaultValue;
            }
        }

        public static double ToDouble(this string str, double defaultValue = 0)
        {
            if (str.Length > 0)
            {
                return double.Parse(str);
            }
            else
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 用逗號分隔的字元, otherStr 若傳入空字串 或 null, 則回傳 false.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="otherStr"></param>
        public static bool ContainsWord(this string str, string otherStr)
        {
            if (otherStr == null || otherStr == "")
            {
                return false;
            }

            return ("," + str + ",").Contains("," + otherStr + ",");
        }

        /// <summary>
        /// 把字串附加在前面，中間放一個分隔字串 (確認後置字串不是 null，但不確定前置字串是否為 null 時使用。)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="otherStr"></param>
        /// <param name="concater">分隔字串</param>
        /// <returns></returns>
        public static string Prepend(this string str, string otherStr, string concater)
        {
            if (string.IsNullOrEmpty(otherStr))
            {
                return str;
            }

            if (string.IsNullOrEmpty(str))
            {
                return otherStr;
            }

            return otherStr + concater + str;
        }

        /// <summary>
        /// 把字串附加在後面，中間放一個分隔字串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="otherStr"></param>
        /// <param name="concater">分隔字串</param>
        /// <returns></returns>
        public static string? Attach(this string? str, string? otherStr, string concater)
        {
            if (string.IsNullOrEmpty(otherStr))
            {
                return str;
            }

            if (string.IsNullOrEmpty(str))
            {
                return otherStr;
            }

            return str + concater + otherStr;
        }

        /// <summary>
        /// 請改用 Attach
        /// </summary>
        /// <param name="str"></param>
        /// <param name="otherStr"></param>
        /// <param name="Concactor"></param>
        /// <returns></returns>
        public static string? Concact(this string? str, string? otherStr, string Concactor)
        {
            return str.Attach(otherStr, Concactor);
        }

        public static string MsSqlObj(this string str, bool isOderBy = false)
        {
            //string nolock = " (nolock)";

            var res = str.Trim();
            var postfix = "";

            if (isOderBy && res.ToLower().EndsWith(" desc"))
            {
                postfix = " desc";
                res = res[0..^5];
            }
            //else if (str.ToLower().EndsWith(nolock))
            //{
            //    postfix = nolock;
            //    res = res.Replace(nolock, "");
            //}

            if (res.EndsWith(" +="))
            {
                res = res.Replace(" +=", "");
            }
            return ("[" + res.Replace("[", "").Replace("]", "") + "]").Replace(".", "].[") + postfix;
        }

        /// <summary>
        /// PostgreSQL 的 Column Name
        /// 不允許 ", 會自動被刪除
        /// 回傳 sample, "Username" 或是 "Order"."Address"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isOderBy">是否為 Order By 裡面的欄位</param>
        /// <returns></returns>
        public static string PgSqlColumnName(this string str, bool isOderBy = false, bool isReplaceDot = true)
        {
            str = str.Trim().Replace("\"", "");

            if (str == "*")
            {
                return str;
            }

            var postfix = "";

            if (isOderBy)
            {
                if (str.ToLower().EndsWith(" desc"))
                {
                    postfix = " desc";
                    str = str[0..^5];
                }

                if (str.ToLower().EndsWith(" asc"))
                {
                    str = str[0..^4];
                }
            }

            if (str.EndsWith(" +="))
            {
                str = str.Replace(" +=", "");
            }

            if (isReplaceDot)
            {
                str = str.Replace(".", "\".\"");
            }

            return $"\"{str}\"{postfix}";
        }

        /// <summary>
        /// 多個欄位; 
        /// ex: a, order.b, c ==> "a", "order"."b", "c"
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isOderBy"></param>
        /// <returns></returns>
        public static string PgSqlColumnNameList(this string str, bool isOderBy = false)
        {
            var columns = str.Split(',');

            for (var i = 0; i < columns.Length; i++)
            {
                var c = columns[i].Trim();

                if (c == "*")
                {
                    continue;
                }

                //已經是欄位格式，拆掉 ", 再重建一次
                if (c.StartsWith("\"") && c.EndsWith("\""))
                {
                    c = c.PgSqlColumnName(isOderBy);
                }

                if (c.Contains(' ') || c.Contains('('))
                {
                    //其它有可能是 function 或 select 指令
                    //檢查 sql injection
                    PgSql.CheckDangerSQL(c);
                }
                else
                {
                    //轉成標準的格式
                    c = c.PgSqlColumnName(isOderBy);
                }

                columns[i] = c;
            }

            return columns.ToOneString(", ");
        }

        /// <summary>
        /// PostgreSQL 的 Table Name, 必需加上 schema name, ex: "public"."Order"
        /// 不允許 ", 會自動被刪除
        /// 若是有 "." 則不會加上預設的 schema 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isOderBy"></param>
        /// <returns></returns>
        public static string PgSqlTableName(this string str, string schema = "public")
        {
            var res = str.Trim().Replace("\"", "");

            if (res.Contains("."))
            {
                return $"\"{str.Replace(".", "\".\"")}\"";
            }
            else
            {
                return $"\"{schema}\".\"{str}\"";
            }
        }

        /// <summary>
        /// 只會把第一個字元變小寫
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CamelCase(this string str)
        {
            return str[0..1].ToLower() + str[1..];
        }

        /// <summary>
        /// 去除 "-" 和 "_", 改為大駝峰，ex: "ab_cde-fgh" 轉換為 "AbCdeFgh"，原字串中的大寫不會做修改
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string PascalCase(this string str)
        {
            return str.Replace('-', '_').Split('_')
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.UpperFirstCharacter())
                .ToOneString("");
        }

        /// <summary>
        /// 把第一個字變大寫 同 UpperFirstCharacter
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UpcaseFirstCharacter(this string str)
        {
            return str.UpperFirstCharacter();

            //if (string.IsNullOrEmpty( str))
            //{
            //    return str;
            //}

            //return str[0..1].ToUpper() + str[1..];
        }

        /// <summary>
        /// 產編 (PD00) 正規化 (全大寫, 前後無空白)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Pd00Normalize(this string str)
        {
            return str.Trim().ToUpper();
        }

        public static System.TimeOnly ToTimeOnly(this string str)
        {
            return TimeOnly.Parse(str);
        }

        ///// <summary>
        ///// 字串結尾有 Z 就直接轉，並且不會加 8 小時
        ///// 若沒結尾沒有 Z, 則轉為日期後再加 8 小時
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static System.DateTime? ToUtcDateTime(this string? str, int addHours = 8)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return null;
        //    }

        //    if (str.EndsWith("Z"))
        //    {
        //        return DateTime.Parse(str);
        //    }
        //    else
        //    {
        //        return DateTime.Parse(str).AddHours(addHours);
        //    }
        //}

        /// <summary>
        /// 各種字串的 To Datetime
        /// yyyyMMdd
        /// YYYYMMddHHmmss
        /// YYYYMMddHHmmssfff
        /// YYYYMMddHHmm
        /// 其它可用 System.DateTime.Parse(str) 轉換的字串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static System.DateTime ToDate(this string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception("can't convert empty strin to datetime.");
            }

            if (str.IsDigitOnly())
            {
                if (str.Length == 8)
                {
                    return System.DateTime.Parse(str[..4] + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2));
                }

                if (str.Length == 14)
                {
                    return System.DateTime.Parse(str[..4] + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2) + ":" + str.Substring(12, 2));
                }

                if (str.Length == 17)
                {
                    return System.DateTime.Parse(str[..4] + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2) + ":" + str.Substring(12, 2) + "." + str.Substring(14, 3));
                }

                if (str.Length == 12)
                {
                    return System.DateTime.Parse(str[..4] + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2));
                }
            }

            return System.DateTime.Parse(str);
        }
    }

    public static class DateTimeExtension
    {
        /// <summary>
        /// 只有日期的比較 (沒有時分秒), 結束時間會加一天
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool IsBetweenDate(this DateTime dt, DateTime startDate, DateTime endDate)
        {
            return (startDate <= dt && endDate.AddDays(1) > dt);
        }


        /// <summary>
        /// yyyy-MM-ddTHH:mm:ss, 注意, 預設會加 ''
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="IsAddQuotation"></param>
        /// <returns></returns>
        public static string SqlDate(this DateTime dt, bool IsAddQuotation = true)
        {
            if (IsAddQuotation)
            {
                return "'" + dt.ISO8601() + "'";
            }
            else
            {
                return dt.ISO8601();
            }
        }

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string YyyyMMddHHmmss(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// yyyy-MM-dd HH:mm
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string YyyyMMddHHmm(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm");
        }


        /// <summary>
        /// yyyy-MM-ddTHH:mm:ss.fff
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ISO8601(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }

        /// <summary>
        /// 回傳 yyyy-MM-ddTHH:mm:00
        /// 注意，最後會補 00
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string IsoYmdHmForMsSql(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm") + ":00";
        }

        /// <summary>
        /// 回傳 yyyy-MM-ddTHH:mm:ss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string IsoYmdHms(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        /// <summary>
        /// 回傳 yyyy-MM-ddT00:00:00Z, 注意，不會扣 8 小時, (只有年月日)
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string? IsoDayZ(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return ((DateTime)dt).Date.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
        }

        /// <summary>
        /// 回傳 yyyy-MM-ddTHH:mm:ssZ, 注意，不會扣 8 小時,
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string? IsoZ(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return ((DateTime)dt).ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
        }

        /// <summary>
        /// 台灣時間轉 Utc 時間(扣 8 小時)，回傳 yyyy-MM-ddTHH:mm:ssZ,
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string? TwToUtcZ(this DateTime? dt)
        {
            if (dt == null)
            {
                return null;
            }

            return ((DateTime)dt).TwToUtcZ();
        }

        /// <summary>
        /// 台灣時間轉 Utc 時間(扣 8 小時)，回傳 yyyy-MM-ddTHH:mm:ssZ,
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string TwToUtcZ(this DateTime dt)
        {
            return ((DateTime)dt).AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
        }

        /// <summary>
        /// 台灣時間轉 Utc 時間(扣 8 小時), 扣 1970-01-01, 取秒數.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int TwToUtcTimeStamp(this DateTime twDateTime)
        {
            return Convert.ToInt32(twDateTime.AddHours(-8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        ///// <summary>
        ///// 台灣時間轉 Utc 時間(扣 8 小時)，回傳 yyyy-MM-ddTHH:mm:ss+00:00,
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <returns></returns>
        //public static string? TwToUtcPlus(this DateTime? dt)
        //{
        //    if (dt == null)
        //    {
        //        return null;
        //    }

        //    return ((DateTime)dt).AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss") + "+00:00";
        //}

        ///// <summary>
        ///// 台灣時間轉 Utc 時間(扣 8 小時)，回傳 yyyy-MM-ddTHH:mm:ss+00:00
        ///// </summary>
        ///// <param name="dt"></param>
        ///// <returns></returns>
        //public static string TwToUtcPlus(this DateTime dt)
        //{
        //    return ((DateTime)dt).AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss") + "+00:00";
        //}

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ISOYMD(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// yyyy-MM-dd 或預設的回傳值(null)
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="nullReturn">dt 為 null 時，預設的回傳值(null)</param>
        /// <returns></returns>
        public static string? ISOYMD(this DateTime? dt, string? nullReturn = null)
        {
            return dt == null ? nullReturn : Convert.ToDateTime(dt).ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 110年01月02日
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string Tw年月日(this DateTime dt)
        {
            return (dt.Year - 1911) + "年" + dt.ToString("MM月dd日");
        }

        /// <summary>
        /// 110/01/02
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string TwYmd(this DateTime dt)
        {
            return (dt.Year - 1911) + "/" + dt.ToString("MM/dd");
        }

        /// <summary>
        /// 110.01.02
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string TwDotYmd(this DateTime dt)
        {
            return (dt.Year - 1911) + "." + dt.ToString("MM.dd");
        }

        /// <summary>
        /// 日~六
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isAddPrefix"></param>
        /// <returns></returns>
        public static string TwWeekDay(this DateTime dt, bool isAddPrefix = false)
        {
            string prefix = isAddPrefix ? "星期" : "";
            switch ((int)(dt.DayOfWeek))
            {
                case 0:
                    return prefix + "日";
                case 1:
                    return prefix + "一";
                case 2:
                    return prefix + "二";
                case 3:
                    return prefix + "三";
                case 4:
                    return prefix + "四";
                case 5:
                    return prefix + "五";
                case 6:
                    return prefix + "六";
            }

            return "";
        }

        /// <summary>
        /// yyyyMMddHHmmssfff, 共 17 碼
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string Ymdhmsf(this DateTime dt, string postfix = "")
        {
            return dt.ToString("yyyyMMddHHmmssfff") + postfix;
        }

        /// <summary>
        /// HHmmssfff
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string Hmsf(this DateTime dt, string postfix = "")
        {
            return dt.ToString("HHmmssfff") + postfix;
        }

        /// <summary>
        /// HHmmss
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string Hms(this DateTime dt, string postfix = "")
        {
            return dt.ToString("HHmmss") + postfix;
        }

        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string Ymdhms(this DateTime dt, string postfix = "")
        {
            return dt.ToString("yyyyMMddHHmmss") + postfix;
        }

        /// <summary>
        /// yyyyMMdd
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="postfix"></param>
        /// <returns></returns>
        public static string Ymd(this DateTime dt, string postfix = "")
        {
            return dt.ToString("yyyyMMdd") + postfix;
        }
    }

    public static class DataRowExtension
    {
        public static T CopyTo<T>(this DataRow row, string onlyFields = null)
        {
            var t = (T)Activator.CreateInstance(typeof(T));

            ObjUtil.CopyFromDataRow<T>(t, row, onlyFields);

            return t;
        }

        public static Dictionary<string, object?> ToDictionary(this DataRow row)
        {
            var dic = new Dictionary<string, object?>();

            var dt = row.Table;
            foreach (DataColumn col in dt.Columns)
            {
                dic.Add(col.ColumnName, Convert.IsDBNull(row[col.ColumnName]) ? null : row[col.ColumnName]);
            }

            return dic;
        }

        public static T Field<T>(this DataRow row, MsSql.ColumnName column)
        {
            return row.Field<T>(column.ToString()[1..^1]);
        }
    }

    public static class DataTableExtension
    {
        /// <summary>
        /// 把欄位名稱改為 Pascal(大駝峰) 的格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <param name="onlyFields"></param>
        /// <returns></returns>
        public static DataTable PascalColumnName(this DataTable dt)
        {
            foreach (DataColumn cn in dt.Columns)
            {
                cn.ColumnName = cn.ColumnName.PascalCase();
            }

            return dt;
        }

        public static List<T> GetList<T>(this DataTable dt, string onlyFields = null)
        {
            return dt.AsEnumerable().Select(r => r.CopyTo<T>(onlyFields)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fields">可填多欄位, 用逗號分隔</param>
        /// <returns></returns>
        public static DataTable CRLFtoBR(this DataTable dt, string fields)
        {
            foreach (string f in fields.Split(','))
            {
                foreach (DataRow oRow in dt.Rows)
                {
                    oRow[f] = oRow[f].DBNullToDefault().CRLFtoBR();
                }
            }

            return dt;
        }

        public static DataTable SetOrdinalAndName(this DataColumn column, int ordinal, string name)
        {
            if (column == null)
            {
                return null;
            }

            column.SetOrdinal(ordinal);
            column.ColumnName = name;

            return column.Table;
        }

        public static DataTable RemoveAdditionalColumns(this DataTable dt, int index)
        {
            while (dt.Columns.Count > index)
            {
                dt.Columns.RemoveAt(index);
            }

            return dt;
        }

        /// <summary>
        /// 檢查是否所有欄位都存在
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static void CheckColumns(this DataTable dt, string columns)
        {
            var columnList = columns.ToLower().Split(',')
                .Select(c => c.Trim())
                .ToList();

            var dbColumns = new List<string>();
            foreach(DataColumn c  in dt.Columns)
            {
                dbColumns.Add(c.ColumnName.ToLower().Trim());
            }

            var missColumns = columnList.Where(c => !dbColumns.Contains(c)).ToList();
            if(missColumns.Count > 0)
            {
                throw new CustomException($"少了以下欄位: {missColumns.ToOneString(",")}");
            }
        }

        public static DataTable RandomSort(this DataTable dt, Random oRG = null)
        {
            if (oRG == null)
            {
                oRG = new Random();
            }

            return dt.AsEnumerable()
                    .OrderBy(r => oRG.Next())
                    .ToList()
                    .CopyToDataTable();
        }

        /// <summary>
        /// IsEndByDay 若未指定, 很容易發生錯誤而未發覺, 所以讓它必填. StartFieldName, EndFieldName 若是錯誤, 應該很快就會發現.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="IsEndByDay"></param>
        /// <param name="StartFieldName"></param>
        /// <param name="EndFieldName"></param>
        /// <returns></returns>
        public static DataTable FilterByDate(this DataTable dt, bool IsEndByDay, string StartFieldName = "StartDate", string EndFieldName = "EndDate")
        {
            return dt.FilterByDate(StartFieldName, EndFieldName, IsEndByDay);
        }

        /// <summary>
        /// 沒有符合資料時, 會回傳空的 DT
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="StartFieldName">通常是 Start</param>
        /// <param name="EndFieldName">通常是 EndDate</param>
        /// <param name="IsEndByDay">是否以日期為準, 不要比對時分秒</param>
        /// <returns></returns>
        public static DataTable FilterByDate(this DataTable dt, string StartFieldName, string EndFieldName, bool IsEndByDay)
        {
            if (dt.Rows.Count > 0)
            {
                var Now = DateTime.Now;
                var E = Now;
                if (IsEndByDay)
                {
                    //以日期為準, 不要比對時分秒
                    E = Now.Date;
                }
                var Q = dt.AsEnumerable().Where(r => r.Field<DateTime>(StartFieldName) <= Now && r.Field<DateTime>(EndFieldName) >= E);

                if (Q.Count() > 0)
                {
                    return Q.ToList().CopyToDataTable();
                }
                else
                {
                    return dt = dt.Clone();
                }
            }
            else
            {
                return dt;
            }
        }

        /// <summary>
        /// Cache Expire 的日期
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="StartFieldName"></param>
        /// <param name="EndFieldName"></param>
        /// <param name="IsEndByDay">結束日期是否以天為單位</param>
        /// <returns></returns>
        public static DateTime CacheExpireDate(this DataTable dt, string StartFieldName, string EndFieldName, bool IsEndByDay)
        {
            var MinS = DateTime.MaxValue;
            var MinE = DateTime.MaxValue;

            if (dt.Rows.Count > 0)
            {
                var Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(StartFieldName)).Where(D => D > DateTime.Now);
                if (Q.Count() > 0)
                {
                    MinS = Q.OrderBy(D => D).First();
                }

                // 找出下一個活動的結束時間
                if (IsEndByDay)
                {
                    //若是活動為當天結束, 它的結束時間應該是隔天的 00:00:00
                    Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(EndFieldName)).Where(D => D >= DateTime.Now.Date);
                }
                else
                {
                    Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(EndFieldName)).Where(D => D > DateTime.Now);
                }

                if (Q.Count() > 0)
                {
                    MinE = Q.OrderBy(D => D).First();

                    if (IsEndByDay)
                    {
                        //暫存時間要以 MinE + 1 天為準(若是活動為當天結束, 它的結束時間應該是隔天的 00:00:00)
                        MinE = MinE.Date.AddDays(1);
                    }
                }
            }

            if (MinS < MinE)
            {
                return MinS;
            }

            return MinE;
        }
    }

    public static class Int32Extension
    {
        public static decimal ToDecimal(this int str)
        {
            return decimal.Parse(str.ToString());
        }
        public static float ToFloat(this int str)
        {
            return float.Parse(str.ToString());
        }
        public static double ToDouble(this int str)
        {
            return double.Parse(str.ToString());
        }
    }

    public static class DecimalExtension
    {
        public static double ToDouble(this decimal str)
        {
            return double.Parse(str.ToString());
        }
        //去除 decimal ToString() 夾帶的尾數零
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
        //去除 decimal ToString() 夾帶的尾數零
        public static string ToStringG29(this decimal value)
        {
            return value.ToString("G29");
        }
    }

    public static class ControllerExtension
    {
        public static ContentResult RedirectMessageContent(this Controller controller, string msg, string nextURL = "/", int delaySeconds = 5)
        {
            return Su.Wu.RedirectMessageContent(controller, msg, nextURL, delaySeconds);
        }
    }
}


//using Newtonsoft.Json.Converters;
//using System.Collections.Specialized;
//using System.Data;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Xml;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Dynamic;
//using System.Diagnostics;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;

//namespace Su
//{
//    public static class IQueryableExtensions
//    {
//        public static IQueryable<TTarget> SelectTo<TSource, TTarget>(this IQueryable<TSource> source) where TTarget : class, new()
//        {
//            var sourceType = typeof(TSource);
//            var targetType = typeof(TTarget);

//            // 获取源类型和目标类型的属性
//            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
//            var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

//            // 创建参数表达式
//            var parameter = Expression.Parameter(sourceType, "x");

//            // 创建绑定列表，只包含同名且类型相同的属性
//            var bindings = targetProperties
//                .Where(tp => sourceProperties.Any(sp => sp.Name == tp.Name))
//                .Select(tp =>
//                {
//                    var sourceProperty = sourceProperties.First(sp => sp.Name == tp.Name);

//                    Expression sourcePropertyExpression = Expression.Property(parameter, sourceProperty);

//                    // Handle nullability differences
//                    if (sourceProperty.PropertyType != tp.PropertyType)
//                    {
//                        sourcePropertyExpression = Expression.Convert(sourcePropertyExpression, tp.PropertyType);
//                    }

//                    return Expression.Bind(tp, sourcePropertyExpression);
//                })
//                .ToList();

//            // 检查是否有绑定
//            if (!bindings.Any())
//            {
//                throw new InvalidOperationException("No matching properties found between source and target types.");
//            }

//            // 创建成员初始化表达式
//            var body = Expression.MemberInit(Expression.New(targetType), bindings);

//            // 创建选择表达式
//            var selector = Expression.Lambda<Func<TSource, TTarget>>(body, parameter);

//            //Debug.WriteLine($"selector: {selector}");

//            // 返回查询
//            return source.Provider.CreateQuery<TTarget>(
//                Expression.Call(
//                    typeof(Queryable),
//                    "Select",
//                    new Type[] { sourceType, targetType },
//                    source.Expression,
//                    Expression.Quote(selector)
//                )
//            );
//        }
//    }

//    public static class ExceptionExtension
//    {
//        public static string FullInfo(this Exception ex)
//        {
//            var info = ex.Message;

//            info = info.Attach(ex.StackTrace, "; StackTrace: ");

//            if(ex.InnerException != null)
//            {
//                info = info.Attach(ex.InnerException.Message, "; InnerException:");

//                info = info.Attach(ex.InnerException.StackTrace, "; InnerException StackTrace: ");
//            }

//            return info ?? "No Exception Message.";
//        }
//    }

//    public static class FileUploadExtion
//    {
//        public static string GetText(this IFormFile fu)
//        {
//            if (fu.Length > 0)
//            {
//                StreamReader reader = new StreamReader(fu.OpenReadStream());
//                return reader.ReadToEnd();
//            }

//            return "";
//        }

//        public static bool IsValidExt(this IFormFile fu, string validExts = "jpg,gif,png,jpeg,xls,xlsx,svg")
//        {
//            if (fu == null || fu.Length <= 0)
//            {
//                return true;
//            }

//            if (("," + validExts.Replace(" ", "").ToLower() + ",").Contains("," + fu.Ext().ToLower() + ","))
//            {
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// 回傳值不包含 ".", 會轉小寫
//        /// </summary>
//        /// <param name="fu"></param>
//        /// <returns></returns>
//        public static string Ext(this IFormFile fu)
//        {
//            return Su.TextFns.FileExt(fu.FileName.ToLower());
//        }

//        public static void SaveAs(this IFormFile fu, string fullFileName)
//        {
//            using (Stream fileStream = new FileStream(fullFileName, FileMode.Create))
//            {
//                fu.CopyTo(fileStream);
//            }
//        }

//        public static string SaveWithDate(this IFormFile fu, string diretory, string prefix = "", bool isCreateDirectory = true)
//        {
//            System.Threading.Thread.Sleep(5); //避免連續叫用, 檔名重覆.
//            string filename = prefix + System.DateTime.Now.Ymdhmsf() + "." + fu.Ext();
//            System.IO.Directory.CreateDirectory(diretory);
//            using (Stream fileStream = new FileStream(Path.Combine(diretory, filename), FileMode.Create))
//            {
//                fu.CopyTo(fileStream);
//            }

//            return filename;
//        }
//    }

//    public static class ExpressionExtension
//    {
//        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
//        {
//            var combineE = Expression.AndAlso(e1.Body, Expression.Invoke(e2, e1.Parameters[0]));

//            return Expression.Lambda<Func<T, bool>>(combineE, e1.Parameters);
//        }

//        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
//        {
//            var combineE = Expression.OrElse(e1.Body, Expression.Invoke(e2, e1.Parameters[0]));

//            return Expression.Lambda<Func<T, bool>>(combineE, e1.Parameters);
//        }

//        public static Expression<Func<T, bool>> OrElseAll<T>(this IEnumerable<Expression<Func<T, bool>>> exps)
//        {
//            if (exps.Count() == 1)
//            {
//                return exps.First();
//            }

//            var e0 = exps.First();

//            var orExp = exps.Skip(1).Aggregate(e0.Body, (x, y) => Expression.OrElse(x, Expression.Invoke(y, e0.Parameters[0])));

//            return Expression.Lambda<Func<T, bool>>(orExp, e0.Parameters);
//        }

//        public static Expression<Func<T, bool>> AndAlsoAll<T>(this IEnumerable<Expression<Func<T, bool>>> exps)
//        {
//            if (exps.Count() == 1)
//            {
//                return exps.First();
//            }

//            var e0 = exps.First();

//            var orExp = exps.Skip(1).Aggregate(e0.Body, (x, y) => Expression.AndAlso(x, Expression.Invoke(y, e0.Parameters[0])));

//            return Expression.Lambda<Func<T, bool>>(orExp, e0.Parameters);
//        }
//    }

//    public static class ObjectExtension
//    {
//        public static DataTable ReOrderColumn(this DataTable dt, params (string oldColumnName, string newColumnNmae)[] columns)
//        {
//            for(int i = 0; i < columns.Length; i++)
//            {
//                dt.Columns[columns[i].oldColumnName].SetOrdinal(i);
//                dt.Columns[columns[i].oldColumnName].ColumnName = columns[i].newColumnNmae;
//            }

//            while(dt.Columns.Count > columns.Length)
//            {
//                dt.Columns.RemoveAt(dt.Columns.Count - 1);
//            }

//            return dt;
//        }

//        public static DataTable ToDataTable<T>(this List<T> items, string skips = null)
//        {
//            return ((IEnumerable<T>)items).ToDataTable(skips);
//        }

//        //public static DataTable ToDataTable<T>(this List<T> items, string skips = null)
//        //{
//        //    DataTable dataTable = new DataTable(typeof(T).Name);
//        //    //Get all the properties
//        //    PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
//        //    foreach (PropertyInfo prop in Props)
//        //    {
//        //        //Setting column names as Property names
//        //        dataTable.Columns.Add(prop.Name);
//        //    }
//        //    foreach (T item in items)
//        //    {
//        //        var values = new object[Props.Length];
//        //        for (int i = 0; i < Props.Length; i++)
//        //        {
//        //            //inserting property values to datatable rows
//        //            values[i] = Props[i].GetValue(item, null);
//        //        }
//        //        dataTable.Rows.Add(values);
//        //    }
//        //    //put a breakpoint here and check datatable
//        //    return dataTable;
//        //}

//        public static DataTable ToDataTable<T>(this IEnumerable<T>? items, string skips = null)
//        {
//            DataTable dataTable = new DataTable(typeof(T).Name);

//            //Get all the properties
//            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
//            foreach (PropertyInfo prop in Props)
//            {
//                //Setting column names as Property names
//                dataTable.Columns.Add(prop.Name);
//            }

//            if(items == null)
//            {
//                return dataTable;
//            }

//            foreach (T item in items)
//            {
//                var values = new object[Props.Length];
//                for (int i = 0; i < Props.Length; i++)
//                {
//                    //inserting property values to datatable rows
//                    values[i] = Props[i].GetValue(item, null);
//                }
//                dataTable.Rows.Add(values);
//            }
//            //put a breakpoint here and check datatable
//            return dataTable;
//        }

//        public static Dictionary<string, object?> ToDictionary<T>(this T src, string? skips = null)
//        {
//            var dic = new Dictionary<string, object?>();

//            ObjUtil.CopyToDictionary(src, dic, skips);

//            return dic;
//        }

//        /// <summary>
//        /// 給 PostFileAsync 使用
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="src"></param>
//        /// <param name="skips"></param>
//        /// <returns></returns>
//        public static Dictionary<string, string?> ToStringDictionary<T>(this T src, string skips = null)
//        {
//            return ObjUtil.CopyPropertiesToStringDictionary(src, skips);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="dest"></param>
//        /// <param name="propertyName">不分大小寫</param>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        /// <exception cref="Exception"></exception>
//        public static T SetProperty<T>(this T dest, string propertyName, object value)
//        {
//            var destProps = dest.GetType().GetProperties()
//                    .Where(x => x.CanWrite)
//                    .ToList();

//            if (destProps.Any(x => x.Name.ToLower() == propertyName.ToLower()))
//            {
//                var destItem = destProps.First(x => x.Name.ToLower() == propertyName.ToLower());
//                if (destItem.CanWrite)
//                { // check if the property can be set or no.
//                    try
//                    {
//                        destItem.SetValue(dest, value, null);
//                    }
//                    catch (Exception ex)
//                    {
//                        throw new Exception("can't find property: " + propertyName + ", " + ex.FullInfo());
//                    }
//                }
//            }

//            return dest;
//        }

//        /// <summary>
//        /// 會檢查 src 是否為 null, 同時呼叫 CopyPropertiesTo 和 CopyFieldsTo
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="dest"></param>
//        /// <param name="src"></param>
//        /// <param name="skips"></param>
//        /// <returns></returns>
//        public static T CopyFrom<T>(this T dest, object src, string skips = null)
//        {
//            if(src == null)
//            {
//                return dest;
//            }
//            ObjUtil.CopyTo(src, dest, skips);
//            return dest;
//        }

//        /// <summary>
//        /// 這個版本的 Copy To, 會觸發 dest 物件的每個 property 的 set 動作, 很奇怪..
//        /// 所以不能用在 dto 物件 copy to ORM 物件使用.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="src"></param>
//        /// <param name="skips"></param>
//        /// <returns></returns>
//        public static T CopyTo<T>(this object src, string? skips = null, object? additionalProperties = null)
//        {
//            T dest = (T)Activator.CreateInstance(typeof(T))!;
//            return ObjUtil.CopyTo(src, dest, skips, additionalProperties);
//        }

//        public static T CopyTo<T>(this object src, T dest, string? skips = null, object? additionalProperties = null)
//        {
//            return ObjUtil.CopyTo(src, dest, skips, additionalProperties);
//        }

//        /// <summary>
//        ///  預設會接 Response.end, 自已本身不會 Response.Write
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="IsEnd"></param>
//        /// <param name="DateFormat"></param>
//        public static void WriteJSON(this object value, bool IsEnd = true, string DateFormat = null)
//        {
//            Su.Wu.WriteJSON(value, IsEnd, DateFormat);
//        }

//        /// <summary>
//        /// 盡量不要在迴圈中使用. 可改用原生的 GetProperty(propertyName).GetValue(src, null);
//        /// Dynamic 物件不適用.
//        /// </summary>
//        /// <param name="src"></param>
//        /// <param name="propertyName"></param>
//        /// <returns></returns>
//        public static object? GetPropertyValue(this object src, string propertyName)
//        {
//            var p = src.GetType().GetProperty(propertyName);
//            if (p == null)
//            {
//                return null;
//            }
//            return p.GetValue(src, null);
//        }

//        public static object? GetFieldValue(this object src, string propertyName)
//        {
//            var p = src.GetType().GetField(propertyName);
//            if (p == null)
//            {
//                return null;
//            }
//            return p.GetValue(src);
//        }

//        /// <summary>
//        /// 會先試 field 再試 property
//        /// </summary>
//        /// <param name="src"></param>
//        /// <param name="propertyName"></param>
//        /// <returns></returns>
//        public static object? GetValue(this object src, string propertyName)
//        {
//            //System.Dynamic.ExpandoObject
//            if (src.GetType().ToString() == "System.Dynamic.ExpandoObject")
//            {
//                IDictionary<string, object> dic = (IDictionary<string, object>)src;
//                if (dic.ContainsKey(propertyName))
//                {
//                    return dic[propertyName];
//                }
//                else
//                {
//                    return null;
//                }
//            }

//            return src.GetFieldValue(propertyName) ?? src.GetPropertyValue(propertyName);
//        }

//        /// <summary>
//        /// 若是 ExpandoObject 會先轉 json 字串, 再轉物件.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="src"></param>
//        /// <param name="propertyName"></param>
//        /// <returns></returns>
//        public static T? GetValue<T>(this object src, string propertyName)
//        {
//            //System.Dynamic.ExpandoObject
//            if (src.GetType().ToString() == "System.Dynamic.ExpandoObject")
//            {
//                IDictionary<string, object> dic = (IDictionary<string, object>)src;
//                return dic.TryGetValue(propertyName, out object? value).JsonCopyTo<T>();
//            }

//            var res = src.GetFieldValue(propertyName);
//            if (res != null)
//            {
//                return (T)res;
//            }

//            return (T?)src.GetPropertyValue(propertyName);
//        }

//        public static bool IsDBNull(this object obj)
//        {
//            return Convert.IsDBNull(obj);
//        }

//        public static string DBNullToDefault(this object value, string defaultValue = "", string dateTimeFormate = "yyyy-MM-dd HH:mm:ss")
//        {
//            if (Convert.IsDBNull(value) || value == null)
//            {
//                return defaultValue;
//            }
//            else
//            {
//                if (value.GetType().ToString() == "System.DateTime")
//                {
//                    return ((System.DateTime)value).ToString(dateTimeFormate);
//                }

//                return value.ToString() ?? "";
//            }
//        }

//        //public static string Json(this object value, string DateFormat = null)
//        //{
//        //    if (DateFormat == null)
//        //    {
//        //        return Newtonsoft.Json.JsonConvert.SerializeObject(value);
//        //    }
//        //    else
//        //    {
//        //        return Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = DateFormat });
//        //    }
//        //}

//        /// <summary>
//        /// 要注意, 沒有 Response.End
//        /// </summary>
//        /// <param name="value"></param>
//        public static void WriteJSON(this object value)
//        {
//            Wu.WriteJSON(value);
//        }

//        public static Microsoft.AspNetCore.Mvc.OkObjectResult SuccessJsonResultData(this object data)
//        {
//            return new Microsoft.AspNetCore.Mvc.OkObjectResult(data.SuccessDataJson());
//            //return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = true, data });
//        }

//        //public static string DBNullToDefault(this Object value, string Default = "")
//        //{
//        //    return (Convert.IsDBNull(value) || value == null) ? Default : value.ToString();
//        //}

//        public static Dictionary<string, object?> CopyToDictionary(this Object value)
//        {
//            var dic = new Dictionary<string, object?>();
//            Su.ObjUtil.CopyToDictionary(value, dic);

//            return dic;
//        }

//        public static T? JsonCopyTo<T>(this Object? src)
//        {
//            if(src == null)
//            {
//                return default;
//            }

//            return src.Json().JsonDeserialize<T>();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="dateFormat"></param>
//        /// <param name="isIndented"></param>
//        /// <param name="isCamelCase">使用 Su.CamelCaseContractResolver</param>
//        /// <returns></returns>
//        public static string Json(this Object value, string dateFormat = null, bool isIndented = false, bool isCamelCase = false)
//        {
//            var setting = new Newtonsoft.Json.JsonSerializerSettings();

//            if(dateFormat != null)
//            {
//                setting.DateFormatString = dateFormat;
//            }

//            if (isIndented)
//            {
//                setting.Formatting = Newtonsoft.Json.Formatting.Indented;
//            }

//            if (isCamelCase)
//            {
//                //DefaultContractResolver contractResolver = new DefaultContractResolver
//                //{
//                //    NamingStrategy = new Su.CamelCaseContractResolver();
//                //};

//                //setting.ContractResolver = contractResolver;
//                setting.ContractResolver = new Su.CamelCaseContractResolver();
//            }

//            return Newtonsoft.Json.JsonConvert.SerializeObject(value, setting);
//        }

//        public static string SuccessDataJson(this Object data, string DateFormat = null)
//        {
//            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, data }, Newtonsoft.Json.Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = DateFormat });
//        }
//    }

//    public static partial class StringExtension
//    {
//        public static bool IsYN(this char c)
//        {
//            return c == 'Y' || c == 'N';
//        }

//        /// <summary>
//        /// 組成要執行的 SQL
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
//        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
//        /// <param name="isCheckDangerSQL"></param>
//        /// <param name="isRemoveCrLf"></param>
//        /// <returns></returns>
//        public static string ToMsSql(this string sql, object parameters, object sqlObjects = null, bool isCheckDangerSQL = true, bool isRemoveCrLf = true)
//        {
//            //有傳入 parameters 了, 預設可以把 CRLF 拿掉, 變數應該會在 parameters 之中。
//            if (isRemoveCrLf)
//            {
//                sql = sql.Replace("\r", " ").Replace("\n", " ");
//            }

//            if (isCheckDangerSQL)
//            {
//                MsSql.CheckDangerSQL(sql);
//            }

//            var sourceProperties = parameters.GetType().GetProperties().ToList();

//            foreach (var srcItem in sourceProperties)
//            {
//                sql = sql.Replace("{" + srcItem.Name + "}", srcItem.GetValue(parameters).MsSqlValue());
//            }

//            if (sqlObjects != null)
//            {
//                sourceProperties = sqlObjects.GetType().GetProperties().ToList();

//                foreach (var srcItem in sourceProperties)
//                {
//                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlObjects).ToString().MsSqlObj());
//                }
//            }

//            return sql;
//        }

//        /// <summary>
//        /// 組成要執行的 SQL
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
//        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
//        /// <param name="isCheckDangerSQL"></param>
//        /// <param name="isRemoveCrLf"></param>
//        /// <returns></returns>
//        public static string ToPgSql(this string sql, object parameters, object sqlColumns = null, object sqlTables = null, bool isCheckDangerSQL = true, bool isRemoveCrLf = true)
//        {
//            //有傳入 parameters 了, 預設可以把 CRLF 拿掉, 變數應該會在 parameters 之中。
//            if (isRemoveCrLf)
//            {
//                sql = sql.Replace("\r", " ").Replace("\n", " ");
//            }

//            if (isCheckDangerSQL)
//            {
//                PgSql.CheckDangerSQL(sql);
//            }

//            var sourceProperties = parameters.GetType().GetProperties().ToList();

//            foreach (var srcItem in sourceProperties)
//            {
//                sql = sql.Replace("{" + srcItem.Name + "}", srcItem.GetValue(parameters).PgSqlValue());
//            }

//            if (sqlColumns != null)
//            {
//                sourceProperties = sqlColumns.GetType().GetProperties().ToList();

//                foreach (var srcItem in sourceProperties)
//                {
//                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlColumns).ToString().PgSqlColumnName());
//                }
//            }

//            if (sqlTables != null)
//            {
//                sourceProperties = sqlTables.GetType().GetProperties().ToList();

//                foreach (var srcItem in sourceProperties)
//                {
//                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlTables).ToString().PgSqlTableName());
//                }
//            }

//            return sql;
//        }

//        /// <summary>
//        /// 使用 Converter 看來沒有明顯的時間差異, 跑 10000000 測試, 時間比在 1.052 ~ 0.902 之間
//        /// </summary>
//        public class Converter
//        {
//            public string src;
//            public Converter(string src)
//            {
//                this.src = src;
//            }

//            public DateTime ToDate()
//            {
//                return System.DateTime.Parse(src);
//            }
//        }

//        public class ImageClass
//        {
//            public string filename;
//            public ImageClass(string filename)
//            {
//                this.filename = filename;
//            }

//            ///// <summary>
//            ///// 第一個參數 為舊的檔名
//            ///// </summary>
//            ///// <param name="NewFile"></param>
//            ///// <param name="W"></param>
//            ///// <param name="H"></param>
//            ///// <param name="Quality"></param>
//            ///// <param name="Interpolation"></param>
//            //public void ResizeImage(string NewFile, int W = -1, int H = -1, int Quality = 80, int Interpolation = 5, double Threshold = 0.35)
//            //{
//            //    //因為比對過原圖日期了, 所以一率刪檔重建
//            //    if (System.IO.File.Exists(NewFile))
//            //    {
//            //        //UW.WU.DebugWriteLine("Delete " + NewFile, false, true);
//            //        System.IO.File.Delete(NewFile);
//            //    }





//            //    var objJpeg = new ASPJPEGLib.ASPJpeg();
//            //    objJpeg.Open(filename);

//            //    if (((double)new System.IO.FileInfo(filename).Length / (objJpeg.Width * objJpeg.Height)) > Threshold)
//            //    {


//            //        if (W != -1 && W > 0) { objJpeg.Width = W; }
//            //        if (H != -1 && H > 0) { objJpeg.Height = H; }

//            //        objJpeg.Quality = Quality;
//            //        objJpeg.Interpolation = Interpolation;
//            //        objJpeg.Progressive = 1;
//            //    }
//            //    objJpeg.Save(NewFile);
//            //    objJpeg.Close();
//            //}
//        }


//        public static ImageClass Image(this string value)
//        {
//            return new ImageClass(value);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="parameters">值</param>
//        /// <param name="sqlObjects">欄位</param>
//        /// <param name="isCheckInection"></param>
//        /// <returns></returns>
//        public static MsSql.Criteria ToSqlCriteria(this string sql, object parameters, object sqlObjects = null, bool isCheckInection = true)
//        {
//            return MsSql.Criteria.GetSinglCriteria(sql, parameters, sqlObjects, isCheckInection);
//        }

//        public static Converter C(this string value)
//        {
//            return new Converter(value);
//        }

//        //public static MsSql.MsSqlBuilder MsSqlBuilder(this string value)
//        //{
//        //    return new MsSql.MsSqlBuilder(value);
//        //}

//        public static IOClass IO(this string value)
//        {
//            return new IOClass(value);
//        }

//        public class IOClass
//        {
//            string _src = "";

//            public IOClass(string src)
//            {
//                this._src = src;
//            }

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="path"></param>
//            /// <param name="enc">預設是 UTF8</param>
//            public void WriteToFile(string path, Encoding enc = null)
//            {
//                if (enc == null)
//                {
//                    enc = System.Text.Encoding.UTF8;
//                }
//                System.IO.File.WriteAllText(path, _src, enc);
//            }
//        }

//        public const string vbCrLf = "\r\n";

//        public static string CRLFtoBR(this string value)
//        {
//            if(value == null)
//            {
//                return null;
//            }
//            return value
//                    .Replace("\r\n", "<br>")
//                    .Replace("\n", "<br>")
//                    .Replace("\r", "<br>");
//        }

//        //public static void DebugWriteLine(this string value, bool IsHtmlEncode = false, bool IsStop = false, bool IsFlush = false, bool IsAddTimeFlag = false)
//        //{
//        //    WU.DebugWriteLine(value, IsHtmlEncode, IsStop, IsFlush, IsAddTimeFlag);
//        //}

//        public static string SuccessMessageJson(this string msg)
//        {
//            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, msg });
//        }

//        public static string ErrorMessageJson(this string msg)
//        {
//            return Newtonsoft.Json.JsonConvert.SerializeObject(new { success = false, msg });
//        }

//        //public static object SuccessObject(this string msg)
//        //{
//        //    return new { success = true, msg };
//        //}

//        public static Microsoft.AspNetCore.Mvc.JsonResult SuccessJsonResultMessage(this string msg)
//        {
//            return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = true, msg });
//        }

//        public static Microsoft.AspNetCore.Mvc.JsonResult ErrorJsonResultMessage(this string msg)
//        {
//            return new Microsoft.AspNetCore.Mvc.JsonResult(new { success = false, msg });
//        }

//        public static object ErrorObject(this string msg)
//        {
//            return new { success = false, msg };
//        }

//        public static string GotoJs(this string returnURL, bool IsTop = false)
//        {
//            string top = "";
//            if (IsTop)
//            {
//                top = ".top";
//            }

//            return "<script>window" + top + ".location.href='" + returnURL + "';\r\n</script>";
//        }



//        /// <summary>
//        /// window alert, 有加 script tag
//        /// </summary>
//        /// <param name="Message"></param>
//        /// <param name="IsBack"></param>
//        /// <returns></returns>
//        public static string ShowMessageJs(this string Message, bool IsBack = false)
//        {
//            string Back = "";
//            if (IsBack)
//            {
//                Back = "window.history.back();\r\n";
//            }

//            string JS = @"<script>
//    window.alert('" + Message.JavaScriptString() + @"');
//    " + Back + @"
//</script>";

//            return JS;
//        }

//        public static string JavaScriptString(this string value)
//        {
//            return System.Web.HttpUtility.JavaScriptStringEncode(value);
//        }

//        public static T JsonDeserialize<T>(this string value)
//        {
//            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
//        }

//        public static string GetRemotePage(this string value, Encoding oE = null)
//        {
//            if (oE == null)
//            {
//                oE = Encoding.UTF8;
//            }

//            return Wu.GetRemotePage(value, oE);
//        }

//        public static string Post(this string url, NameValueCollection nvc, Encoding oE = null)
//        {
//            return Wu.Post(url, nvc, oE);
//        }

//        public static string EscapeDataString(this string value)
//        {
//            return System.Uri.EscapeDataString(value);
//        }

//        /// <summary>
//        /// 建議改用 EscapeDataString, 避免空白變加號.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="oE"></param>
//        /// <returns></returns>
//        public static string UrlEncode(this string value, Encoding oE = null)
//        {
//            if (oE == null)
//            {
//                oE = Encoding.UTF8;
//            }

//            return System.Web.HttpUtility.UrlEncode(value, oE);
//        }

//        public static string HtmlEncode(this string value)
//        {
//            return System.Web.HttpUtility.HtmlEncode(value);
//        }

//        /// <summary>
//        /// 主要用於 response.writefile 時的檔名.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string UrlPathEncode(this string value)
//        {
//            return System.Web.HttpUtility.UrlPathEncode(value);
//        }


//        /// <summary>
//        /// XMLEncode
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string XMLEncode(this string value)
//        {
//            return System.Security.SecurityElement.Escape(value);
//        }


//        /// <summary>
//        /// 拿掉 !ENTITY, 避免 XXE 攻擊
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string SecureXML(this string value)
//        {
//            return value.Replace("<!ENTITY", "<_!ENTITY");
//        }

//        public static XmlDocument SecureXMLDoc(this string value)
//        {
//            XmlDocument doc = new XmlDocument();
//            doc.LoadXml(value.SecureXML());
//            return doc;
//        }

//        /// <summary>
//        /// 把 ` 刪除, 再用 `` 包住字串, 把 "." 換成 "`.`"
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="isOderBy"></param>
//        /// <returns></returns>
//        public static string MySqlObj(this string value, bool isOderBy = false)
//        {
//            var res = value.Trim();
//            var postfix = "";

//            if (isOderBy && res.ToLower().EndsWith(" desc"))
//            {
//                postfix = " desc";
//                res = res[0..^5];
//            }

//            if (res.EndsWith(" +="))
//            {
//                res = res.Replace(" +=", "");
//            }

//            return "`" + res.Replace("`", "").Replace(".", "`.`") + "`" + postfix;
//        }

//        public static string PgSqlValue(this object value)
//        {
//            if (value == null)
//            {
//                return " null ";
//            }

//            switch (value.GetType().ToString())
//            {
//                case "System.Char":
//                case "System.String":
//                    return "'" + value.ToString().Replace("'", "''") + "'";
//                case "System.Int32":
//                    return ((int)value).ToString();
//                case "System.Int64":
//                    return ((Int64)value).ToString();
//                case "System.Decimal":
//                    return ((decimal)value).ToString();
//                case "System.DateTime":
//                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.ffffff") + "'";
//                case "System.Single":
//                    return ((Single)value).ToString();
//                case "System.Double":
//                    return ((Double)value).ToString();
//                default:
//                    throw new Exception("不認識的型別: " + value.GetType().ToString());
//            }
//        }

//        public static string PgSqlValue(this object value, bool IsNotUnicode = false)
//        {
//            if (value == null)
//            {
//                return " null ";
//            }

//            switch (value.GetType().ToString())
//            {
//                case "System.Char":
//                case "System.String":
//                    return "'" + value.ToString().Replace("'", "''") + "'";
//                case "System.Int32":
//                    return ((int)value).ToString();
//                case "System.Int64":
//                    return ((Int64)value).ToString();
//                case "System.Decimal":
//                    return ((decimal)value).ToString();
//                case "System.DateTime":
//                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";
//                case "System.Single":
//                    return ((Single)value).ToString();
//                case "System.Double":
//                    return ((Double)value).ToString();
//                default:
//                    throw new Exception("不認識的型別: " + value.GetType().ToString());
//            }
//        }

//        public static string MsSqlValue(this object value, bool IsNotUnicode = false)
//        {
//            if (value == null)
//            {
//                return " null ";
//            }

//            switch (value.GetType().ToString())
//            {
//                case "System.Char":
//                case "System.String":
//                    if (IsNotUnicode)
//                    {
//                        return "'" + value.ToString().Replace("'", "''") + "'";
//                    }
//                    else
//                    {
//                        return "N'" + value.ToString().Replace("'", "''") + "'";
//                    }
//                case "System.Int32":
//                    return ((int)value).ToString();
//                case "System.Int64":
//                    return ((Int64)value).ToString();
//                case "System.Decimal":
//                    return ((decimal)value).ToString();
//                case "System.DateTime":
//                    return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";
//                case "System.Single":
//                    return ((Single)value).ToString();
//                case "System.Double":
//                    return ((Double)value).ToString();
//                default:
//                    if (value is Enum)
//                    {
//                        return ((int)value).ToString();
//                    }
//                    throw new Exception("不認識的型別: " + value.GetType().ToString());
//            }
//        }

//        public static string ToOneString(this IEnumerable<string> list, string separator)
//        {
//            return string.Join(separator, list);
//        }

//        /// <summary>
//        /// 會保持 url1 和 url2 之間有一個 /
//        /// </summary>
//        /// <param name="url1"></param>
//        /// <param name="url2"></param>
//        /// <returns></returns>
//        public static string CombineUrl(this string url1, string url2)
//        {
//            if (url1.EndsWith("/"))
//            {
//                if (url2.StartsWith("/"))
//                {
//                    return url1 + url2.Substring(1);
//                }
//                return url1 + url2;
//            }
//            else
//            {
//                if (url2.StartsWith("/"))
//                {
//                    return url1 + url2;
//                }
//                return url1 + "/" + url2;
//            }
//        }

//        /// <summary>
//        /// 使用 System.IO.Path.Combine(firstPath, path) 來合併 Path，後方的 Paths 會被移除前置的 / 或 \ ，並經由 System.IO.Path.IsPathRooted 檢查是否為完整目錄
//        /// </summary>
//        /// <param name="firstPath"></param>
//        /// <param name="paths"></param>
//        /// <returns></returns>
//        /// <exception cref="Exception"></exception>
//        public static string AddPath(this string firstPath, params string[] paths)
//        {
//            //使用變數時，第一個參數有可能是 null
//            if (string.IsNullOrEmpty(firstPath))
//            {
//                throw new Exception("AddPath 的第一個參數不可為 null");
//            }

//            string finalPath = firstPath;
//            foreach (var path in paths)
//            {
//                if (path.Contains(".."))
//                {
//                    throw new Exception("路徑中不可包含 ..");
//                }

//                //後方的 Paths 會被移除前置的 / 或 \ ，並經由 System.IO.Path.IsPathRooted 檢查是否為完整目錄
//                var subPath = path;
//                while (subPath.StartsWith("/") || subPath.StartsWith(@"\"))
//                {
//                    subPath = subPath[1..];
//                }

//                if (System.IO.Path.IsPathRooted(subPath))
//                {
//                    throw new Exception("不可併入完整路徑 ..");
//                }

//                finalPath = System.IO.Path.Combine(finalPath, subPath);
//            }

//            if (!finalPath.StartsWith(firstPath))
//            {
//                throw new Exception("合併路徑發生問題 ..");
//            }

//            return finalPath;
//        }

//        public const string SafeUrlCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-.";

//        /// <summary>
//        /// 取得安全的區塊
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="paths"></param>
//        /// <returns></returns>
//        public static string GetSafeUrlSegment(string originalSegment)
//        {
//            var res = "";
//            for(int i = 0; i < originalSegment.Length; i++)
//            {
//                var index = SafeUrlCharacters.IndexOf(originalSegment[i]);

//                if(index < 0)
//                {
//                    throw new Exception($"網址中不可使用字元 '{originalSegment[i]}'");
//                }

//                res += SafeUrlCharacters[index];
//            }

//            return res;
//        }

//        /// <summary>
//        /// 只開放特定字元的路徑
//        /// A-Za-z0-9 "_", "-"
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="paths">不可用 / 指定下一階的目錄</param>
//        /// <returns></returns>
//        public static string AddSafeUrl(this string url, params string[] paths)
//        {
//            foreach (var path in paths)
//            {
//                if (path.Contains(".."))
//                {
//                    throw new Exception("路徑中不可包含 ..");
//                }

//                if (string.IsNullOrEmpty(path))
//                {
//                    continue;
//                }

//                var safeSegment = GetSafeUrlSegment(path);

//                if (url.EndsWith("/"))
//                {
//                    url += path;
//                }
//                else
//                {
//                    url += "/" + path;
//                }
//            }
//            return url;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="paths"></param>
//        /// <returns></returns>
//        public static string AddUrl (this string url, params string[] paths)
//        {
//            foreach (var path in paths)
//            {
//                if (path.Contains(".."))
//                {
//                    throw new Exception("路徑中不可包含 ..");
//                }

//                if (string.IsNullOrEmpty(path))
//                {
//                    continue;
//                }

//                if(url.EndsWith("/") && path.StartsWith("/"))
//                {
//                    if(path.Length > 1)
//                    {
//                        url += path.Substring(1);
//                    }
//                }
//                else if(url.EndsWith("/") || path.StartsWith("/"))
//                {
//                    url += path;
//                }
//                else
//                {
//                    url += "/" + path;
//                }
//            }
//            return url;
//        }

//        /// <summary>
//        /// 會把 parameterValue 做 EscapeDataString 之後再併到 Query String 之中。
//        /// </summary>
//        /// <param name="url"></param>
//        /// <param name="parameterName"></param>
//        /// <param name="parameterValue"></param>
//        /// <returns></returns>
//        public static string AddQueryString(this string url, string parameterName, string parameterValue)
//        {
//            if(parameterValue == null)
//            {
//                return url;
//            }

//            url = url.AddQueryString(parameterName + "=" + parameterValue.EscapeDataString());

//            return url;
//        }

//        public static string AddQueryString(this string url, object parameters)
//        {
//            var sourceProps = parameters.GetType().GetProperties().Where(x => x.CanRead).ToList();

//            foreach (var srcItem in sourceProps)
//            {
//                url = url.AddQueryString(srcItem.Name + "=" + srcItem.GetValue(parameters, null)?.ToString().EscapeDataString());
//            }

//            return url;
//        }

//        public static string AddQueryString(this string url, string queryString)
//        {
//            if (!String.IsNullOrWhiteSpace(queryString))
//            {
//                if (queryString[0] == '&' || queryString[0] == '?')
//                {
//                    queryString = queryString.Substring(1);
//                }

//                if (url.IndexOf("?") > 0)
//                {
//                    queryString = "&" + queryString;
//                }
//                else
//                {
//                    queryString = "?" + queryString;
//                }
//            }
//            else
//            {
//                queryString = "";
//            }

//            return url + queryString;
//        }

//        //public static string DecryptStringFromBytes_AesCBC(this string cipherText, string Key, String IV)
//        //{
//        //    return Crypto.DecryptStringFromBytes_AesCBC(cipherText, Key, IV);
//        //}


//        /// <summary>
//        /// 直接傳入的 SQL, 目前檢查是否有 "--", ";", "\r", "\n"
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static bool IsMsSqlInjection(this string value)
//        {
//            return (value.Contains("--") || value.Contains("/*") || value.Contains(";") || value.Contains("\r") || value.Contains("\n"));
//        }

//        /// <summary>
//        /// 直接傳入的 SQL, 目前檢查是否有 "--", ";", "\r", "\n", "/*"
//        /// 
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static bool IsPgSqlInjection(this string value)
//        {
//            return value.IsMsSqlInjection();
//            //return (value.Contains("--") || value.Contains("/*") || value.Contains(";") || value.Contains("\r") || value.Contains("\n"));
//        }

//        /// <summary>
//        /// 直接傳入的 SQL, 目前檢查是否有 "--", ";", "#", "/*", "\r", "\n"
//        /// # 有點麻煩, 很容易衝突.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static bool IsMySQLInjection(this string value)
//        {
//            return (value.Contains("#") || value.Contains("/*") || value.Contains("--") || value.Contains(";") || value.Contains("\r") || value.Contains("\n"));
//        }

//        /// <summary>
//        /// "N'" + value.Replace("'", "''") + "'";
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string SqlNChar(this string value)
//        {
//            return "N'" + value.Replace("'", "''") + "'";
//        }

//        /// <summary>
//        /// "N'" + value.Replace("'", "''") + "'";
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string SqlNCharAndTrim(this string value)
//        {
//            return "N'" + value.Trim().Replace("'", "''") + "'";
//        }

//        /// <summary>
//        /// 回傳 [XXX], 假設欄位名稱中不會有 [ 或 ] 的符號
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string MsSqlField(this string value)
//        {
//            return "[" + value.Replace("[", "").Replace("]", "") + "]";
//        }

//        /// <summary>
//        /// 假設欄位中不會有 ` 符號
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string MySqlField(this string value)
//        {
//            return "`" + value.Replace("`", "").Replace("`", "") + "`";
//        }

//        public static string RemoveHtml(this string value)
//        {
//            return Regex.Replace(value, "<.*?>", string.Empty);
//        }

//        public static string RemoveHtmlComments(this string value)
//        {
//            return Regex.Replace(value, "<!--(.|\\s)*?-->", string.Empty);
//        }

//        /// <summary>
//        /// 用來比對 Regular Expression.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="pattern"></param>
//        /// <param name="regexOptions"></param>
//        /// <returns></returns>
//        public static bool IsMatch(this string value, string pattern, RegexOptions regexOptions = RegexOptions.None)
//        {
//            Regex r = new Regex(pattern, regexOptions);

//            return r.Match(value).Success;
//        }

//        public static string MaskName(this string value)
//        {

//            return Su.TextFns.GetMaskedName(value);
//        }

//        /// <summary>
//        /// 保留第一個字母和 @ 之後的網域
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string MaskEmail(this string value)
//        {
//            return Su.TextFns.GetMaskedEmail(value);           
//        }

//        public static string MaskAddress(this string? value)
//        {
//            return Su.TextFns.GetMaskedAddress(value);
//        }

//        public static string MaskBirthday(this string value)
//        {
//            return Su.TextFns.GetMaskedBirthday(value);
//        }


//        public static string BrToCr(this string? value)
//        {
//            if (string.IsNullOrEmpty(value))
//            {
//                return value;
//            }

//            return Regex.Replace(
//                Regex.Replace(value,
//                    "(<br />|<br/>|</ br>|</br>|<br>)", "\r"),
//                "(<BR />|<BR/>|</ BR>|</BR>|<BR>)", "\r");
//        }

//        public static string RemoveCrLf(this string value)
//        {
//            return value.Replace("\r", "").Replace("\n", "");
//        }

//        public static string[] SplitCrLf(this string value)
//        {
//            return value.Replace("\r\n", "\r").Replace("\n\r", "\r").Replace("\n", "\r").Split('\r');
//        }

//        /// <summary>
//        /// 逗號切開後, 先 Trim 再拿掉空字串.
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public static string[] SplitCommaAndTrim(this string value)
//        {
//            if (value == null || value == "")
//            {
//                return new string[] { };
//            }

//            var Q = value.Split(',').Select(S => S.Trim()).Where(S => S != "");
//            if (Q.Count() > 0)
//            {
//                return Q.ToArray();
//            }
//            else
//            {
//                return new string[] { };
//            }
//            //return value.Split(',').Select(S => S.Trim()).Where(S => S != "").ToArray();
//        }

//        /// <summary>
//        ///  傳入: AAA,BB,,C,,,Dxx  傳回: 'AAA','BB','C','Dxx'
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static string SqlListStr(this string str)
//        {
//            return TextFns.GetSqlStingList(str);
//        }



//        /// <summary>
//        /// 非整數和空白自動移除. ex: 傳入: ,A,B,1,2,3,4,5,,, 傳出: 1,2,3,4,5
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static string SqlListInt(this string str)
//        {
//            return TextFns.GetIntList(str);
//        }

//        public static bool SmallerThan(this string str, string otherStr)
//        {
//            return (string.Compare(str, otherStr) < 0);
//        }

//        public static bool SmallerThanOrEqualTo(this string str, string otherStr)
//        {
//            return (string.Compare(str, otherStr) <= 0);
//        }

//        public static bool BiggerThan(this string str, string otherStr)
//        {
//            return (string.Compare(str, otherStr) > 0);
//        }

//        public static bool BiggerThanOrEqualTo(this string str, string otherStr)
//        {
//            return (string.Compare(str, otherStr) >= 0);
//        }

//        /// <summary>
//        /// 是否為數字, 包括浮點數.
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsNumeric(this string str)
//        {
//            //return Double.TryParse(Convert.ToString(str), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out double retNum);

//            return TextFns.IsNumeric(str);
//        }

//        /// <summary>
//        /// 是否為整數.
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsInt(this string str)
//        {
//            return TextFns.IsInt(str);
//        }

//        /// <summary>
//        /// 是否為台灣地區統編 8 位數字
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsTaxNo(this string idNo)
//        {
//            if (idNo == null)
//            {
//                return false;
//            }
//            Regex regex = new Regex(@"^\d{8}$");
//            Match match = regex.Match(idNo);
//            if (!match.Success)
//            {
//                return false;
//            }
//            int[] idNoArray = idNo.ToCharArray().Select(c => Convert.ToInt32(c.ToString())).ToArray();
//            int[] weight = new int[] { 1, 2, 1, 2, 1, 2, 4, 1 };

//            int subSum;     //小和
//            int sum = 0;    //總和
//            int sumFor7 = 1;
//            for (int i = 0; i < idNoArray.Length; i++)
//            {
//                subSum = idNoArray[i] * weight[i];
//                sum += (subSum / 10)   //商數
//                     + (subSum % 10);  //餘數                
//            }
//            if (idNoArray[6] == 7)
//            {
//                //若第7碼=7，則會出現兩種數值都算對，因此要特別處理。
//                sumFor7 = sum + 1;
//            }
//            return (sum % 10 == 0) || (sumFor7 % 10 == 0);
//        }

//        /// <summary>
//        /// 是否為捐贈碼
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsDonateCode(this string str)
//        {
//            //https://www.cetustek.com.tw/news.php?id=186
//            //捐贈碼
//            //總長度為3至7碼字元
//            //全部由數字【0 - 9】組成

//            var pattern = @"^[0-9]{3,7}$";

//            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

//            Match m = r.Match(str);

//            return (m.Success && m.Groups[0].Value == str);
//        }

//        /// <summary>
//        /// 是否為行動載具碼
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsMobileCarrierCode(this string str)
//        {
//            //https://www.cetustek.com.tw/news.php?id=186
//            //手機條碼
//            //由Code39組成，總長度為8碼字元
//            //第一碼必為『/』
//            //其餘七碼則由數字【0 - 9】、大寫英文【A - Z】與特殊符號【.】【-】【+】組成

//            var pattern = @"^\/{1}[0-9A-Z\.\-\+]{7}$";

//            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

//            Match m = r.Match(str);

//            return (m.Success && m.Groups[0].Value == str);

//            //自然人憑證條碼
//            //總長度為16碼字元
//            //前兩碼為大寫英文【A - Z】
//            //後14碼為數字【0 - 9】




//        }

//        /// <summary>
//        /// 是否為台灣地區手機號碼 (09 開頭 10 位數字不接受 +8869 開頭 13 位數字)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsMobile(this string str)
//        {
//            if (str.Length == 10 && str.StartsWith("09") && str.IsNumeric())
//            {
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// 是否為台灣地區身份証號碼
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsTwIdNo(this string idNo)
//        {
//            var d = false;
//            if (idNo.Length == 10)
//            {
//                idNo = idNo.ToUpper();
//                if (idNo[0] >= 0x41 && idNo[0] <= 0x5A)
//                {
//                    var a = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33 };
//                    var b = new int[11];
//                    b[1] = a[(idNo[0]) - 65] % 10;
//                    var c = b[0] = a[(idNo[0]) - 65] / 10;
//                    for (var i = 1; i <= 9; i++)
//                    {
//                        b[i + 1] = idNo[i] - 48;
//                        c += b[i] * (10 - i);
//                    }
//                    if (((c % 10) + b[10]) % 10 == 0)
//                    {
//                        d = true;
//                    }
//                }
//            }
//            return d;
//        }

//        /// <summary>
//        /// 把開頭的 +8869 改為 09
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static string? TwMobileNormoalize(this string str)
//        {
//            if( str == null)
//            {
//                return null;
//            }

//            if (str.StartsWith("+8869"))
//            {
//                return str.Replace("+8869", "09"); //正規化
//            }

//            return str;
//        }

//        /// <summary>
//        /// 是否為國際電話號碼 (+ 開頭, 最少 10 位數字)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsGlobalMobile(this string str)
//        {
//            if (str.IsMobile())
//            {
//                return false;
//            }

//            //包括国家代码，瑞典的最小长度为9位，以色列为11位，所罗门群岛为8位。
//            if (str.Length >= 8 && str.StartsWith("+") && str.Substring(1).IsNumeric())
//            {
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// String.IsNullOrWhiteSpace(str)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsEmpty(this string str)
//        {
//            return String.IsNullOrWhiteSpace(str);
//        }

//        /// <summary>
//        /// !String.IsNullOrWhiteSpace(str)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsNotEmpty(this string str)
//        {
//            return !String.IsNullOrWhiteSpace(str);
//        }

//        /// <summary>
//        /// 都轉成小寫再比對
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="test"></param>
//        /// <returns></returns>
//        public static bool ContainsLower(this string str, string test)
//        {
//            return str.ToLower().Contains(test.ToLower());
//        }

//        /// <summary>
//        /// 會去除 - 和空白. 若不符合格式, 會回傳 null
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="IsAddZero"></param>
//        /// <returns></returns>
//        public static string ToMobile(this string str, bool IsAddZero = true)
//        {
//            var T = str.Replace("-", "").Replace(" ", "");

//            //WU.DebugWriteLine("T: " + T);

//            if (IsAddZero && T.StartsWith("9"))
//            {
//                T = "0" + T;
//            }

//            //WU.DebugWriteLine("T: " + T);

//            if (T.Length != 10 || !T.StartsWith("09") || !T.IsNumeric())
//            {
//                //WU.DebugWriteLine("T is null");
//                return null;
//            }
//            return T;
//        }

//        /// <summary>
//        /// 檢查Email合法，請先normalize再使用此功能
//        /// 新註冊帳號時要檢查Domain (IsCheckDomain = true)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="IsCheckDomain">檢查是不是打錯domain</param>
//        /// <returns></returns>
//        public static bool IsEmail(this string? str, bool IsCheckDomain = false)
//        {

//            if (string.IsNullOrEmpty(str)) return false;

//            if (!TextFns.IsEmail(str)) return false;

//            if (IsCheckDomain)
//            {
//                string emailtest = str.ToLower();
//                // 20240828 增加檢查domain寫錯
//                var badDomains = @"gmail.com.tw;gmail.co;gmai.com;gmial.com;gamil.com;gmail.tw;gnail.com;gamil.com.tw;gmaii.com;.cpm;.vom;.vom.tw;.con.tw;.cpm.tw;.cm.tw;.con;.te;.rw;yshoo.com;yhaoo.com;yshoo.com.tw;yhaoo.com.tw;hitmail.com;hotmil.com;homail.com;hotmai.com".Split(';');
//                if (badDomains.Any(x => emailtest.EndsWith(x))) return false;
//            }

//            return true;
//        }

//        /// <summary>
//        /// 1. 全形轉半型
//        /// 2. 移除所有空白(前後空白和文字中間的空白)
//        /// 3. 若是有逗號或分號，則取第一個逗號或分號之前的字串
//        /// 4. 移除 "/", 因為 insider 會自動把 "/" 拿掉
//        /// 5. 全部轉小寫
//        /// 6. IsCheckEmail = true時，若非合法的 email, 則回傳 null
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="IsCheckEmail"></param>
//        /// <returns></returns>
//        public static string? EmailNormalize(this string? str, bool IsCheckEmail = false)
//        {
//            if (string.IsNullOrEmpty(str))
//            {
//                return null;
//            }

//            str = str.FullWidthToHalfWidth()
//                .Replace(" ", "")
//                .Replace("/", "")
//                .Trim()
//                .Split(',')[0]
//                .Split(';')[0]
//                .ToLower();

//            if (IsCheckEmail) return str.IsEmail() ? str : null;
//            return str;

//        }


//        public static bool IsNullOrEmpty(this string? str)
//        {
//            return string.IsNullOrEmpty(str);
//        }

//        /// <summary>
//        /// 6~16 位數, 有英文和數字
//        /// </summary>
//        /// <param name="secret"></param>
//        /// <returns></returns>
//        public static bool IsSecretOk(this string? secret)
//        {
//            //bool result = 
//            //    !string.IsNullOrEmpty(secret) 
//            //    && secret.Length >= 6 
//            //    && secret.Length <= 16
//            //    && Regex.IsMatch(secret.ToUpper(), "[A-Z]")
//            //    && Regex.IsMatch(secret, @"\d");

//            bool result =!string.IsNullOrEmpty(secret)
//                         && secret.Length >= 8  // 至少8個字符長度
//                         && Regex.IsMatch(secret, "[a-z]")  // 至少包含一個小寫英文字母
//                         && Regex.IsMatch(secret, "[A-Z]")  // 至少包含一個大寫英文字母
//                         && Regex.IsMatch(secret, @"\d");   // 至少包含一個數字

//            return result;
//        }

//        public static bool IsDate(this string str, bool IsDateOnly = false)
//        {
//            return (DateTime.TryParse(str, out DateTime retDate) && ((!IsDateOnly) || retDate == retDate.Date));
//        }

//        /// <summary>
//        /// 只會對全大寫或是有 _ 的字串處理
//        /// 把 AAA_BBB_CCC 轉為 AaaBbbCcc
//        /// 或是是 ABCDEF 轉為 Abcdef
//        /// </summary>
//        /// <param name="upcaseStr"></param>
//        /// <returns></returns>
//        public static string UpperCaseToPascal(this string upcaseStr)
//        {
//            if (upcaseStr.Contains('_') || upcaseStr == upcaseStr.ToUpper())
//            {
//                return upcaseStr.ToLower().Split('_')
//               .Select(w => w.UpperFirstCharacter())
//               .ToOneString("");
//            }

//            return upcaseStr;
//        }

//        /// <summary>
//        /// 是否為 24 小時制的時間
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static bool IsTime24(this string str)
//        {
//            return !string.IsNullOrWhiteSpace(str) && ("2000-01-01 " + str).IsDate();
//        }

//        public static Int32 ToInt32(this string str, int defaultValue = 0)
//        {
//            if (str.Length > 0)
//            {
//                return Int32.Parse(str);
//            }
//            else
//            {
//                return defaultValue;
//            }
//        }

//        public static Int64 ToInt64(this string str, int defaultValue = 0)
//        {
//            if (str.Length > 0)
//            {
//                return Int64.Parse(str);
//            }
//            else
//            {
//                return defaultValue;
//            }
//        }

//        //name[0..1].ToLower() + name[1..]
//        public static string LowerFirstCharacter(this string? str)
//        {
//            if(string.IsNullOrEmpty(str))
//            {
//                return str;
//            }

//            if (str.Length == 1)
//            {
//                return str.ToLower();
//            }
//            else
//            {
//                return str[0..1].ToLower() + str[1..];
//            }
//        }

//        public static string UpperFirstCharacter(this string? str)
//        {
//            if (string.IsNullOrEmpty(str))
//            {
//                return str;
//            }

//            if (str.Length == 1)
//            {
//                return str.ToUpper();
//            }
//            else
//            {
//                return str[0..1].ToUpper() + str[1..];
//            }
//        }

//        /// <summary>
//        /// 檢查是否所有檔名都是安全字元。
//        /// 檔名中不可包含 ..
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="validFilenameCharacters"></param>
//        /// <returns></returns>
//        public static string SafeFilename(this string str, string validFilenameCharacters = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\-_$.@:/ ")
//        {
//            return Su.TextFns.GetValidFilename(str, validFilenameCharacters);
//        }

//        public static decimal ToDecimal(this string str, decimal defaultValue = 0)
//        {
//            if (str.Length > 0)
//            {
//                return decimal.Parse(str);
//            }
//            else
//            {
//                return defaultValue;
//            }
//        }

//        public static double ToDouble(this string str, double defaultValue = 0)
//        {
//            if (str.Length > 0)
//            {
//                return double.Parse(str);
//            }
//            else
//            {
//                return defaultValue;
//            }
//        }
//        /// <summary>
//        /// 用逗號分隔的字元, otherStr 若傳入空字串 或 null, 則回傳 false.
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="otherStr"></param>
//        public static bool ContainsWord(this string str, string otherStr)
//        {
//            if (otherStr == null || otherStr == "")
//            {
//                return false;
//            }

//            return ("," + str + ",").Contains("," + otherStr + ",");
//        }

//        /// <summary>
//        /// 把字串附加在前面，中間放一個分隔字串 (確認後置字串不是 null，但不確定前置字串是否為 null 時使用。)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="otherStr"></param>
//        /// <param name="concater">分隔字串</param>
//        /// <returns></returns>
//        public static string Prepend(this string str, string otherStr, string concater)
//        {
//            if (string.IsNullOrEmpty(otherStr))
//            {
//                return str;
//            }

//            if (string.IsNullOrEmpty(str))
//            {
//                return otherStr;
//            }

//            return otherStr + concater + str;
//        }

//        //public static string? Attach(this string str, string? otherStr, string concater)
//        //{
//        //    return "";
//        //}

//        /// <summary>
//        /// 把字串附加在後面，中間放一個分隔字串
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="otherStr"></param>
//        /// <param name="concater">分隔字串</param>
//        /// <returns></returns>
//        public static string? Attach(this string? str, string? otherStr, string concater)
//        {
//            if (string.IsNullOrEmpty(otherStr))
//            {
//                return str;
//            }

//            if (string.IsNullOrEmpty(str))
//            {
//                return otherStr;
//            }

//            return str + concater + otherStr;
//        }

//        ///// <summary>
//        ///// 請改用 Attach
//        ///// </summary>
//        ///// <param name="str"></param>
//        ///// <param name="otherStr"></param>
//        ///// <param name="Concactor"></param>
//        ///// <returns></returns>
//        //public static string? Concact(this string str, string? otherStr, string Concactor)
//        //{
//        //    return str.Attach(otherStr, Concactor);
//        //}

//        public static string MsSqlObj(this string str, bool isOderBy = false)
//        {
//            //string nolock = " (nolock)";

//            var res = str.Trim();
//            var postfix = "";

//            if (isOderBy && res.ToLower().EndsWith(" desc"))
//            {
//                postfix = " desc";
//                res = res[0..^5];
//            }
//            //else if (str.ToLower().EndsWith(nolock))
//            //{
//            //    postfix = nolock;
//            //    res = res.Replace(nolock, "");
//            //}

//            if (res.EndsWith(" +="))
//            {
//                res = res.Replace(" +=", "");
//            }
//            return ("[" + res.Replace("[", "").Replace("]", "") + "]").Replace(".", "].[") + postfix;
//        }

//        /// <summary>
//        /// PostgreSQL 的 Column Name
//        /// 不允許 ", 會自動被刪除
//        /// 回傳 sample, "Username" 或是 "Order"."Address"
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="isOderBy">是否為 Order By 裡面的欄位</param>
//        /// <returns></returns>
//        public static string PgSqlColumnName(this string str, bool isOderBy = false)
//        {
//            str = str.Trim().Replace("\"", "");

//            if(str == "*")
//            {
//                return str;
//            }

//            var postfix = "";

//            if (isOderBy)
//            {
//                if(str.ToLower().EndsWith(" desc"))
//                {
//                    postfix = " desc";
//                    str = str[0..^5];
//                }

//                if (str.ToLower().EndsWith(" asc"))
//                {
//                    str = str[0..^4];
//                }
//            }

//            if (str.EndsWith(" +="))
//            {
//                str = str.Replace(" +=", "");
//            }

//            return $"\"{str.Replace(".", "\".\"")}\"{postfix}";
//        }

//        /// <summary>
//        /// 多個欄位; 
//        /// ex: a, order.b, c ==> "a", "order"."b", "c"
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="isOderBy"></param>
//        /// <returns></returns>
//        public static string PgSqlColumnNameList(this string str, bool isOderBy = false)
//        {
//            var columns = str.Split(',');

//            for(var i = 0; i < columns.Length; i++)
//            {
//                var c = columns[i].Trim();

//                if(c == "*")
//                {
//                    continue;
//                }

//                //已經是欄位格式，拆掉 ", 再重建一次
//                if(c.StartsWith('\"') && c.EndsWith('\"'))
//                {
//                    c = c.PgSqlColumnName(isOderBy);
//                }

//                if(c.Contains(' ') || c.Contains('('))
//                {
//                    //其它有可能是 function 或 select 指令
//                    //檢查 sql injection
//                    PgSql.CheckDangerSQL(c);
//                }
//                else
//                {
//                    //轉成標準的格式
//                    c = c.PgSqlColumnName(isOderBy);
//                }

//                columns[i] = c;
//            }

//            return columns.ToOneString(", ");
//        }

//        /// <summary>
//        /// PostgreSQL 的 Table Name, 必需加上 schema name, ex: "public"."Order"
//        /// 不允許 ", 會自動被刪除
//        /// 若是有 "." 則不會加上預設的 schema 
//        /// </summary>
//        /// <param name="str"></param>
//        /// <param name="isOderBy"></param>
//        /// <returns></returns>
//        public static string PgSqlTableName(this string str, string schema = "public")
//        {
//            var res = str.Trim().Replace("\"", "");

//            if (res.Contains("."))
//            {
//                return $"\"{str.Replace(".", "\".\"")}\"";
//            }
//            else
//            {
//                return $"\"{schema}\".\"{str}\"";
//            }
//        }

//        /// <summary>
//        /// 只會把第一個字元變小寫
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static string CamelCase(this string str)
//        {
//            return str[0..1].ToLower() + str[1..];
//        }

//        /// <summary>
//        /// 產編 (PD00) 正規化 (全大寫, 前後無空白)
//        /// </summary>
//        /// <param name="str"></param>
//        /// <returns></returns>
//        public static string Pd00Normalize(this string str)
//        {
//            return str.Trim().ToUpper();
//        }

//        public static System.TimeOnly ToTimeOnly(this string str)
//        {
//            return TimeOnly.Parse(str);
//        }

//        public static System.DateTime ToDate(this string str)
//        {
//            if (str.IsNumeric())
//            {
//                if (str.Length == 8)
//                {
//                    return System.DateTime.Parse(str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2));
//                }

//                if (str.Length == 14)
//                {
//                    return System.DateTime.Parse(str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2) + ":" + str.Substring(12, 2));
//                }

//                if (str.Length == 17)
//                {
//                    return System.DateTime.Parse(str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2) + ":" + str.Substring(12, 2) + "." + str.Substring(14, 3));
//                }

//                if (str.Length == 12)
//                {
//                    return System.DateTime.Parse(str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + " " + str.Substring(8, 2) + ":" + str.Substring(10, 2));
//                }
//            }
//            return System.DateTime.Parse(str);
//        }
//    }

//    public static class DateTimeExtension
//    {
//        /// <summary>
//        /// 只有日期的比較 (沒有時分秒), 結束時間會加一天
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="startDate"></param>
//        /// <param name="endDate"></param>
//        /// <returns></returns>
//        public static bool IsBetweenDate(this DateTime dt, DateTime startDate, DateTime endDate)
//        {
//            return (startDate <= dt && endDate.AddDays(1) > dt);
//        }


//        /// <summary>
//        /// yyyy-MM-ddTHH:mm:ss, 注意, 預設會加 ''
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="IsAddQuotation"></param>
//        /// <returns></returns>
//        public static string SqlDate(this DateTime dt, bool IsAddQuotation = true)
//        {
//            if (IsAddQuotation)
//            {
//                return "'" + dt.ISO8601() + "'";
//            }
//            else
//            {
//                return dt.ISO8601();
//            }
//        }

//        /// <summary>
//        /// yyyy-MM-dd HH:mm:ss
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string YyyyMMddHHmmss(this DateTime dt)
//        {
//            return dt.ToString("yyyy-MM-dd HH:mm:ss");
//        }

//        /// <summary>
//        /// yyyy-MM-dd HH:mm
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string YyyyMMddHHmm(this DateTime dt)
//        {
//            return dt.ToString("yyyy-MM-dd HH:mm");
//        }


//        /// <summary>
//        /// yyyy-MM-ddTHH:mm:ss.fff
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string ISO8601(this DateTime dt)
//        {
//            return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
//        }

//        /// <summary>
//        /// yyyy-MM-dd
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string ISOYMD(this DateTime dt)
//        {
//            return dt.ToString("yyyy-MM-dd");
//        }

//        /// <summary>
//        /// 110年01月02日
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string Tw年月日(this DateTime dt)
//        {
//            return (dt.Year - 1911) + "年" + dt.ToString("MM月dd日");
//        }

//        /// <summary>
//        /// 110/01/02
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string TwYmd(this DateTime dt)
//        {
//            return (dt.Year - 1911) + "/" + dt.ToString("MM/dd");
//        }

//        /// <summary>
//        /// 110.01.02
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <returns></returns>
//        public static string TwDotYmd(this DateTime dt)
//        {
//            return (dt.Year - 1911) + "." + dt.ToString("MM.dd");
//        }

//        /// <summary>
//        /// 日~六
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="isAddPrefix"></param>
//        /// <returns></returns>
//        public static string TwWeekDay(this DateTime dt, bool isAddPrefix = false)
//        {
//            string prefix = isAddPrefix ? "星期" : "";
//            switch ((int)(dt.DayOfWeek))
//            {
//                case 0:
//                    return prefix + "日";
//                case 1:
//                    return prefix + "一";
//                case 2:
//                    return prefix + "二";
//                case 3:
//                    return prefix + "三";
//                case 4:
//                    return prefix + "四";
//                case 5:
//                    return prefix + "五";
//                case 6:
//                    return prefix + "六";
//            }

//            return "";
//        }

//        /// <summary>
//        /// yyyyMMddHHmmssfff, 共 17 碼
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="postfix"></param>
//        /// <returns></returns>
//        public static string Ymdhmsf(this DateTime dt, string postfix = "")
//        {
//            return dt.ToString("yyyyMMddHHmmssfff") + postfix;
//        }

//        /// <summary>
//        /// HHmmssfff
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="postfix"></param>
//        /// <returns></returns>
//        public static string Hmsf(this DateTime dt, string postfix = "")
//        {
//            return dt.ToString("HHmmssfff") + postfix;
//        }

//        /// <summary>
//        /// HHmmss
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="postfix"></param>
//        /// <returns></returns>
//        public static string Hms(this DateTime dt, string postfix = "")
//        {
//            return dt.ToString("HHmmss") + postfix;
//        }

//        /// <summary>
//        /// yyyyMMddHHmmss
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="postfix"></param>
//        /// <returns></returns>
//        public static string Ymdhms(this DateTime dt, string postfix = "")
//        {
//            return dt.ToString("yyyyMMddHHmmss") + postfix;
//        }

//        /// <summary>
//        /// yyyyMMdd
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="postfix"></param>
//        /// <returns></returns>
//        public static string Ymd(this DateTime dt, string postfix = "")
//        {
//            return dt.ToString("yyyyMMdd") + postfix;
//        }
//    }

//    public static class DataRowExtension
//    {
//        public static T CopyTo<T>(this DataRow row, string onlyFields = null)
//        {
//            var t = (T)Activator.CreateInstance(typeof(T));

//            ObjUtil.CopyFromDataRow<T>(t, row, onlyFields);

//            return t;
//        }

//        public static T Field<T>(this DataRow row, MsSql.ColumnName column)
//        {
//            return row.Field<T>(column.ToString()[1..^1]);
//        }
//    }

//    public static class DataTableExtension
//    {
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="dt"></param>
//        /// <param name="onlyFields"></param>
//        /// <param name="isChangeColumnName">用來把 postgresql 的資料表轉換為物件使用, 會把欄位名稱中的底線移除</param>
//        /// <returns></returns>
//        public static List<T> GetList<T>(this DataTable dt, string onlyFields = null, bool isChangeColumnName = false)
//        {
//            if (isChangeColumnName)
//            {
//                var fieldNames = new List<string>(); 
//                var fields = typeof(T).GetProperties();
//                foreach (var f in fields)
//                {
//                    fieldNames.Add(f.Name);
//                }

//                foreach (DataColumn column in dt.Columns)
//                {
//                    string? fieldName = fieldNames.FirstOrDefault(f => f.ToLower() == column.ColumnName.Replace("_", "").ToLower());
//                    if(fieldName != null)
//                    {
//                        column.ColumnName = fieldName;
//                    }
//                }
//            }

//            return dt.AsEnumerable().Select(r => r.CopyTo<T>(onlyFields)).ToList();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="fields">可填多欄位, 用逗號分隔</param>
//        /// <returns></returns>
//        public static DataTable CRLFtoBR(this DataTable dt, string fields)
//        {
//            foreach(string f in fields.Split(','))
//            {
//                foreach (DataRow oRow in dt.Rows)
//                {
//                    oRow[f] = oRow[f].DBNullToDefault().CRLFtoBR();
//                }
//            }

//            return dt;
//        }

//        public static DataTable SetOrdinalAndName(this DataColumn column, int ordinal, string name)
//        {
//            if(column == null)
//            {
//                return null;
//            }

//            column.SetOrdinal(ordinal);
//            column.ColumnName = name;

//            return column.Table;        
//        }

//        public static DataTable RemoveAdditionalColumns(this DataTable dt, int index)
//        {
//            while(dt.Columns.Count > index)
//            {
//                dt.Columns.RemoveAt(index);
//            }

//            return dt;
//        }

//        public static DataTable RandomSort(this DataTable dt, Random oRG = null)
//        {
//            if (oRG == null)
//            {
//                oRG = new Random();
//            }

//            return dt.AsEnumerable()
//                    .OrderBy(r => oRG.Next())
//                    .ToList()
//                    .CopyToDataTable();
//        }

//        /// <summary>
//        /// IsEndByDay 若未指定, 很容易發生錯誤而未發覺, 所以讓它必填. StartFieldName, EndFieldName 若是錯誤, 應該很快就會發現.
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="IsEndByDay"></param>
//        /// <param name="StartFieldName"></param>
//        /// <param name="EndFieldName"></param>
//        /// <returns></returns>
//        public static DataTable FilterByDate(this DataTable dt, bool IsEndByDay, string StartFieldName = "StartDate", string EndFieldName = "EndDate")
//        {
//            return dt.FilterByDate(StartFieldName, EndFieldName, IsEndByDay);
//        }

//        /// <summary>
//        /// 沒有符合資料時, 會回傳空的 DT
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="StartFieldName">通常是 Start</param>
//        /// <param name="EndFieldName">通常是 EndDate</param>
//        /// <param name="IsEndByDay">是否以日期為準, 不要比對時分秒</param>
//        /// <returns></returns>
//        public static DataTable FilterByDate(this DataTable dt, string StartFieldName, string EndFieldName, bool IsEndByDay)
//        {
//            if (dt.Rows.Count > 0)
//            {
//                var Now = DateTime.Now;
//                var E = Now;
//                if (IsEndByDay)
//                {
//                    //以日期為準, 不要比對時分秒
//                    E = Now.Date;
//                }
//                var Q = dt.AsEnumerable().Where(r => r.Field<DateTime>(StartFieldName) <= Now && r.Field<DateTime>(EndFieldName) >= E);

//                if (Q.Count() > 0)
//                {
//                    return Q.ToList().CopyToDataTable();
//                }
//                else
//                {
//                    return dt = dt.Clone();
//                }
//            }
//            else
//            {
//                return dt;
//            }
//        }

//        /// <summary>
//        /// Cache Expire 的日期
//        /// </summary>
//        /// <param name="dt"></param>
//        /// <param name="StartFieldName"></param>
//        /// <param name="EndFieldName"></param>
//        /// <param name="IsEndByDay">結束日期是否以天為單位</param>
//        /// <returns></returns>
//        public static DateTime CacheExpireDate(this DataTable dt, string StartFieldName, string EndFieldName, bool IsEndByDay)
//        {
//            var MinS = DateTime.MaxValue;
//            var MinE = DateTime.MaxValue;

//            if (dt.Rows.Count > 0)
//            {
//                var Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(StartFieldName)).Where(D => D > DateTime.Now);
//                if (Q.Count() > 0)
//                {
//                    MinS = Q.OrderBy(D => D).First();
//                }

//                // 找出下一個活動的結束時間
//                if (IsEndByDay)
//                {
//                    //若是活動為當天結束, 它的結束時間應該是隔天的 00:00:00
//                    Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(EndFieldName)).Where(D => D >= DateTime.Now.Date);
//                }
//                else
//                {
//                    Q = dt.AsEnumerable().Select(r => r.Field<DateTime>(EndFieldName)).Where(D => D > DateTime.Now);
//                }

//                if (Q.Count() > 0)
//                {
//                    MinE = Q.OrderBy(D => D).First();

//                    if (IsEndByDay)
//                    {
//                        //暫存時間要以 MinE + 1 天為準(若是活動為當天結束, 它的結束時間應該是隔天的 00:00:00)
//                        MinE = MinE.Date.AddDays(1);
//                    }
//                }
//            }

//            if (MinS < MinE)
//            {
//                return MinS;
//            }

//            return MinE;
//        }
//    }

//    public static class Int32Extension
//    {
//        public static decimal ToDecimal(this int str)
//        {
//            return decimal.Parse(str.ToString());
//        }
//        public static float ToFloat(this int str)
//        {
//            return float.Parse(str.ToString());
//        }
//        public static double ToDouble(this int str)
//        {
//            return double.Parse(str.ToString());
//        }
//    }

//    public static class DecimalExtension
//    {
//        public static double ToDouble(this decimal str)
//        {
//            return double.Parse(str.ToString());
//        }
//        //去除 decimal ToString() 夾帶的尾數零
//        public static decimal Normalize(this decimal value)
//        {
//            return value / 1.000000000000000000000000000000000m;
//        }
//        //去除 decimal ToString() 夾帶的尾數零
//        public static string ToStringG29(this decimal value)
//        {
//            return value.ToString("G29");
//        }
//    }

//    public static class ControllerExtension
//    {
//        public static ContentResult RedirectMessageContent(this Controller controller, string msg, string nextURL = "/", int delaySeconds = 5)
//        {
//            return Su.Wu.RedirectMessageContent(controller, msg, nextURL, delaySeconds);
//        }
//    }
//}
