using Npgsql;
using System.Data;

namespace Su
{
    //public static partial class StringExtension
    //{
    //    public static PgSql.SqlBuilder PgSqlBuilder(this string tableName)
    //    {
    //        return new PgSql.SqlBuilder(tableName);
    //    }
    //}

    public partial class PgSql
    {
        //private const string PageDbNameKey = "Su.PgSql.PageDbName";

        ///// <summary>
        ///// 只有在指定日期內會設定
        ///// </summary>
        //public static string? DebugDbc { get; set; }

        ///// <summary>
        ///// 發生 TransactionAbortedException 的嘗試次數
        ///// </summary>
        //public const int maxTransactionAborted = 10;

        static Sql.DbId _DefaultDbId = null;

        /// <summary>
        /// 本專案的預設 DbId
        /// </summary>
        public static Sql.DbId DefaultDbId
        {
            get
            {
                //假設都會被設定，不考慮未設定的狀況
                return _DefaultDbId;
            }

            set
            {
                if (_DefaultDbId != null)
                {
                    throw new Exception("不可重覆設定 DefaultDbId");
                }

                _DefaultDbId = value;
            }
        }

        static readonly Dictionary<Sql.DbId, string> Dbcs = new();

        public static void AddDbc(Sql.DbId dbId, string dbc)
        {
            Dbcs.Add(dbId, dbc);
        }

        public static string GetDbc(Sql.DbId dbId = null)
        {
            if (dbId != null)
            {
                return Dbcs[dbId];
            }

            //if (_httpContext.Items != null && _httpContext.Items[PageDbNameKey] != null)
            //{
            //    return Dbcs[(Sql.DbId)_httpContext.Items[PageDbNameKey]];
            //}

            return Dbcs[DefaultDbId];
        }


        ///// <summary>
        ///// 因為 DBC 會外洩, 所以要改為 internal
        ///// </summary>
        ///// <param name="DBC"></param>
        ///// <returns></returns>
        //internal static NpgsqlConnection GetOpenedConnection(Sql.DbId dbId = null)
        //{
        //    NpgsqlConnection sqlConn = new NpgsqlConnection(GetDbc(dbId));
        //    sqlConn.Open();
        //    return sqlConn;
        //}

        ///// <summary>
        ///// 回傳影響的資料數量
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="DBC"></param>
        ///// <param name="timeout"></param>
        ///// <param name="IsReturnIdentity"></param>
        ///// <returns></returns>
        //public static long ExecuteSql(string sql, Sql.DbId dbId = null, Int32 timeout = 0, bool IsReturnIdentity = false)
        //{
        //    NpgsqlConnection sqlConn;
        //    long Res = 0;
        //    using (sqlConn = GetOpenedConnection(dbId))
        //    {
        //        NpgsqlCommand Cmd = new NpgsqlCommand();
        //        if (timeout > 0)
        //        {
        //            Cmd.CommandTimeout = timeout;
        //        }
        //        Cmd.CommandText = sql;
        //        Cmd.CommandType = CommandType.Text;
        //        Cmd.Connection = sqlConn;

        //        Res = Cmd.ExecuteNonQuery();

        //        if (IsReturnIdentity == true)
        //        {
        //            Res = GetIdentity(sqlConn);
        //        }

        //        sqlConn.Close();
        //    }
        //    return Res;
        //}

        //public static long ExecuteSql(string sql, NpgsqlConnection sqlConn, Int32 timeout = 0, bool IsReturnIdentity = false)
        //{
        //    NpgsqlCommand Cmd = new NpgsqlCommand();
        //    if (timeout > 0)
        //    {
        //        Cmd.CommandTimeout = timeout;
        //    }
        //    Cmd.CommandText = sql;
        //    Cmd.CommandType = CommandType.Text;
        //    Cmd.Connection = sqlConn;

        //    long Res = Cmd.ExecuteNonQuery();
        //    if (IsReturnIdentity == true)
        //    {
        //        Res = GetIdentity(sqlConn);
        //    }

        //    return Res;
        //}

        //public class Column
        //{
        //    /// <summary>
        //    /// 這個盡量不要用了, 請改用 ColumnName (保留相容性)
        //    /// </summary>
        //    public string FieldName = "";

        //    /// <summary>
        //    /// 欄位名稱
        //    /// </summary>
        //    public string ColumnName
        //    {
        //        get
        //        {
        //            return FieldName;
        //        }
        //        set
        //        {
        //            FieldName = value;
        //        }
        //    }

        //    public object Value = new object();

        //    public Column(string columnName, object value)
        //    {
        //        this.FieldName = columnName.Trim();

        //        this.Value = value;
        //    }

        //    public string PgSqlValue
        //    {
        //        get
        //        {
        //            return Value.PgSqlValue();
        //        }
        //    }
        //}

        ///// <summary>
        ///// ColumnName 的摘要描述
        ///// </summary>
        //public class ColumnName
        //{
        //    public readonly string OriginalName = null;
        //    readonly string Name = null;
        //    public ColumnName(string name)
        //    {
        //        OriginalName = name;
        //        Name = name.PgSqlColumnName();
        //    }

        //    public override string ToString()
        //    {
        //        return Name;
        //    }

        //    /// <summary>
        //    /// Set A = B
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <param name="b"></param>
        //    /// <returns></returns>
        //    public static Column operator ^(ColumnName a, object b)
        //    {
        //        return new Column(a.OriginalName, b);
        //    }

        //    public static Criteria operator >(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " >", b);
        //    }

        //    public static Criteria operator <(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " <", b);
        //    }

        //    public static Criteria operator ==(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " =", b);
        //    }

        //    public static Criteria operator !=(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " <>", b);
        //    }

