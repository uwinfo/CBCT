using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Su
{
    public static partial class StringExtension
    {
        public static MsSql.SqlBuilder MsSqlBuilder(this string tableName)
        {
            return new MsSql.SqlBuilder(tableName);
        }
    }

    public partial class MsSql
    {
        public enum UnicodeType
        {
            /// <summary>
            /// 依欄位名稱決定, 若欄位名稱為 Is_ 開頭, 即為非 Unicode. 其它為 Unicode
            /// </summary>
            ByFieldName = 0,
            Yes = 1,
            No = 2
        }

        public class Column
        {
            public string FieldName = "";
            public object Value = new object();
            public bool IsNotUnicode = false;

            public Column(string columnName, object value, UnicodeType unicode = UnicodeType.ByFieldName)
            {
                this.FieldName = columnName.Trim();

                if (FieldName.ToLower().StartsWith("is_") && unicode == UnicodeType.ByFieldName)
                {
                    IsNotUnicode = true;
                }
                this.Value = value;
            }

            public string MsSqlValue
            {
                get
                {
                    return Value.MsSqlValue(IsNotUnicode);
                }
            }
        }

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
                if(_DefaultDbId != null)
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
            if(dbId != null)
            {
                return Dbcs[dbId];
            }

            if (CurrentContext.Current.Items[PageDbNameKey] != null)
            {
                return Dbcs[(Sql.DbId)CurrentContext.Current.Items[PageDbNameKey]];
            }

            return Dbcs[DefaultDbId];
        }

        private const string PageDbNameKey = "Su.MsSql.PageDbName";

        /// <summary>
        /// 在一個頁面中使用的 DBC, 設定用的介面. 讀取時請用 GetDBC()
        /// </summary>
        public static Sql.DbId PageDbName
        {
            set
            {
                if (CurrentContext.Current.Items[PageDbNameKey] == null)
                {
                    CurrentContext.Current.Items[PageDbNameKey] = value;
                }
                else
                {
                    throw new Exception("不可重覆設定 PageDBC。");
                }
            }
        }

        /// <summary>
        /// 因為 DBC 會外洩, 所以要改為 internal
        /// </summary>
        /// <param name="DBC"></param>
        /// <returns></returns>
        internal static SqlConnection GetOpenedConnection(Sql.DbId dbId = null)
        {
            SqlConnection sqlConn = new SqlConnection(GetDbc(dbId));
            sqlConn.Open();
            return sqlConn;
        }

        /// <summary>
        /// 回傳影響的資料數量
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DBC"></param>
        /// <param name="timeout"></param>
        /// <param name="IsReturnIdentity"></param>
        /// <returns></returns>
        public static Int32 ExecuteSql(string sql, Sql.DbId dbId = null, Int32 timeout = 0, bool IsReturnIdentity = false)
        {
            SqlConnection sqlConn;
            Int32 Res = 0;
            using (sqlConn = GetOpenedConnection(dbId))
            {
                SqlCommand Cmd = new SqlCommand();
                if (timeout > 0)
                {
                    Cmd.CommandTimeout = timeout;
                }
                Cmd.CommandText = sql;
                Cmd.CommandType = CommandType.Text;
                Cmd.Connection = sqlConn;

                Res = Cmd.ExecuteNonQuery();

                if (IsReturnIdentity == true)
                {
                    Res = GetIdentity(sqlConn);
                }

                sqlConn.Close();
            }
            return Res;
        }

        public static int ExecNonQuery(SqlCommand cmd, Sql.DbId dbId = null)
        {
            using (var conn = GetOpenedConnection(dbId))
            {
                cmd.Connection = conn;
                return cmd.ExecuteNonQuery();
            }
        }

        public static async Task<int> ExecNonQueryAsync(SqlCommand cmd, Sql.DbId dbId = null)
        {
            using (var conn = GetOpenedConnection(dbId))
            {
                cmd.Connection = conn;
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        //public static Int32 ExecuteSQL(string sql, SqlTransaction tran, Int32 timeout = 0, bool IsReturnIdentity = false)
        //{
        //    int Res = 0;
        //    if (tran == null)
        //    {
        //        Res = ExecuteSQL(sql, timeout: timeout, IsReturnIdentity: IsReturnIdentity);
        //    }
        //    else
        //    {
        //        SqlCommand cmd = new SqlCommand();
        //        if (timeout > 0)
        //        {
        //            cmd.CommandTimeout = timeout;
        //        }
        //        cmd.CommandText = sql;
        //        cmd.CommandType = CommandType.Text;
        //        cmd.Connection = tran.Connection;
        //        cmd.Transaction = tran;
        //        Res = cmd.ExecuteNonQuery();

        //        if (IsReturnIdentity == true)
        //        {
        //            Res = GetIdentity(tran);
        //        }
        //    }
        //    return Res;
        //}

        public static int ExecuteSql(string sql, SqlConnection sqlConn, Int32 timeout = 0, bool IsReturnIdentity = false)
        {
            SqlCommand Cmd = new SqlCommand();
            if (timeout > 0)
            {
                Cmd.CommandTimeout = timeout;
            }
            Cmd.CommandText = sql;
            Cmd.CommandType = CommandType.Text;
            Cmd.Connection = sqlConn;

            int Res = Cmd.ExecuteNonQuery();
            if (IsReturnIdentity == true)
            {
                //DataTable dt = Su.SQL.DtFromSql("Select @@Identity", sqlConn);
                //Res = Convert.ToInt32(dt.Rows[0][0]);
                Res = GetIdentity(sqlConn);
            }

            return Res;
        }


        /// <summary>
        /// ColumnName 的摘要描述
        /// </summary>
        public class ColumnName
        {
            public string OriginalName { get; set; }

            readonly string Name = null;
            public ColumnName(string name)
            {
                OriginalName = name;
                Name = name.MsSqlField();
            }

            public override string ToString()
            {
                return Name;
            }

            public static Column operator ^(ColumnName a, object b)
            {
                return new Column(a.ToString(), b);
            }

            public static Criteria operator >(ColumnName a, object b)
            {
                return new Criteria(a.Name + " >", b);
            }

            public static Criteria operator <(ColumnName a, object b)
            {
                return new Criteria(a.Name + " <", b);
            }

            public static Criteria operator ==(ColumnName a, object b)
            {
                return new Criteria(a.Name + " =", b);
            }

            public static Criteria operator !=(ColumnName a, object b)
            {
                return new Criteria(a.Name + " <>", b);
            }

            public static Criteria operator >=(ColumnName a, object b)
            {
                return new Criteria(a.Name + " >=", b);
            }

            public static Criteria operator <=(ColumnName a, object b)
            {
                return new Criteria(a.Name + " <=", b);
            }

            /// <summary>
            /// 這個是 in
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator |(ColumnName a, string b)
            {
                return new Criteria(a.Name + " in", b);
            }


            public static Criteria operator |(ColumnName a, IEnumerable<string> b)
            {
                if (b == null || !b.Any())
                {
                    return new Criteria(a.Name + " in", "()");
                }
                else
                {
                    return new Criteria(a.Name + " in", "('" + string.Join(",", b.Select(x => x.MsSqlValue())) + ")");
                }
            }

            public static Criteria operator |(ColumnName a, IEnumerable<int> b)
            {
                if (b == null || !b.Any())
                {
                    return new Criteria(a.Name + " in", "()");
                }
                else
                {
                    return new Criteria(a.Name + " in", "(" + string.Join(", ", b.Select(x => x.ToString())) + ")");
                }
            }

            /// <summary>
            /// 這個是 in
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator |(ColumnName a, List<int> b)
            {
                return new Criteria(a.Name + " in", "(" + string.Join(", ", b) + ")");
            }

            /// <summary>
            /// 這個是 like
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator %(ColumnName a, string b)
            {
                return new Criteria(a.Name + " like", b);
            }

            /// <summary>
            /// 這是 wildcard 的 like, 會自動在後面的字串左右加上 %
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator *(ColumnName a, string b)
            {
                return new Criteria(a.Name + " like", "%" + b + "%");
            }

            /// <summary>
            /// 這個是 == 'N'
            /// </summary>
            /// <param name="a"></param>
            /// <returns></returns>
            public static Criteria operator !(ColumnName a)
            {
                return new Criteria(a.Name + " =", "N");
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj is null)
                {
                    return false;
                }

                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
        }
        
        public static DataTable DtFromSql(string Sql, SqlConnection conn, Int32 timeout = 0)
        {
            if (conn == null)
            {
                throw new Exception("conn 不可為 null");
            }

            var DA = new SqlDataAdapter(Sql, conn);
            if (timeout > 0)
            {
                DA.SelectCommand.CommandTimeout = timeout;
            }

            DataTable DT = new DataTable();
            DA.Fill(DT);
            return DT;
        }

        //        public static DataTable GetTableStructure(string TableName, string NewDBC = "")
        //        {
        //            if (NewDBC == "")
        //            {
        //                NewDBC = GetDBC();
        //            }

        //            string Sql = @"SELECT systypes.[Name] as TypeName, syscolumns.Length, sysobjects.Name AS [Table], syscolumns.Name AS [Column], syscolumns.isnullable AS Is_Nullable, TE.Value as Description 
        //FROM (sysobjects INNER JOIN syscolumns ON syscolumns.Id = sysobjects.Id) Left JOIN systypes on syscolumns.xusertype = systypes.xusertype 
        //Left Join (SELECT * FROM fn_listextendedproperty(NULL, 'user', 'dbo', 'TABLE', {TableName}, 'COLUMN', null)) TE on TE.objname COLLATE Chinese_Taiwan_Stroke_CI_AS = syscolumns.[name] 
        //where sysobjects.Name = {TableName}";

        //            try
        //            {
        //                return DtFromSql(Sql, new { TableName }, NewDBC, isCheckDangerSQL: false);
        //            }
        //            catch (Exception ex)
        //            {
        //                Su.WU.WriteText("GetTableStructure: " + Sql);
        //                Su.WU.WriteText(ex.ToString());

        //                throw (new Exception(ex.ToString() + ", " + ex.StackTrace));
        //            }
        //        }


        //        /// <summary>
        //        /// 找不到時回傳 null.
        //        /// </summary>
        //        /// <param name="sql"></param>
        //        /// <param name="DBC"></param>
        //        /// <returns></returns>
        //        public static object GetSingleValue(string sql, string DBC = null)
        //        {
        //            try
        //            {
        //                DataTable DT = DtFromSql(sql, GetDBC(DBC));
        //                if (DT.Rows.Count == 0)
        //                {
        //                    return null;
        //                }
        //                else
        //                {
        //                    return DT.Rows[0][0];
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception("Su.MsSql.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
        //            }
        //        }

        //        public static object GetSingleValue(string sql, SqlTransaction tran)
        //        {
        //            try
        //            {
        //                DataTable DT = DtFromSql(sql, tran);
        //                if (DT.Rows.Count == 0)
        //                {
        //                    return null;
        //                }
        //                else
        //                {
        //                    return DT.Rows[0][0];
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception("Su.MsSql.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
        //            }
        //        }

        //        /// <summary>
        //        /// 因為避免造成相容性的問題, 所以只抓幾個最不可能出現的幾個指令和表單.
        //        /// xp_cmdshell, sysobjects, information_schema, syscolumns
        //        /// </summary>
        //        /// <param name="sql"></param>
        //        /// <returns></returns>
        //        public static void CheckDangerSQL(string sql)
        //        {
        //            var sl = sql.ToLower();
        //            if (sl.Contains("xp_cmdshell") ||
        //                sl.Contains("sysobjects") ||
        //                sl.Contains("information_schema") ||
        //                sl.Contains("syscolumns"))
        //            {
        //                throw new Exception("There is danger in sql");
        //            }
        //        }

        /// <summary>
        /// 用 SQL 字串取得 DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
        /// <param name="DBC"></param>
        /// <param name="timeout"></param>
        /// <param name="isCheckDangerSQL"></param>
        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
        /// <returns></returns>
        public static DataTable DtFromSql(string sql, object parameters, Sql.DbId dbId, Int32 timeout = 30, bool isCheckDangerSQL = true, object sqlObjects = null, bool isRemoveCrLf = true)
        {
            return DtFromSql(sql.ToMsSql(parameters, sqlObjects, isCheckDangerSQL, isRemoveCrLf), dbId, timeout, isCheckDangerSQL: false);
        }

        /// <summary>
        /// 因為避免造成相容性的問題, 所以只抓幾個最不可能出現的幾個指令和表單.
        /// xp_cmdshell, sysobjects, information_schema, syscolumns, --
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static void CheckDangerSQL(string sql)
        {
            if (sql.IsMsSqlInjection())
            {
                throw new Exception("There is invalid words in sql (may cause sql injection)");
            }

            var sl = sql.ToLower();
            if (sl.Contains("xp_cmdshell") ||
                sl.Contains("sysobjects") ||
                sl.Contains("information_schema") ||
                sl.Contains("syscolumns") ||
                sl.Contains("drop"))
            {
                throw new Exception("There is danger in sql");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DBC"></param>
        /// <param name="timeout">微軟預設值 30 秒</param>
        /// <param name="isCheckDangerSQL"></param>
        /// <returns></returns>
        public static DataTable DtFromSql(string sql, Sql.DbId dbId, Int32 timeout = 30, bool isCheckDangerSQL = true, bool isRemoveCrLf = true)
        {
            //Www.Uc.AppendLog(" \r\n" + sql + "\r\n ================================================================================================");

            if (isRemoveCrLf)
            {
                sql = sql.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            }

            if (isCheckDangerSQL)
            {
                CheckDangerSQL(sql);
            }

            using (var DA = new SqlDataAdapter(sql, GetDbc(dbId)))
            {
                //不可為 null
                DataTable DT = new DataTable();

                DA.SelectCommand.CommandTimeout = timeout;

                DA.Fill(DT);

                return DT;
            }
        }

        //public static DataTable DtFromSql(string Sql, SqlTransaction tran, Int32 timeout = 30)
        //{
        //    if (tran == null)
        //    {
        //        throw new Exception("tran 不可為 null");
        //    }

        //    SqlCommand Cmd = new SqlCommand();
        //    Cmd.Transaction = tran;
        //    Cmd.Connection = tran.Connection;
        //    Cmd.CommandType = CommandType.Text;
        //    Cmd.CommandText = Sql;

        //    using (var DA = new SqlDataAdapter(Cmd))
        //    {
        //        //不可為 null
        //        DataTable DT = new DataTable();
        //        if (timeout > 0)
        //        {
        //            DA.SelectCommand.CommandTimeout = timeout;
        //        }

        //        DA.Fill(DT);

        //        return DT;
        //    }
        //}

        /// <summary>
        /// 找不到時回傳 null.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="DBC"></param>
        /// <returns></returns>
        public static object GetSingleValue(string sql, Sql.DbId dbId = null)
        {
            try
            {
                DataTable DT = DtFromSql(sql, dbId);
                if (DT.Rows.Count == 0)
                {
                    return null;
                }
                else
                {
                    return DT.Rows[0][0];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Su.SQL.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
            }
        }

        //public static object GetSingleValue(string sql, SqlTransaction tran)
        //{
        //    try
        //    {
        //        DataTable DT = DtFromSql(sql, tran);
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

        public static int GetIdentity(SqlConnection conn)
        {
            DataTable DT = DtFromSql("Select @@Identity", conn);
            return Convert.ToInt32(DT.Rows[0][0]);
        }

        //public static int GetIdentity(SqlTransaction tran)
        //{
        //    DataTable DT = DtFromSql("Select @@Identity", tran);
        //    return Convert.ToInt32(DT.Rows[0][0]);
        //}

        //        public static string SqlValue(object value, bool IsNotUnicode = false)
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
        //                        return "'" + value.ToString().SqlStr() + "'";
        //                    }
        //                    else
        //                    {
        //                        return "N'" + value.ToString().SqlStr() + "'";
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
        //                    throw new Exception("不認識的型別: " + value.GetType().ToString());
        //            }
        //        }

        //        public class FieldDef
        //        {
        //            /// <summary>
        //            /// SQL 的欄位定義名稱
        //            /// </summary>
        //            public string TypeName;
        //            public int Length;
        //            public string Description;
        //        }


        //        /// <summary>
        //        /// 未來考慮做一個 Cache 的版本
        //        /// </summary>
        //        /// <param name="TableName"></param>
        //        /// <param name="NewDBC"></param>
        //        /// <returns></returns>
        //        public static Dictionary<string, FieldDef> GetFieldDef(string TableName, string NewDBC = "")
        //        {
        //            var ltTableName = TableName.Split('.');
        //            //取最後一個字串為TableName
        //            TableName = ltTableName[ltTableName.Length - 1].Replace("[", "").Replace("]", "");

        //            var dicFields = new Dictionary<string, FieldDef>();

        //            var dtStructure = Su.MsSql.GetTableStructure(TableName, NewDBC);

        //            foreach (DataRow oRow in dtStructure.Rows)
        //            {
        //                FieldDef F = new FieldDef();
        //                F.TypeName = oRow["TypeName"].ToString();
        //                F.Length = Convert.ToInt32(oRow["length"]);
        //                F.Description = oRow["Description"].ToString();

        //                dicFields.Add(oRow["Column"].ToString(), F);
        //            }

        //            return dicFields;
        //        }

        //        public static Dictionary<string, Dictionary<string, FieldDef>> dicFieldDefCache = new Dictionary<string, Dictionary<string, FieldDef>>();

        //        /// <summary>
        //        /// 未來考慮做一個 Cache 的版本
        //        /// </summary>
        //        /// <param name="TableName"></param>
        //        /// <param name="NewDBC"></param>
        //        /// <returns></returns>
        //        public static Dictionary<string, FieldDef> GetFieldDef_WithCache(string TableName, string NewDBC = "")
        //        {
        //            var ltTableName = TableName.Split('.');
        //            //取最後一個字串為TableName
        //            TableName = ltTableName[ltTableName.Length - 1].Replace("[", "").Replace("]", "");

        //            if (dicFieldDefCache.ContainsKey(TableName))
        //            {
        //                return dicFieldDefCache[TableName];
        //            }

        //            lock (dicFieldDefCache)
        //            {
        //                if (dicFieldDefCache.ContainsKey(TableName))
        //                {
        //                    return dicFieldDefCache[TableName];
        //                }

        //                var dicFields = new Dictionary<string, FieldDef>();

        //                var dtStructure = Su.MsSql.GetTableStructure(TableName, NewDBC);

        //                foreach (DataRow oRow in dtStructure.Rows)
        //                {
        //                    FieldDef F = new FieldDef();
        //                    F.TypeName = oRow["TypeName"].ToString();
        //                    F.Length = Convert.ToInt32(oRow["length"]);
        //                    F.Description = oRow["Description"].ToString();

        //                    dicFields.Add(oRow["Column"].ToString(), F);
        //                }

        //                dicFieldDefCache.Add(TableName, dicFields);
        //                return dicFields;
        //            }
        //        }

        //        public static void ClearFieldDefCache()
        //        {
        //            dicFieldDefCache = new Dictionary<string, Dictionary<string, FieldDef>>();
        //        }


        //        /// <summary>
        //        /// 因為 DBC 會外洩, 所以要改為 internal
        //        /// </summary>
        //        /// <param name="DBC"></param>
        //        /// <returns></returns>
        //        internal static SqlConnection GetOpenedConnection(string DBC = null)
        //        {
        //            SqlConnection sqlConn = new SqlConnection(GetDBC(DBC));
        //            sqlConn.Open();
        //            return sqlConn;
        //        }

        //        /// <summary>
        //        /// 結尾的 _ 表示是 Local 的 function.
        //        /// </summary>
        //        /// <param name="sql"></param>
        //        /// <param name="timeout"></param>
        //        /// <returns></returns>
        //        public Int32 ExecuteSQL_(string sql, Int32 timeout = 0)
        //        {
        //            return SQL.ExecuteSQL(sql, _DBC, timeout);
        //        }

        //        /// <summary>
        //        /// 回傳影響的資料數量
        //        /// </summary>
        //        /// <param name="sql"></param>
        //        /// <param name="DBC"></param>
        //        /// <param name="timeout"></param>
        //        /// <param name="IsReturnIdentity"></param>
        //        /// <returns></returns>
        //        public static Int32 ExecuteSQL(string sql, string DBC = null, Int32 timeout = 0, bool IsReturnIdentity = false)
        //        {
        //            Www.Uc.AppendLog("........................................ExecuteSQL S................................................");
        //            Www.Uc.AppendLog(sql);
        //            Www.Uc.AppendLog("........................................ExecuteSQL E................................................");

        //            SqlConnection sqlConn;
        //            Int32 Res = 0;
        //            using (sqlConn = GetOpenedConnection(DBC))
        //            {
        //                SqlCommand Cmd = new SqlCommand();
        //                if (timeout > 0)
        //                {
        //                    Cmd.CommandTimeout = timeout;
        //                }
        //                Cmd.CommandText = sql;
        //                Cmd.CommandType = CommandType.Text;
        //                Cmd.Connection = sqlConn;

        //                Res = Cmd.ExecuteNonQuery();

        //                if (IsReturnIdentity == true)
        //                {
        //                    Res = SQL.GetIdentity(sqlConn);
        //                }

        //                sqlConn.Close();
        //            }
        //            return Res;
        //        }

        //        public static Int32 ExecuteSQL(string sql, SqlTransaction tran, Int32 timeout = 0, bool IsReturnIdentity = false)
        //        {
        //            int Res = 0;
        //            if (tran == null)
        //            {
        //                Res = ExecuteSQL(sql, timeout: timeout, IsReturnIdentity: IsReturnIdentity);
        //            }
        //            else
        //            {
        //                SqlCommand cmd = new SqlCommand();
        //                if (timeout > 0)
        //                {
        //                    cmd.CommandTimeout = timeout;
        //                }
        //                cmd.CommandText = sql;
        //                cmd.CommandType = CommandType.Text;
        //                cmd.Connection = tran.Connection;
        //                cmd.Transaction = tran;
        //                Res = cmd.ExecuteNonQuery();

        //                if (IsReturnIdentity == true)
        //                {
        //                    Res = SQL.GetIdentity(tran);
        //                }
        //            }
        //            return Res;
        //        }

        //        public static Int32 ExecuteSQL(string sql, SqlConnection sqlConn, Int32 timeout = 0, bool IsReturnIdentity = false)
        //        {
        //            SqlCommand Cmd = new SqlCommand();
        //            if (timeout > 0)
        //            {
        //                Cmd.CommandTimeout = timeout;
        //            }
        //            Cmd.CommandText = sql;
        //            Cmd.CommandType = CommandType.Text;
        //            Cmd.Connection = sqlConn;

        //            int Res = Cmd.ExecuteNonQuery();
        //            if (IsReturnIdentity == true)
        //            {
        //                //DataTable dt = Su.MsSql.DtFromSql("Select @@Identity", sqlConn);
        //                //Res = Convert.ToInt32(dt.Rows[0][0]);
        //                Res = SQL.GetIdentity(sqlConn);
        //            }

        //            return Res;
        //        }



        //        public static void InsertSysLog(int RID, string TableName, int MemberId, string OldContent, string NewContent, SqlTransaction tran = null)
        //        {
        //            if (NewContent is null)
        //            {
        //                NewContent = "";
        //            }

        //            "sys_log".SQL().InsertBuilder()
        //                            .Set("Member_Id", MemberId)
        //                            .Set("Table_Name", TableName)
        //                            .Set("RID", RID)
        //                            .Set("Content", OldContent)
        //                            .Set("New_Content", NewContent)
        //                            .Set("IP", Su.WU.ClientIP.SqlStr())
        //                            .Execute(tran);
        //        }

        //        //為了要把 GetOpenedConnection 變成 internal, 所以要實作以下介面給小三用.
        //        public static object ExecScalar(SqlCommand cmd)
        //        {
        //            using (var conn = GetOpenedConnection())
        //            {
        //                cmd.Connection = conn;
        //                return cmd.ExecuteScalar();
        //            }
        //        }

        //        public static int ExecQuery(SqlCommand cmd, DataSet ds)
        //        {
        //            using (var conn = GetOpenedConnection())
        //            {
        //                cmd.Connection = conn;
        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    return da.Fill(ds);
        //                }
        //            }
        //        }

        //        public static int ExecQuery(SqlCommand cmd, DataSet ds, int RecordStartIndex, int RecordCount, string TableName)
        //        {
        //            using (var conn = GetOpenedConnection())
        //            {
        //                cmd.Connection = conn;
        //                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //                {
        //                    return da.Fill(ds, RecordStartIndex, RecordCount, TableName);
        //                }
        //            }
        //        }

        //        public static int ExecNonQuery(SqlCommand cmd)
        //        {
        //            using (var conn = GetOpenedConnection())
        //            {
        //                cmd.Connection = conn;
        //                return cmd.ExecuteNonQuery();
        //            }
        //        }

        //        public static string strXMLFromDataRow(int RID, string TableName, SqlTransaction tran = null, string IdFieldName = "Id")
        //        {
        //            if (RID > 0)
        //            {
        //                var dt = TableName.SQL().Select().Where(IdFieldName + " = ", RID).DT(tran);
        //                return strXMLFromDT(dt);
        //            }
        //            else
        //            {
        //                throw new Exception("RID 未設定");
        //            }
        //        }

        //        public static string strXMLFromDT(DataTable dt, string LocalName = "Item", string OuterLocalName = "")
        //        {
        //            if (dt == null || dt.Rows.Count == 0) return "";

        //            var stbBuilder = new StringBuilder();
        //            var xtwWriter = new XmlTextWriter(new StringWriter(stbBuilder));

        //            if (OuterLocalName.Length > 0)
        //            {
        //                xtwWriter.WriteStartElement(OuterLocalName);
        //            }

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                xtwWriter.WriteStartElement(LocalName);
        //                foreach (DataColumn col in dt.Columns)
        //                {
        //                    var FN = col.ColumnName;
        //                    xtwWriter.WriteStartElement(FN);
        //                    xtwWriter.WriteStartAttribute("Type", "");
        //                    xtwWriter.WriteString(row[FN].GetType().ToString());
        //                    xtwWriter.WriteEndAttribute();
        //                    var value = "";
        //                    if (row[FN] == System.DBNull.Value)
        //                    {
        //                        value = "NULL";
        //                    }
        //                    else
        //                    {
        //                        value = row[FN].ToString();
        //                    }
        //                    xtwWriter.WriteString(value);
        //                    xtwWriter.WriteEndElement();
        //                }
        //                xtwWriter.WriteEndElement();
        //            }

        //            if (OuterLocalName.Length > 0)
        //            {
        //                xtwWriter.WriteEndElement();
        //            }

        //            xtwWriter.Flush();

        //            return stbBuilder.ToString();
        //        }

        //        public static string DTtoHTMLTable(DataTable DT, Hashtable ht欄位名稱對照表 = null, Hashtable htExcludeFields = null, bool IsTROnly = false)
        //        {
        //            string Res = "<tr>" + "\r\n";

        //            // Header
        //            foreach (DataColumn CL in DT.Columns)
        //            {
        //                if (htExcludeFields == null || !htExcludeFields.Contains(CL.ColumnName))
        //                {
        //                    if (ht欄位名稱對照表 != null && ht欄位名稱對照表.Contains(CL.ColumnName))
        //                        Res += "  <th>" + ht欄位名稱對照表[CL.ColumnName] + "</th>" + "\r\n";
        //                    else
        //                        Res += "  <th>" + CL.ColumnName + "</th>" + "\r\n";
        //                }
        //            }

        //            Res += "</tr>" + "\r\n";

        //            // 資料
        //            foreach (DataRow row in DT.Rows)
        //            {
        //                Res += "<tr>" + "\r\n";
        //                for (Int32 I = 0; I <= DT.Columns.Count - 1; I++)
        //                {
        //                    if (htExcludeFields == null || !htExcludeFields.Contains(DT.Columns[I].ColumnName))
        //                    {
        //                        string Text = "";
        //                        if (Convert.IsDBNull(row[I]) || row[I].ToString().Length == 0)
        //                            Text = "&nbsp;";
        //                        else if (row[I].GetType().ToString().ToLower() == "datetime")
        //                            Text = ((DateTime)row[I]).ToString("yyyy-MM-dd HH:mm:ss").Replace(" 00:00:00", "");
        //                        else
        //                            Text = row[I].ToString();
        //                        Res += "  <td>" + Text + "</td>" + "\r\n";
        //                    }
        //                }
        //                Res += "</tr>" + "\r\n";
        //            }

        //            if (! IsTROnly)
        //            {
        //                return "<table>" + Res + "</table>";
        //            }
        //            else
        //            {
        //                return Res;
        //            }
        //        }
    }
}
