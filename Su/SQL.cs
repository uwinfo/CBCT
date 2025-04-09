namespace Su.Sql
{
    /// <summary>
    /// 不要把 DbcNames 轉為數字。
    /// 這裡應該不用指定數字。
    /// </summary>
    public class DbId
    {
        /// <summary>
        /// 控制 Id 不可重覆
        /// </summary>
        public HashSet<int> allIds = new HashSet<int>();

        int _Id = -1;

        /// <summary>
        /// 建立 Cache 時會用到這個 Id
        /// </summary>
        public int Id
        {
            get
            {
                return _Id;
            }
        }

        public DbId(int id)
        {
            lock(Su.LockerProvider.GetLocker("Create DbId"))
            {
                if (allIds.Contains(id))
                {
                    throw new Exception("不可立重覆 Id 的 DbId");
                }

                allIds.Add(id);
            }

            this._Id = id;
        }
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Xml;

//namespace Su
//{
//    public enum SQLOP
//    {
//        /// <summary>
//        /// 相等
//        /// </summary>
//        EQ = 100,

//        /// <summary>
//        /// 大於
//        /// </summary>
//        BT = 200,

//        /// <summary>
//        /// 大於或等於
//        /// </summary>
//        BE = 300,

//        /// <summary>
//        /// 小於
//        /// </summary>
//        ST = 400,

//        /// <summary>
//        /// 小於或等於
//        /// </summary>
//        SE = 500
//    }

//    public partial class SQL
//    {

//        /// <summary>
//        /// 一但指定之後就不能更改.
//        /// </summary>
//        private readonly string _DBC = null;
//        /// <summary>
//        /// 指定 Default Sqlconnection string 的物件.
//        /// </summary>
//        /// <param name="DBC"></param>
//        public SQL(string DBC)
//        {
//            _DBC = DBC;
//        }

//        private readonly SqlConnection _Conn = null;
//        /// <summary>
//        /// 共用 SqlConnection 的物件. 這裡不會處理關閉 connetion 的動作, 要記得自已做.
//        /// </summary>
//        /// <param name="conn"></param>
//        public SQL(SqlConnection conn)
//        {
//            _Conn = conn;
//        }

//        /// <summary>
//        /// 不要直接使用 _DefaltDBC, 請使用 GetDBC();
//        /// 全域預設 DBC, 在 Global.asax 中設定, 優先權小於 DBC
//        /// </summary>
//        static string _DefaltDBC = null;

//        /// <summary>
//        /// 設定用的介面, 全域預設 DBC, 在 Global.asax 或 Startup.cs 的 Configure 中設定, 優先權小於 DBC, 只能設定一次.
//        /// 不能直接使用 DefaltDBC, 請使用 GetDBC();
//        /// </summary>
//        public static string DefaltDBC
//        {
//            set
//            {
//                if (_DefaltDBC == null)
//                {
//                    Su.WU.AddLog("Set DefaultDBC: " + value, "Startup");
//                    _DefaltDBC = value;
//                }
//                else
//                {
//                    throw new Exception("DefaltDBC 只能指定一次");
//                }
//            }
//        }

//        public static void ClearDefaltDBC()
//        {
//            _DefaltDBC = null;
//        }

//        private const string PageDBCKey = "Su.SQL.PageDBC";

//        /// <summary>
//        /// 在一個頁面中使用的 DBC, 設定用的介面. 讀取時請用 GetDBC()
//        /// </summary>
//        public static string PageDBC
//        {
//            set
//            {
//                if (Su.HttpContext.Current.Items[PageDBCKey] == null)
//                {
//                    Su.HttpContext.Current.Items[PageDBCKey] = value;
//                }
//                else
//                {
//                    throw new Exception("PageDBC 只能指定一次");
//                }
//            }
//        }

//        public static bool IsWriteSysLog = true;

//        public DataTable DTFromSQL_(string sql, Int32 timeout = 0)
//        {
//            if (_Conn != null)
//            {
//                return SQL.DtFromSql(sql, _Conn, timeout);
//            }

//            return SQL.DtFromSql(sql, GetDBC(_DBC), timeout);
//        }

//        public static DataTable DtFromSql(string Sql, SqlConnection conn, Int32 timeout = 0)
//        {
//            if (conn != null)
//            {
//                var DA = new SqlDataAdapter(Sql, conn);
//                if (timeout > 0)
//                {
//                    DA.SelectCommand.CommandTimeout = timeout;
//                }

//                DataTable DT = new DataTable();
//                DA.Fill(DT);
//                return DT;
//            }
//            else
//            {
//                return DtFromSql(Sql, GetDBC(), timeout);
//            }
//        }

//        internal static string GetDBC(string ExtraDBC = null)
//        {
//            if (!string.IsNullOrWhiteSpace(ExtraDBC))
//            {
//                return ExtraDBC;
//            }


//            if(Su.HttpContext.Current == null)
//            {
//                return _DefaltDBC;
//            }

//            if(Su.HttpContext.Current.Items == null)
//            {
//                return _DefaltDBC;
//                //throw new Exception("Su.HttpContext.Current.Items is nothing..");
//            }

//            if (Su.HttpContext.Current.Items[PageDBCKey] != null)
//            {
//                return (string)Su.HttpContext.Current.Items[PageDBCKey];
//            }

//            return _DefaltDBC;
//        }

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
//                throw new Exception("Su.SQL.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
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
//                throw new Exception("Su.SQL.GetSingleValue: \r\n" + ex.ToString() + "\r\n" + sql);
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

//        /// <summary>
//        /// 用 SQL 字串取得 DataTable
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="parameters">用 MsSqlValue() 取代 SQL 字串中的 {XXX}</param>
//        /// <param name="DBC"></param>
//        /// <param name="timeout"></param>
//        /// <param name="isCheckDangerSQL"></param>
//        /// <param name="sqlObjects">用 SqlObj() 取代 SQL 字串中的 [XXX]</param>
//        /// <returns></returns>
//        public static DataTable DtFromSql(string sql, object parameters, string DBC = null, Int32 timeout = 30, bool isCheckDangerSQL = true, object sqlObjects = null)
//        {
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
//                    sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(sqlObjects).ToString().SqlObj());
//                }
//            }

//            return DtFromSql(sql, DBC, timeout, isCheckDangerSQL);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="sql"></param>
//        /// <param name="DBC"></param>
//        /// <param name="timeout">微軟預設值 30 秒</param>
//        /// <param name="isCheckDangerSQL"></param>
//        /// <returns></returns>
//        public static DataTable DtFromSql(string sql, string DBC = null, Int32 timeout = 30, bool isCheckDangerSQL = true)
//        {
//            //Www.Uc.AppendLog(" \r\n" + sql + "\r\n ================================================================================================");

//            if (isCheckDangerSQL)
//            {
//                CheckDangerSQL(sql);
//            }

//            using (var DA = new SqlDataAdapter(sql, GetDBC(DBC)))
//            {
//                //不可為 null
//                DataTable DT = new DataTable();

//                DA.SelectCommand.CommandTimeout = timeout;

//                DA.Fill(DT);

//                return DT;
//            }
//        }

//        public static DataTable DtFromSql(string Sql, SqlTransaction tran, Int32 timeout = 30)
//        {
//            if (tran == null)
//            {
//                return DtFromSql(Sql, timeout: timeout);
//            }

//            SqlCommand Cmd = new SqlCommand();
//            Cmd.Transaction = tran;
//            Cmd.Connection = tran.Connection;
//            Cmd.CommandType = CommandType.Text;
//            Cmd.CommandText = Sql;

//            using (var DA = new SqlDataAdapter(Cmd))
//            {
//                //不可為 null
//                DataTable DT = new DataTable();
//                if (timeout > 0)
//                {
//                    DA.SelectCommand.CommandTimeout = timeout;
//                }

//                DA.Fill(DT);

//                return DT;
//            }
//        }

//        public static int GetIdentity(SqlConnection conn)
//        {
//            DataTable DT = SQL.DtFromSql("Select @@Identity", conn);
//            return Convert.ToInt32(DT.Rows[0][0]);
//        }

//        public static int GetIdentity(SqlTransaction tran)
//        {
//            DataTable DT = SQL.DtFromSql("Select @@Identity", tran);
//            return Convert.ToInt32(DT.Rows[0][0]);
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

//            var dtStructure = Su.SQL.GetTableStructure(TableName, NewDBC);

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

//                var dtStructure = Su.SQL.GetTableStructure(TableName, NewDBC);

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
//                //DataTable dt = Su.SQL.DtFromSql("Select @@Identity", sqlConn);
//                //Res = Convert.ToInt32(dt.Rows[0][0]);
//                Res = SQL.GetIdentity(sqlConn);
//            }

//            return Res;
//        }

        

//        public static void InsertSysLog(int RID, string TableName, int MemberId, string OldContent, string NewContent)
//        {
//            if (NewContent is null)
//            {
//                NewContent = "";
//            }

//            "sys_log".MsSqlBuilder()
//                .Set("Member_Id", MemberId)
//                .Set("Table_Name", TableName)
//                .Set("RID", RID)
//                .Set("Content", OldContent)
//                .Set("New_Content", NewContent)
//                .Set("IP", Su.WU.ClientIP)
//                .Insert();
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

//        //public static string strXMLFromDataRow(int RID, string TableName, SqlTransaction tran = null, string IdFieldName = "Id")
//        //{
//        //    if (RID > 0)
//        //    {
//        //        var dt = TableName.SQL().Select().Where(IdFieldName + " = ", RID).DT(tran);
//        //        return strXMLFromDT(dt);
//        //    }
//        //    else
//        //    {
//        //        throw new Exception("RID 未設定");
//        //    }
//        //}

//        //public static string strXMLFromDT(DataTable dt, string LocalName = "Item", string OuterLocalName = "")
//        //{
//        //    if (dt == null || dt.Rows.Count == 0) return "";

//        //    var stbBuilder = new StringBuilder();
//        //    var xtwWriter = new XmlTextWriter(new StringWriter(stbBuilder));

//        //    if (OuterLocalName.Length > 0)
//        //    {
//        //        xtwWriter.WriteStartElement(OuterLocalName);
//        //    }

//        //    foreach (DataRow row in dt.Rows)
//        //    {
//        //        xtwWriter.WriteStartElement(LocalName);
//        //        foreach (DataColumn col in dt.Columns)
//        //        {
//        //            var FN = col.ColumnName;
//        //            xtwWriter.WriteStartElement(FN);
//        //            xtwWriter.WriteStartAttribute("Type", "");
//        //            xtwWriter.WriteString(row[FN].GetType().ToString());
//        //            xtwWriter.WriteEndAttribute();
//        //            var value = "";
//        //            if (row[FN] == System.DBNull.Value)
//        //            {
//        //                value = "NULL";
//        //            }
//        //            else
//        //            {
//        //                value = row[FN].ToString();
//        //            }
//        //            xtwWriter.WriteString(value);
//        //            xtwWriter.WriteEndElement();
//        //        }
//        //        xtwWriter.WriteEndElement();
//        //    }

//        //    if (OuterLocalName.Length > 0)
//        //    {
//        //        xtwWriter.WriteEndElement();
//        //    }

//        //    xtwWriter.Flush();

//        //    return stbBuilder.ToString();
//        //}

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
//    }
//}