        //    public static Criteria operator >=(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " >=", b);
        //    }

        //    public static Criteria operator <=(ColumnName a, object b)
        //    {
        //        return new Criteria(a.Name + " <=", b);
        //    }

        //    /// <summary>
        //    /// 這個是 in
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <param name="b"></param>
        //    /// <returns></returns>
        //    public static Criteria operator |(ColumnName a, string b)
        //    {
        //        return new Criteria(a.Name + " in", b);
        //    }


        //    public static Criteria operator |(ColumnName a, IEnumerable<string> b)
        //    {
        //        if (b == null || !b.Any())
        //        {
        //            return new Criteria(a.Name + " in", "()");
        //        }
        //        else
        //        {
        //            return new Criteria(a.Name + " in", "('" + string.Join(",", b.Select(x => x.PgSqlValue())) + ")");
        //        }
        //    }

        //    public static Criteria operator |(ColumnName a, IEnumerable<int> b)
        //    {
        //        if (b == null || !b.Any())
        //        {
        //            return new Criteria(a.Name + " in", "()");
        //        }
        //        else
        //        {
        //            return new Criteria(a.Name + " in", "(" + string.Join(", ", b.Select(x => x.ToString())) + ")");
        //        }
        //    }

        //    /// <summary>
        //    /// 這個是 in
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <param name="b"></param>
        //    /// <returns></returns>
        //    public static Criteria operator |(ColumnName a, List<int> b)
        //    {
        //        return new Criteria(a.Name + " in", "(" + string.Join(", ", b) + ")");
        //    }

        //    /// <summary>
        //    /// 這個是 like
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <param name="b"></param>
        //    /// <returns></returns>
        //    public static Criteria operator %(ColumnName a, string b)
        //    {
        //        return new Criteria(a.Name + " like", b);
        //    }

        //    /// <summary>
        //    /// 這是 wildcard 的 like, 會自動在後面的字串左右加上 %
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <param name="b"></param>
        //    /// <returns></returns>
        //    public static Criteria operator *(ColumnName a, string b)
        //    {
        //        return new Criteria(a.Name + " like", "%" + b + "%");
        //    }

        //    /// <summary>
        //    /// 這個是 == 'N'
        //    /// </summary>
        //    /// <param name="a"></param>
        //    /// <returns></returns>
        //    public static Criteria operator !(ColumnName a)
        //    {
        //        return new Criteria(a.Name + " =", "N");
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        if (ReferenceEquals(this, obj))
        //        {
        //            return true;
        //        }

        //        if (obj is null)
        //        {
        //            return false;
        //        }

        //        throw new NotImplementedException();
        //    }

        //    public override int GetHashCode()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public static DataTable DtFromSql(string Sql, NpgsqlConnection conn, Int32 timeout = 0)
        //{
        //    if (conn == null)
        //    {
        //        throw new Exception("conn 不可為 null");
        //    }

        //    var DA = new NpgsqlDataAdapter(Sql, conn);
        //    if (timeout > 0)
        //    {
        //        DA.SelectCommand.CommandTimeout = timeout;
        //    }

        //    DataTable DT = new DataTable();
        //    DA.Fill(DT);
        //    return DT;
        //}

        ///// <summary>
        ///// 用 SQL 字串取得 DataTable
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
        ///// <param name="DBC"></param>
        ///// <param name="timeout"></param>
        ///// <param name="isCheckDangerSQL"></param>
        ///// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
        ///// <returns></returns>
        //public static DataTable DtFromSql(string sql, object parameters, Sql.DbId dbId, Int32 timeout = 30, bool isCheckDangerSQL = true, object sqlColumns = null, object sqlTables = null, bool isRemoveCrLf = true)
        //{
        //    return DtFromSql(sql.ToPgSql(parameters, sqlColumns, sqlTables, isCheckDangerSQL, isRemoveCrLf), dbId, timeout, isCheckDangerSQL: false);
        //}

        /// <summary>
        /// 這個會先檢查 sqlinjection, 再檢檢危險的指令
        /// 因為避免造成相容性的問題, 所以只抓幾個最不可能出現的幾個指令和表單.
        /// xp_cmdshell, sysobjects, information_schema, syscolumns, --
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static void CheckDangerSQL(string sql)
        {
            sql.CheckPgSqlInjection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DBC"></param>
        /// <param name="timeout">微軟預設值 30 秒</param>
        /// <param name="isCheckDangerSQL"></param>
        /// <returns></returns>
        public static DataTable DtFromSql(string sql, Sql.DbId dbId = null, Int32 timeout = 30, bool isCheckDangerSQL = true, bool IsRemoveCrLf = true)
        {
            if (IsRemoveCrLf)
            {
                sql = sql.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            }

            if (isCheckDangerSQL)
            {
                CheckDangerSQL(sql);
            }

            using (var DA = new NpgsqlDataAdapter(sql, GetDbc(dbId)))
            {
                //不可為 null
                DataTable DT = new DataTable();

                DA.SelectCommand.CommandTimeout = timeout;

                DA.Fill(DT);

                return DT;
            }
        }

        ///// <summary>
        ///// 找不到時回傳 null.
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="dbId"></param>
        ///// <returns></returns>
        //public static object GetSingleValue(string sql, Sql.DbId dbId = null)
        //{
        //    try
        //    {
        //        DataTable DT = DtFromSql(sql, dbId);
        //        if (DT.Rows.Count == 0)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return DT.Rows[0][0];
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Su.SQL.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
        //    }
        //}

        //public static long GetIdentity(NpgsqlConnection conn)
        //{
        //    DataTable DT = DtFromSql("Select LASTVAL();", conn);
        //    return Convert.ToInt64(DT.Rows[0][0]);
        //}
    }
}
