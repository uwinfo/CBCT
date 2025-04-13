using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Su
{
    public partial class MsSql
    {
        public class SqlBuilder
        {
            public string TableName { get; set; }
            private bool _IsNoCriteria = false;
            private readonly List<Criteria> LtAnd = new List<Criteria>();
            private readonly List<string> LtAndCriteria = new List<string>();
            private readonly List<Column> LtSet = new List<Column>();
            private readonly List<string> LtLeftjoin = new List<string>();
            private string? orderBy = null;

            /// <summary>
            /// Insert 時, 是否一定要有 CreatorId 和 CreateDate 這兩個欄位
            /// </summary>
            public static bool IsCreatorRequired { get; set; } = true;

            /// <summary>
            /// update 是否一定要有 ModifiorId 和 ModifyDate 這兩個欄位
            /// </summary>
            public static bool IsModifierRequired { get; set; } = true;

            public SqlBuilder(string tableName)
            {
                TableName = tableName;
            }

            public SqlBuilder Where(string FieldAndOP, object Value)
            {
                And(FieldAndOP, Value);
                return this;
            }

            public SqlBuilder Where(Criteria criteria)
            {
                And(criteria);
                return this;
            }


            static string? _NotDeletedSql = null;
            /// <summary>
            /// 可以在 startup 時, 自定 NotDeletedSql
            /// </summary>
            public static string NotDeletedSql
            {
                get
                {
                    if (_NotDeletedSql == null)
                    {
                        _NotDeletedSql = "Is_Deleted = 'N'";
                    }

                    return _NotDeletedSql;
                }
                set
                {
                    if (_NotDeletedSql == null)
                    {
                        _NotDeletedSql = value;
                    }
                    else
                    {
                        throw new Exception("NotDeletedSql can't not be reset.");
                    }
                }
            }

            /// <summary>
            /// 加上條件 Is_Deleted = 'N'
            /// </summary>
            /// <returns></returns>
            public SqlBuilder NotDeleted(string columnName = "Is_Deleted")
            {
                And(columnName.MsSqlField() + " =", "N");
                return this;
            }

            public SqlBuilder Leftjoin(string TableName, string on)
            {
                if (on.IsMsSqlInjection())
                {
                    throw new Exception("on 字串, 可能有 SQL Injection");
                }

                LtLeftjoin.Add("left join " + TableName.MsSqlObj() + " on " + on);

                return this;
            }

            public SqlBuilder And(Criteria criteria)
            {
                this.LtAnd.Add(criteria);
                return this;
            }

            public SqlBuilder And(string FieldAndOP, object Value)
            {
                this.LtAnd.Add(new Criteria(fieldAndOP: FieldAndOP, value: Value));
                return this;
            }

            public SqlBuilder OrCriterias(params Criteria[] orList)
            {
                return this.AndCriteria(orList.Select(x => x.Sql).ToOneString(" or "));
            }

            public SqlBuilder AndCriteria(string criteria)
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    throw new Exception("criteria 不可為空白");
                }

                if (criteria.IsMsSqlInjection())
                {
                    throw new Exception("可能有 SQL Injection");
                }

                this.LtAndCriteria.Add(criteria);
                return this;
            }

            /// <summary>
            /// 把 Values 的所有欄位(Properties and Fields)填入(注意, 預設會帶入 Id)
            /// </summary>
            /// <param name="Values"></param>
            /// <param name="onlyFields"></param>
            /// <param name="skipFields"></param>
            /// <returns></returns>
            public SqlBuilder SetObject(object Values, string onlyFields = null, string skipFields = null)
            {
                //不要 Global 的 Field 或 Property
                BindingFlags bindingFlags = BindingFlags.Public |
                          BindingFlags.NonPublic |
                          BindingFlags.Instance;

                List<string>? fields = null;
                if (!string.IsNullOrEmpty(onlyFields))
                {
                    fields = onlyFields.ToLower().Split(',').ToList();
                }

                List<string>? notFields = null;
                if (!string.IsNullOrEmpty(skipFields))
                {
                    notFields = skipFields.ToLower().Split(',').ToList();
                }

                foreach (var pf in Values.GetType().GetProperties(bindingFlags))
                {
                    if (
                        (fields == null || fields.Contains(pf.Name.ToLower()))
                        && (notFields == null || !notFields.Contains(pf.Name.ToLower()))
                        )
                    {
                        this.Set(pf.Name, pf.GetValue(Values, null));
                    }
                }

                foreach (var pf in Values.GetType().GetFields())
                {
                    if (!pf.IsDefined(typeof(CompilerGeneratedAttribute), false)) //去除掉 Compiler 會生出來的欄位 https://stackoverflow.com/questions/31528140/get-with-reflection-fields-that-are-not-generated-by-the-compiler
                    {
                        if ((fields == null || fields.Contains(pf.Name.ToLower()))
                            && (notFields == null || !notFields.Contains(pf.Name.ToLower()))
                            )
                        {
                            this.Set(pf.Name, pf.GetValue(Values));
                        }
                    }
                }
                return this;
            }

            public SqlBuilder SetDeleted(int? modifierId, bool isDeletedDate = false)
            {
                Set("Is_Deleted", "Y");
                if (modifierId != null)
                {
                    Set("ModifierId", modifierId);
                    Set("ModifyDate", DateTime.Now);
                }

                if (isDeletedDate)
                {
                    Set("DeletedDate", DateTime.Now);
                }

                return this;
            }

            public SqlBuilder Set(params Column[] fields)
            {
                foreach (var f in fields)
                {
                    this.LtSet.Add(f);
                }

                return this;
            }

            public SqlBuilder Set(string field, object value)
            {
                this.LtSet.Add(new Column(field, value));
                return this;
            }

            public SqlBuilder Set(ColumnName column, object value)
            {
                this.LtSet.Add(new Column(column.ToString(), value));
                return this;
            }


            public SqlBuilder OrderBy(string orderBy)
            {
                if (orderBy.IsMsSqlInjection())
                {
                    throw new Exception("OrderBy 可能有 SQL Injection.");
                }

                orderBy = orderBy.Trim();
                if (orderBy.StartsWith("order by"))
                {
                    orderBy = orderBy[8..].Trim();
                }

                this.orderBy = orderBy;
                return this;
            }

            public SqlBuilder OrderBy(ColumnName column)
            {
                return OrderBy(column.ToString());
            }

            /// <summary>
            /// update 或 delete 時, 必需設定條件, 否則就要執行 SetNoCriteria
            /// </summary>
            /// <returns></returns>
            public SqlBuilder SetNoCriteria()
            {
                _IsNoCriteria = true;
                return this;
            }

            /// <summary>
            /// 沒有 LtAnd, LtAndCriteria, SetNoCriteria --> 會發生 exception
            /// </summary>
            /// <param name="ltRes"></param>
            void BuildAndCriteria(List<string> ltRes)
            {
                List<string> parts = new List<string>();
                if (LtAnd.Count > 0)
                {
                    foreach (Criteria criteria in LtAnd)
                    {
                        if (criteria != null) // null 可以直接略過, 可能是和 bool 做 or 之後的結果.
                        {
                            parts.Add("(" + criteria.Sql + ")");
                        }
                    }
                }

                if (LtAndCriteria.Count > 0)
                {
                    foreach (string criteria in LtAndCriteria)
                    {
                        parts.Add("(" + criteria + ")");
                    }
                }

                if (parts.Count > 0)
                {
                    ltRes.Add("where");
                    ltRes.Add(string.Join(" and ", parts));
                }

                if (parts.Count == 0 && this._IsNoCriteria == false)
                {
                    throw new Exception("為避免意外, 無條件更新請先叫用 SetNoCriteria()");
                }
            }

            //public List<T> GetList<T>(string SelectFields = "*", int top = 0, Sql.DbId dbId = null)
            //{
            //    var list = Dt(SelectFields, top, dbId).GetList<T>();
            //    if (list.Count == 0)
            //    {
            //        return list;
            //    }

            //    MethodInfo method = list[0].GetType().GetMethod("ClearModifiedFields");
            //    if (method != null)
            //    {
            //        foreach (var item in list)
            //        {
            //            method = item.GetType().GetMethod("ClearModifiedFields");
            //            method.Invoke(item, null);
            //        }
            //    }

            //    return list;
            //}

            /// <summary>
            /// PageDT 也會用
            /// </summary>
            /// <param name="SelectFields"></param>
            /// <param name="top"></param>
            /// <param name="isNolock"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            public string SelectSql(string SelectFields = "*", int top = 0, bool isNolock = true, int id = -1)
            {
                if (id > 0)
                {
                    this.And("Id = ", id);
                }

                List<string> ltRes = new List<string>();

                if (LtSet.Count > 0)
                {
                    throw new Exception("不可包含Set");
                }
                ltRes.Add("select");
                if (top > 0)
                {
                    ltRes.Add("top " + top);
                }
                ltRes.Add(SelectFields);
                ltRes.Add("from");
                ltRes.Add(TableName.MsSqlObj());
                if (isNolock)
                {
                    ltRes.Add("(NoLock)");
                }
                if (LtLeftjoin.Count > 0)
                {
                    ltRes.Add(string.Join(" ", LtLeftjoin));
                }

                BuildAndCriteria(ltRes);

                if (!string.IsNullOrEmpty(orderBy))
                {
                    orderBy = orderBy.Trim();
                    if (orderBy.ToLower().StartsWith("order by"))
                    {
                        orderBy = orderBy[8..].Trim();
                    }

                    ltRes.Add("order by " +
                            orderBy.Split(",")
                            .Select(x => x.Trim().MsSqlObj(true))
                            .ToOneString(", "));
                }

                return ltRes.ToOneString(" ");
            }

            //public DataTable Select(string SelectFields = "*", int top = 0, Sql.DbId dbId = null, bool isNolock = true, int id = -1)
            //{
            //    return Dt(SelectFields, top, dbId, isNolock, id);
            //}

            //public DataTable Select(params ColumnName[] columnNames)
            //{
            //    var SelectFields = columnNames.Select(x => x.ToString()).ToOneString(", ");

            //    return Select(SelectFields);
            //}

            //public DataTable Dt(string SelectFields = "*", int top = 0, Sql.DbId dbId = null, bool isNolock = true, int id = -1)
            //{
            //    string sql = SelectSql(SelectFields, top, isNolock, id);

            //    //Su.Debug.WriteLine("SelectSql: ");
            //    //Su.Debug.WriteLine(sql);

            //    return Su.MsSql.DtFromSql(sql, dbId);
            //}

            //public IEnumerable<DataRow> Rows(string SelectFields = "*", int top = 0, Sql.DbId dbId = null, bool isNolock = true, int id = -1)
            //{
            //    return Dt(SelectFields, top, dbId, isNolock, id).AsEnumerable();
            //}

            //public IEnumerable<T> ValuesOfField<T>(string fieldName, int top = 0, Sql.DbId dbId = null, bool isNolock = true, int id = -1)
            //{
            //    if (fieldName.StartsWith("[") && fieldName.EndsWith("]"))
            //    {
            //        fieldName = fieldName[1..^1];
            //    }
            //    return Rows(fieldName, top, dbId, isNolock, id).Select(r => r.Field<T>(fieldName));
            //}

            //public IEnumerable<T> ValuesOfField<T>(ColumnName columnName, int top = 0, Sql.DbId dbId = null, bool isNolock = true, int id = -1)
            //{
            //    return ValuesOfField<T>(columnName.ToString(), top, dbId, isNolock, id);
            //}

            //public DataRow FirstRow(int id = -1, Sql.DbId dbId = null)
            //{
            //    if (id > 0)
            //    {
            //        this.And("Id = ", id);
            //    }

            //    var ret = Dt(top: 1, dbId: dbId);
            //    return ret.Rows.Count == 0 ? null : ret.Rows[0];
            //}

            //public T First<T>(int id = -1)
            //{
            //    if (id > 0)
            //    {
            //        this.And("Id = ", id);
            //    }

            //    var row = FirstRow();
            //    if (row == null)
            //    {
            //        return default;
            //    }

            //    var item = row.CopyTo<T>();

            //    MethodInfo method = item.GetType().GetMethod("ClearModifiedFields");
            //    if (method != null)
            //    {
            //        method.Invoke(item, null);
            //    }

            //    return item;
            //}

            //public void AddInsertSqlToQueue(int? creatorId = null, bool skipCreator = false, bool skipModifier = false)
            //{
            //    AddSqlToQueue(InsertSql(creatorId, skipCreator, skipModifier));
            //}

            public string InsertSql(int? creatorId = null, bool skipCreator = false, bool skipModifier = false)
            {
                if (creatorId != null)
                {
                    this.Set("CreatorId", creatorId);
                    this.Set("CreateDate", DateTime.Now);
                    this.Set("ModifierId", creatorId);
                    this.Set("ModifyDate", DateTime.Now);
                }

                if (IsCreatorRequired && !skipCreator)
                {
                    if (!LtSet.Any(x => x.FieldName == "CreatorId"))
                    {
                        throw new Exception("CreatorId is required.");
                    }

                    if (!LtSet.Any(x => x.FieldName == "CreateDate"))
                    {
                        throw new Exception("CreateDate is required.");
                    }
                }


                if (IsModifierRequired && !skipModifier)
                {
                    if (!LtSet.Any(x => x.FieldName == "ModifierId"))
                    {
                        throw new Exception("ModifierId is required.");
                    }

                    if (!LtSet.Any(x => x.FieldName == "ModifyDate"))
                    {
                        throw new Exception("ModifyDate is required.");
                    }
                }

                return $"insert into {TableName.MsSqlObj()}({LtSet.Select(x => x.FieldName.MsSqlObj()).ToOneString(", ")}) values({LtSet.Select(x => x.MsSqlValue).ToOneString(", ")})";
            }

            ///// <summary>
            ///// 結尾的 _ 表示是 Local 的 function.
            ///// </summary>
            ///// <param name="sql"></param>
            ///// <param name="timeout"></param>
            ///// <returns></returns>
            //private Int32 ExecuteSQL_(string sql, Int32 timeout = 0)
            //{
            //    return ExecuteSql(sql, DbId.NotSet, timeout);
            //}


            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="dbc"></param>
            ///// <param name="timeout"></param>
            ///// <param name="creatorId">非 Null 時, 會同時設定CreatorId, ModifyDate, ModifierId, ModifyDate</param>
            ///// <param name="skipCreator"></param>
            ///// <param name="skipModifier"></param>
            ///// <returns></returns>
            //public int Insert(Sql.DbId dbId = null, int timeout = 0, int? creatorId = null, bool skipCreator = false, bool skipModifier = false)
            //{
            //    return ExecuteSql(
            //        InsertSql(creatorId, skipCreator, skipModifier)
            //        , dbId, timeout, IsReturnIdentity: true);
            //}

            //public int Update(Sql.DbId dbId = null, int timeout = 0, int? modifierId = null, bool skipModifier = false)
            //{
            //    return ExecuteSql(UpdateSql(modifierId, skipModifier), dbId, timeout);
            //}

            //public const string QueueSqlKey = "SqlInQueue";

            //public static void AddSqlToQueue(string sql)
            //{
            //    if (CurrentContext.Current.Items[QueueSqlKey] == null)
            //    {
            //        CurrentContext.Current.Items[QueueSqlKey] = new List<string>();
            //    }

            //    ((List<string>)CurrentContext.Current.Items[QueueSqlKey]).Add(sql);
            //}

            ///// <summary>
            ///// 產生 update sql , 並放入待執行的 Queue 中. 以便在 transaction 中執行.
            ///// </summary>
            ///// <param name="modifierId"></param>
            ///// <param name="skipModifier"></param>
            //public void AddUpdateSqlToQueue(int? modifierId = null, bool skipModifier = false)
            //{
            //    AddSqlToQueue(UpdateSql(modifierId, skipModifier));
            //}

            ///// <summary>
            ///// 在 Queue 中的 所有 SQL. 
            ///// </summary>
            ///// <returns></returns>
            //public static string QueuedSql()
            //{
            //    if (CurrentContext.Current.Items[QueueSqlKey] == null)
            //    {
            //        throw new Exception("Queue is null");
            //    }

            //    return ((List<string>)CurrentContext.Current.Items[QueueSqlKey]).ToOneString(";\r\n") + ";";
            //}

            ///// <summary>
            ///// 同時執行 Queue 中的 sql, 這裡面會使用 TransactionScope
            ///// </summary>
            //public static void CommitQueuedSql()
            //{
            //    using (var tran = new System.Transactions.TransactionScope())
            //    {
            //        ExecuteSql(QueuedSql());
            //        tran.Complete();
            //    }

            //    CurrentContext.Current.Items[QueueSqlKey] = null;
            //}



            public string UpdateSql(int? modifierId = null, bool skipModifier = false)
            {
                if (modifierId != null)
                {
                    this.Set("ModifierId", modifierId);
                    this.Set("ModifyDate", DateTime.Now);
                }

                if (IsModifierRequired && !skipModifier && !LtSet.Any(x => x.FieldName == "ModifierId"))
                {
                    throw new Exception("ModifierId is required.");
                }

                if (IsModifierRequired && !skipModifier && !LtSet.Any(x => x.FieldName == "ModifyDate"))
                {
                    throw new Exception("ModifyDate is required.");
                }

                List<string> ltRes = new List<string>();
                ltRes.Add($"update {TableName.MsSqlObj()} set ");
                ltRes.Add(LtSet.Select(x => $"{x.FieldName.MsSqlObj()} = " + x.MsSqlValue).ToOneString(", "));
                BuildAndCriteria(ltRes);

                return ltRes.ToOneString(" ");
            }

            //public int Delete()
            //{
            //    List<string> ltRes = new List<string>();
            //    ltRes.Add($"Delete {TableName.MsSqlObj()}");
            //    BuildAndCriteria(ltRes);

            //    return Su.MsSql.ExecuteSql(ltRes.ToOneString(" "));
            //}

            public static string SqlValue(object value)
            {
                if (value == null)
                {
                    return " null ";
                }

                switch (value.GetType().ToString())
                {
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

            //public int GetCount(Sql.DbId dbId = null)
            //{
            //    List<string> ltRes = new List<string>();

            //    ltRes.Add($"select count(*) from {TableName.MsSqlObj()}");
            //    BuildAndCriteria(ltRes);

            //    return Convert.ToInt32(GetSingleValue(ltRes.ToOneString(" "), dbId));
            //}

            //public PagedDt GetPagedDt(int pageNum, int pageSize = 20, Sql.DbId dbId = null, string fields = "*")
            //{
            //    var oRes = new PagedDt();
            //    oRes.TotalRecord = GetCount(dbId);
            //    oRes.PageSize = pageSize;

            //    if (oRes.TotalRecord % pageSize == 0)
            //    {
            //        oRes.TotalPage = oRes.TotalRecord / pageSize;
            //    }
            //    else
            //    {
            //        oRes.TotalPage = (oRes.TotalRecord / pageSize) + 1;
            //    }

            //    if (oRes.TotalPage < pageNum)
            //    {
            //        pageNum = oRes.TotalPage;
            //    }

            //    if (oRes.TotalRecord == 0)
            //    {
            //        //無資料固定回傳頁數1
            //        pageNum = 1;
            //    }

            //    oRes.CurrentPage = pageNum;

            //    if (oRes.TotalRecord > 0)
            //    {
            //        oRes.Dt = PagerDT(oRes.CurrentPage, pageSize, dbId, fields);
            //    }

            //    oRes.SetSQL(SelectSql(fields));

            //    return oRes;
            //}

            ///// <summary>
            ///// 把 PageSql 記錄下來, Debug 可能會用到
            ///// </summary>
            //public string PageSql = null;
            //protected DataTable PagerDT(int CurrentPage, int PageSize = 20, Sql.DbId dbId = null, string fields = "*")
            //{
            //    int StartIndex = (CurrentPage - 1) * PageSize;

            //    if (string.IsNullOrWhiteSpace(orderBy))
            //    {
            //        //一定要有 Order By 才能用 OFFSET, 就假設有 id 吧. 沒 id 又沒有 orderBy 就會丟 Exception 了.
            //        orderBy = "id";
            //    }

            //    PageSql = SelectSql(fields) + " OFFSET " + StartIndex + " ROWS FETCH NEXT " + PageSize + " ROWS ONLY"; // SQL2012 之後的版本

            //    DataTable dt = null;
            //    try
            //    {
            //        dt = DtFromSql(PageSql, dbId);
            //    }
            //    catch (Exception ex)
            //    {
            //        if (ex.ToString().Contains("Incorrect syntax near 'OFFSET'."))
            //        {
            //            //SQL2008R2 之前的版本
            //            PageSql = "SELECT * " +
            //                      " FROM (" + SelectSql(SelectFields: fields + ", ROW_NUMBER() OVER (" + orderBy + ") AS x ") + ") AS tbl" +
            //                      " WHERE tbl.x Between " + StartIndex + " AND " + (StartIndex + PageSize);

            //            dt = DtFromSql(PageSql, dbId);
            //        }
            //    }
            //    return dt;
            //}
        }
    }
}
