//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Data;
//using System.Data.SqlClient;
//using System.Reflection;
//using System.Runtime.CompilerServices;

//namespace Su
//{
//    public partial class SQL
//    {
//        public class Field
//        {
//            public enum Type
//            {
//                null_ = -1,
//                str_ = 0,
//                int32_ = 10,
//                int64_ = 11,
//                single_ = 20,
//                double_ = 30,
//                date_ = 40,
//                decimal_ = 50,
//                function_ = 999
//            }

//            public bool isAppend
//            {
//                get
//                {
//                    if (_name.Trim().EndsWith(" +="))
//                    {
//                        return true;
//                    }
//                    return false;
//                }
//            }

//            string _name;

//            /// <summary>
//            /// 讀出時, 會用 sql object 的型式.
//            /// </summary>
//            public string Name
//            {
//                get
//                {
//                    return _name.SqlObj();
//                }
//                set
//                {
//                    _name = value;
//                }
//            }

//            public string SqlFieldName
//            {
//                get
//                {
//                    return _name.SqlObj();
//                }
//            }

//            public Type type = Type.str_;
//            public object value;

//            /// <summary>
//            /// 字串前是否要加上 N
//            /// </summary>
//            bool IsNotUnicode = false;

//            //public string SqlValue
//            //{
//            //    get
//            //    {
//            //        if (value == null)
//            //        {
//            //            return "null";
//            //        }

//            //        switch (type)
//            //        {
//            //            case Type.str_:
//            //                if (IsNotUnicode)
//            //                {
//            //                    return "'" + value.ToString().SqlStr() + "'";
//            //                }
//            //                else
//            //                {
//            //                    return "N'" + value.ToString().SqlStr() + "'";
//            //                }

//            //            case Type.int32_:
//            //                return ((int)value).ToString();

//            //            case Type.int64_:
//            //                return ((Int64)value).ToString();

//            //            case Type.single_:
//            //                return ((Single)value).ToString();

//            //            case Type.double_:
//            //                return ((double)value).ToString();

//            //            case Type.decimal_:
//            //                return ((decimal)value).ToString();

//            //            case Type.date_:
//            //                return "'" + ((System.DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";

//            //            case Type.function_:
//            //                if (value.ToString().IsSQLInjection())
//            //                {
//            //                    throw new Exception("以下語法, 有可能發生 SQL Injection: " + SqlFieldName + " = " + value.ToString());
//            //                }

//            //                //用 () 包起來, 增加 SQL Injection 的難度.
//            //                return "(" + value.ToString() + ")";

//            //            default:尸
//            //                throw new Exception("不認識的類別: " + type.ToString());
//            //        }
//            //    }
//            //}

//            /// <summary>
//            /// 指定型別
//            /// </summary>
//            /// <param name="name"></param>
//            /// <param name="value"></param>
//            /// <param name="type"></param>
//            /// <param name="IsNotUnicode"></param>
//            public Field(string name, object value, Type type, bool IsNotUnicode = false)
//            {
//                this.Name = name;
//                this.value = value;
//                this.Type = type;
//                this.IsNotUnicode = IsNotUnicode;
//            }

//            /// <summary>
//            /// 自動決定 Type
//            /// </summary>
//            /// <param name="name"></param>
//            /// <param name="value"></param>
//            /// <param name="IsNotUnicode"></param>
//            public Field(string name, object value, bool IsNotUnicode = false)
//            {
//                this.IsNotUnicode = IsNotUnicode;

//                Name = name.Trim();
//                //會有.Set("fields +=", "AAA")的情形，要略過, Reiko, 2019/03/15
//                if (name.EndsWith("=") && !name.EndsWith("+="))
//                {
//                    Name = name.Substring(0, name.Length - 1);
//                    Name = name.Trim();
//                }
//                this.Name = name;
//                this.value = value;

//                if (value == null)
//                {
//                    this.Type = Type.null_;
//                }
//                else
//                {
//                    switch (value.GetType().ToString())
//                    {
//                        case "System.Char":
//                        case "System.String":
//                            Type = Type.str_;
//                            break;
//                        case "System.Int32":
//                            Type = Type.int32_;
//                            break;
//                        case "System.Int64":
//                            Type = Type.int64_;
//                            break;
//                        case "System.DateTime":
//                            Type = Type.date_;
//                            break;
//                        case "System.Single":
//                            Type = Type.single_;
//                            break;
//                        case "System.Double":
//                            Type = Type.double_;
//                            break;
//                        case "System.Decimal":
//                            Type = Type.decimal_;
//                            break;
//                        default:
//                            throw new Exception("不認識的型別: " + value.GetType().ToString());
//                    }
//                }
//            }
//        }

//        abstract public class BaseSQLBuilder
//        {
//            public string tableName;
//            public List<Field> ltFields = new List<Field>();
//            public List<string> ltUpdateCommands = new List<string>();
//            public Criteria criteria = new Criteria();

//            protected string _terminator = ";\r\n";
//            /// <summary>
//            /// 設定結束符號, 預設為 ";\r\n"
//            /// </summary>
//            /// <param name="terminator"></param>
//            /// <returns></returns>
//            public BaseSQLBuilder SetEnd(string terminator)
//            {
//                _terminator = terminator;
//                return this;
//            }

//            public abstract override string ToString();

//            public abstract string Sql
//            {
//                get;
//            }

//            protected bool _IsNoCriteria = false;
//            /// <summary>
//            /// 設定這個 Update 是沒有條件的. 用來防呆, 避免忘了設定 Criteria, Update 和 Insert 會用到
//            /// </summary>
//            /// <returns></returns>
//            public BaseSQLBuilder SetNoCriteria()
//            {
//                _IsNoCriteria = true;
//                return this;
//            }

//            protected DataTable _dtStructure;

//            protected void _GetStructure()
//            {
//                _dicFields = new Dictionary<string, FieldDef>();

//                _dtStructure = Su.SQL.GetTableStructure(this.tableName);

//                //int
//                //nvarchar
//                //datetime
//                //datetime
//                //int
//                //char

//                foreach (DataRow oRow in _dtStructure.Rows)
//                {
//                    FieldDef F = new FieldDef();
//                    F.TypeName = oRow["TypeName"].ToString();
//                    F.Length = Convert.ToInt32(oRow["length"]);

//                    _dicFields.Add(oRow["Column"].ToString(), F);
//                }
//            }

//            Dictionary<string, FieldDef> _dicFields = null;

//            public Dictionary<string, FieldDef> dicFields
//            {
//                get
//                {
//                    if (_dicFields == null)
//                    {
//                        _GetStructure();
//                    }

//                    return _dicFields;
//                }
//            }

//            /// <summary>
//            /// 回傳運算元, 注意, 回傳值前方會加空白.
//            /// </summary>
//            /// <param name="sQLOP"></param>
//            /// <returns></returns>
//            protected string Operand(SQLOP sQLOP)
//            {
//                switch (sQLOP)
//                {
//                    case SQLOP.BE:
//                        return " >=";

//                    case SQLOP.BT:
//                        return " >";

//                    case SQLOP.EQ:
//                        return " =";

//                    case SQLOP.SE:
//                        return " <=";

//                    case SQLOP.ST:
//                        return " <";

//                }

//                throw new Exception("無法識別的運算元: " + sQLOP.ToString());
//            }

//            protected BaseSQLBuilder _SetAll(int UpdaterId = -1, int CreatorId = -1)
//            {
//                if (dicFields == null)
//                {
//                    _GetStructure();
//                }

//                foreach (string FieldName in this.dicFields.Keys)
//                {
//                    if (FieldName.ToLower() != "id")
//                    {
//                        _Set(FieldName);
//                    }
//                }

//                if(UpdaterId >= 0 && dicFields.ContainsKey("UpdaterId"))
//                {
//                    this.ltFields.Add(new Field("UpdaterId", UpdaterId));
//                }

//                if (CreatorId >= 0 && dicFields.ContainsKey("CreatorId"))
//                {
//                    this.ltFields.Add(new Field("CreatorId", CreatorId));
//                }

//                return this;
//            }

//            /// <summary>
//            /// 更新所有欄位, 從 form 中讀取資料. 這裡不做資料檢查, 若是傳入空白, 非字串型別的會被填入 null, 字串型別會填入空字串
//            /// </summary>
//            /// <param name="UpdaterId">會更新 UpdaterId 或 ModifierId, 若是有 ModifierIp 也會一併更新</param>
//            /// <returns></returns>
//            protected BaseSQLBuilder _SetAllWithNull(int UpdaterId = -1)
//            {
//                if (dicFields == null)
//                {
//                    _GetStructure();
//                }

//                foreach (string FieldName in this.dicFields.Keys)
//                {
//                    if (FieldName.ToLower() != "id")
//                    {
//                        _SetWithNull(FieldName);
//                    }
//                }

//                if(UpdaterId > -1)
//                {
//                    if (dicFields.ContainsKey("UpdaterId"))
//                    {
//                        this.ltFields.Add(new Field("UpdaterId", UpdaterId));
//                    }

//                    if (dicFields.ContainsKey("ModifierId"))
//                    {
//                        this.ltFields.Add(new Field("ModifierId", UpdaterId));
//                    }

//                    if (dicFields.ContainsKey("ModifierIp"))
//                    {
//                        this.ltFields.Add(new Field("ModifierIp", WU.ClientIP));
//                    }
//                }

//                return this;
//            }

//            private string __PostFix = "";
//            protected string _PostFix
//            {
//                set
//                {
//                    __PostFix = value;
//                }
//                get
//                {
//                    return __PostFix;
//                }
//            }

//            /// <summary>
//            /// 從 form 中讀取資料. 這裡不做資料檢查, 若是傳入空白, 非字串型別的會被填入 null, 字串型別會填入空字串
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            protected BaseSQLBuilder _SetWithNull(string FieldName)
//            {
//                string Value = WU.GetFormValue(FieldName + _PostFix, IsReturnNothing: true);

//                if (!dicFields.ContainsKey(FieldName))
//                {
//                    throw new Exception("找不到欄位: " + FieldName);
//                }

//                if (Value != null)
//                {
//                    var F = dicFields[FieldName];
//                    switch (F.TypeName)
//                    {
//                        case "bigint":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToInt64(Value)));
//                            }
//                            else
//                            {
//                                this.ltFields.Add(new Field(FieldName, null));
//                            }
//                            break;
//                        case "int":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToInt32(Value)));
//                            }
//                            else
//                            {
//                                this.ltFields.Add(new Field(FieldName, null));
//                            }
//                            break;
//                        case "numeric":
//                        case "decimal":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToDecimal(Value)));
//                            }
//                            else
//                            {
//                                this.ltFields.Add(new Field(FieldName, null));
//                            }
//                            break;
//                        case "real":
//                        case "float":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToSingle(Value)));
//                            }
//                            else
//                            {
//                                this.ltFields.Add(new Field(FieldName, null));
//                            }
//                            break;
//                        case "nvarchar":
//                        case "varchar":
//                        case "char":
//                        case "nchar":
//                            this.ltFields.Add(new Field(FieldName, Value));
//                            break;
//                        case "date":
//                        case "datetime":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, DateTime.Parse(Value)));
//                            }
//                            else
//                            {
//                                this.ltFields.Add(new Field(FieldName, null));
//                            }
//                            break;
//                    }
//                }
//                return this;
//            }

//            /// <summary>
//            /// 從 form 中讀取資料. 這裡不做資料檢查
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            protected BaseSQLBuilder _Set(string FieldName)
//            {
//                string Value = WU.GetFormValue(FieldName + _PostFix, IsReturnNothing: true);

//                if (Value != null)
//                {
//                    if (!dicFields.ContainsKey(FieldName))
//                    {
//                        throw new Exception("找不到欄位: " + FieldName);
//                    }

//                    var F = dicFields[FieldName];
//                    switch (F.TypeName)
//                    {
//                        case "bigint":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToInt64(Value)));
//                            }
//                            break;
//                        case "int":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToInt32(Value)));
//                            }
//                            break;
//                        case "numeric":
//                        case "decimal":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToDecimal(Value)));
//                            }
//                            break;
//                        case "real":
//                        case "float":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, Convert.ToSingle(Value)));
//                            }
//                            break;
//                        case "nvarchar":
//                        case "varchar":
//                        case "char":
//                        case "nchar":
//                            this.ltFields.Add(new Field(FieldName, Value));
//                            break;
//                        case "date":
//                        case "datetime":
//                            if (Value != "")
//                            {
//                                this.ltFields.Add(new Field(FieldName, DateTime.Parse(Value)));
//                            }
//                            break;
//                    }
//                }

//                return this;
//            }
//        }

//        public class DeleteSQLBuilder : BaseSQLBuilder
//        {
//            public DeleteSQLBuilder(string tableName)
//            {
//                this.tableName = tableName;
//            }

//            public DeleteSQLBuilder And(string FieldAndOP, object Value)
//            {
//                criteria.And(FieldAndOP, Value);

//                return this;
//            }

//            /// <summary>
//            /// And 的同義 function
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <returns></returns>
//            public DeleteSQLBuilder Where(string FieldAndOP, object Value)
//            {
//                return this.And(FieldAndOP, Value);
//            }

//            /// <summary>
//            /// 沒有條件的 Update, 要叫用 SetNoCriteria
//            /// </summary>
//            /// <returns></returns>
//            public override string ToString()
//            {
//                return Sql;
//            }

//            public override string Sql
//            {
//                get
//                {
//                    string Where = criteria.Where();

//                    if (!_IsNoCriteria && (Where == null || Where == ""))
//                    {
//                        throw new Exception("為避免意外 Update, 無條件更新請先叫用 SetNoCriteria()");
//                    }

//                    return " Delete " + tableName.SqlObj() + " " + Where + _terminator;
//                }
//            }

//            public int Execute(string DBC = null, Int32 timeout = 0)
//            {
//                return SQL.ExecuteSQL(Sql, DBC, timeout);
//            }

//            public int Execute(SqlTransaction tran, Int32 timeout = 0)
//            {
//                return SQL.ExecuteSQL(Sql, tran, timeout);
//            }
//        }

//        public static void UpdateTableExtendedProperty(string TableName, string PropertyName, string Value, string DBC = null)
//        {
//            //更新 Property.
//            DataTable dt = Su.SQL.DtFromSql("Select (Select value from sys.extended_properties Where sys.extended_properties.major_id = sys.tables.object_id and Name = '" + PropertyName.Replace("'", "''") + "' and minor_id = 0) as [Description] from sys.tables Where Type ='U' and Name = '" + TableName.Replace("'", "''") + "'", DBC: DBC);

//            var sp = "";
//            if (dt.Rows.Count == 0 || Convert.IsDBNull(dt.Rows[0]["Description"]))
//            {
//                if (string.IsNullOrEmpty(Value))
//                {
//                    return;
//                }
//                sp = "sp_addextendedproperty";
//            }
//            else
//            {
//                if(dt.Rows[0]["Description"].ToString() == Value)
//                {
//                    return;
//                }
//                sp = "sp_updateextendedproperty";
//            }

//            string sql = "exec " + sp + " '" + PropertyName.Replace("'", "''") + "', '" + Value.Replace("'", "''").Trim() + "', 'user', 'dbo', 'table', '" + TableName.Replace("'", "''") + "'";

//            Su.SQL.ExecuteSQL(sql, DBC: DBC);
//        }

//        public static void UpdateColumnDescription(string tableName, string columnName, string value, string DBC = null)
//        {
//            // 更新 Property.
//            string sql = "Select Value FROM fn_listextendedproperty (NULL, 'schema', 'dbo', 'table', '" + tableName.MsSqlStr() + "', 'column', default) Where objname = '" + columnName.Replace("'", "''") + "' and Name = 'MS_Description'";
//            DataTable dt = Su.SQL.DtFromSql(sql, DBC);

//            var sp = "";
//            if (dt.Rows.Count == 0 || Convert.IsDBNull(dt.Rows[0]["Value"]))
//            {
//                if (string.IsNullOrEmpty(value))
//                {
//                    return;
//                }
//                sp = "sp_addextendedproperty";
//            }
//            else
//            {
//                if (dt.Rows[0]["Value"].ToString() == value)
//                {
//                    return;
//                }
//                sp = "sp_updateextendedproperty";
//            }

//            sql = "exec " + sp + " 'MS_Description', '" + value.Trim().MsSqlStr() + "', 'user', 'dbo', 'table', '" + tableName.MsSqlStr() + "', 'column', '" + columnName.MsSqlStr() + "'";
            
//            Su.SQL.ExecuteSQL(sql);
//        }

//        public class UpdateSQLBuilder : BaseSQLBuilder
//        {
//            /// <summary>
//            /// tableName 不可包含 [ 或 ] 符號
//            /// </summary>
//            /// <param name="tableName"></param>
//            public UpdateSQLBuilder(string tableName)
//            {
//                this.tableName = tableName;
//            }

//            public UpdateSQLBuilder GetTableStructure()
//            {
//                this._GetStructure();
//                return this;
//            }

//            public UpdateSQLBuilder And(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP, Value, IsNotUnicode);

//                return this;
//            }


//            public UpdateSQLBuilder And(Criteria oC)
//            {
//                criteria.And(oC);

//                return this;
//            }

//            /// <summary>
//            /// 相等, criteria.And(FieldAndOP + " =", Value), value 為 null 時, 會自動用 is null
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder And_EQ(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " =", Value, IsNotUnicode);
//                return this;
//            }

            

//            /// <summary>
//            /// 三個變數的 And
//            /// </summary>
//            /// <param name="Field"></param>
//            /// <param name="sQLOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder A3(string Field, SQLOP sQLOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(Field + Operand(sQLOP), Value, IsNotUnicode);
//                return this;
//            }

//            /// <summary>
//            /// 直接寫 Criteria.
//            /// </summary>
//            /// <param name="Criteria"></param>
//            /// <param name="IsCheckInection"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder AndCriteria(string Criteria, bool IsCheckInection = true)
//            {
//                criteria.And(Criteria, IsCheckInection);

//                return this;
//            }

//            public UpdateSQLBuilder AndIsNull(string FieldName)
//            {
//                criteria.AndIsNull(FieldName);

//                return this;
//            }

//            /// <summary>
//            /// And 的同義 function
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder Where(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                return this.And(FieldAndOP, Value, IsNotUnicode);
//            }

//            public UpdateSQLBuilder Where_EQ(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                return this.And_EQ(FieldAndOP, Value, IsNotUnicode);
//            }


//            public UpdateSQLBuilder Set(Field F)
//            {
//                this.ltFields.Add(F);

//                return this;
//            }

//            /// <summary>
//            /// 寫入 sys_log 時使用的 UserId
//            /// </summary>
//            public int ModifierId;

//            /// <summary>
//            /// 設定 ModifierId
//            /// </summary>
//            /// <param name="UserId"></param>
//            /// <param name="ModifierIdFieldName">預設為 ModifierId, 不可為空白</param>
//            /// <param name="ModifyDateFieldName">預設為 ModifyDate, 可改為空白, 表示不設定</param>
//            /// <param name="ModifierIpFieldName">預設為 ModifierIp, 可改為空白, 表示不設定</param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetUser(Int32 UserId,
//                string ModifierIdFieldName = "ModifierId",
//                string ModifyDateFieldName = "ModifyDate",
//                string ModifierIpFieldName = "ModifierIp")
//            {
//                this.ModifierId = UserId;

//                this.ltFields.Add(new Field(ModifierIdFieldName, UserId));

//                if (ModifyDateFieldName != "")
//                {
//                    this.ltFields.Add(new Field(ModifyDateFieldName, "GetDate()", Field.Type.function_));
//                }

//                if (ModifierIpFieldName != "")
//                {
//                    this.ltFields.Add(new Field(ModifierIpFieldName, WU.ClientIP));
//                }

//                return this;
//            }

//            /// <summary>
//            /// 自動找是否有以下欄位(注意大小寫), 並更新之 ModifierId, ModifyDate, ModifierIp, UpdaterId, UpdateDate, UpdateIp
//            /// </summary>
//            /// <param name="UserId"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetUser_Auto(Int32 UserId)
//            {
//                this.ModifierId = UserId;

//                if (this.dicFields.ContainsKey("ModifierId"))
//                {
//                    this.ltFields.Add(new Field("ModifierId", UserId));
//                }

//                if (this.dicFields.ContainsKey("UpdaterId"))
//                {
//                    this.ltFields.Add(new Field("UpdaterId", UserId));
//                }

//                if (this.dicFields.ContainsKey("ModifyDate"))
//                {
//                    this.ltFields.Add(new Field("ModifyDate", "GetDate()", Field.Type.function_));
//                }

//                if (this.dicFields.ContainsKey("UpdateDate"))
//                {
//                    this.ltFields.Add(new Field("UpdateDate", "GetDate()", Field.Type.function_));
//                }

//                if (this.dicFields.ContainsKey("ModifierIp"))
//                {
//                    this.ltFields.Add(new Field("ModifierIp", WU.ClientIP));
//                }

//                if (this.dicFields.ContainsKey("UpdateIp"))
//                {
//                    this.ltFields.Add(new Field("UpdateIp", WU.ClientIP));
//                }

//                return this;
//            }

//            public int UpdateId;
//            /// <summary>
//            /// 等同 Where("Id = ", EditId)，但會記錄更新的Id
//            /// </summary>
//            /// <param name="EditId"></param>
//            /// <param name="IdFieldName"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetId(int EditId, string IdFieldName = "Id")
//            {
//                this.UpdateId = EditId;

//                return this.And(IdFieldName + " = ", EditId);
//            }

//            /// <summary>
//            /// 直接用 sql 來更新欄位, 只會做簡單的 SQL Injection 檢查, 盡量避免與外部傳入值混用.
//            /// </summary>
//            /// <param name="SQL"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetSQL(string SQL)
//            {
//                return SetCommand(SQL);
//            }

//            /// <summary>
//            /// 直接用 Command 來更新欄位, 只會做簡單的 SQL Injection 檢查, 盡量避免與外部傳入值混用.
//            /// </summary>
//            /// <param name="Command"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetCommand(string Command)
//            {
//                if (Command.IsSQLInjection())
//                {
//                    throw new Exception("以下指令可能會有 SQL Injection: " + Command);
//                }

//                this.ltUpdateCommands.Add(Command);

//                return this;
//            }



//            /// <summary>
//            /// 等於 Set(FieldName, null)
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetNull(string FieldName)
//            {
//                this.ltFields.Add(new Field(FieldName, null));
//                return this;
//            }

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="FieldName">欄位可用 = 或 += </param>
//            /// <param name="Value"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder Set(string FieldName, object Value)
//            {
//                this.ltFields.Add(new Field(FieldName.Trim(), Value));
//                return this;
//            }

//            public UpdateSQLBuilder Set(string FieldName, object Value, Field.Type type)
//            {
//                this.ltFields.Add(new Field(FieldName, Value, type));
//                return this;
//            }

//            /// <summary>
//            /// 從 form 中讀取資料. 這裡不做資料檢查
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder Set(string FieldName)
//            {
//                _Set(FieldName);
//                return this;
//            }

//            public UpdateSQLBuilder SetAll()
//            {
//                _SetAll();
//                return this;
//            }

//            /// <summary>
//            /// 更新所有欄位, 從 form 中讀取資料. 這裡不做資料檢查, 若是傳入空白, 非字串型別的會被填入 null, 字串型別會填入空字串
//            /// </summary>
//            /// <param name="UpdaterId">會更新 UpdaterId 或 ModifierId, 若是有 ModifierIp 也會一併更新</param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetAllWithNull(int UpdaterId = -1)
//            {
//                _SetAllWithNull(UpdaterId);
//                return this;
//            }

//            public int SetAllAndExecute()
//            {
//                _SetAll();
//                return this.Execute();
//            }

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetGetDate(string FieldName)
//            {
//                this.ltFields.Add(new Field(FieldName, "GetDate()", Field.Type.function_));
//                return this;
//            }

//            public override string Sql
//            {
//                get
//                {
//                    if ((ltFields.Count + ltUpdateCommands.Count) == 0)
//                    {
//                        throw new Exception("沒有指定欄位");
//                    }

//                    string Where = criteria.Where();

//                    if (!_IsNoCriteria && (Where == null || Where == ""))
//                    {
//                        throw new Exception("為避免意外 Update, 無條件更新請先叫用 SetNoCriteria()");
//                    }

//                    string UpdatedFields = "";
//                    foreach (Field F in ltFields)
//                    {
//                        if (F.isAppend)
//                        {
//                            switch (F.Type)
//                            {
//                                case Field.Type.str_:
//                                    UpdatedFields += "," + F.SqlFieldName + " = IsNull(" + F.SqlFieldName + ", '') + " + F.SqlValue;
//                                    break;

//                                case Field.Type.int32_:
//                                case Field.Type.int64_:
//                                case Field.Type.single_:
//                                case Field.Type.double_:
//                                case Field.Type.decimal_:
//                                    UpdatedFields += "," + F.SqlFieldName + " = IsNull(" + F.SqlFieldName + ", 0) + " + F.SqlValue;
//                                    break;

//                                default:
//                                    throw new Exception("不可使用+=, " + F.Type.ToString());
//                            }
//                        }
//                        else
//                        {
//                            UpdatedFields += "," + F.SqlFieldName + " = " + F.SqlValue;
//                        }
//                    }

//                    foreach (string C in ltUpdateCommands)
//                    {
//                        UpdatedFields += "," + C;
//                    }

//                    return " Update " + tableName.SqlObj() + " Set " + UpdatedFields.Substring(1) + " " + Where + _terminator;
//                }
//            }


//            /// <summary>
//            /// 沒有條件的 Update, 要叫用 SetNoCriteria
//            /// </summary>
//            /// <returns></returns>
//            public override string ToString()
//            {
//                return Sql;
//            }

//            public int Execute(string DBC = null, Int32 timeout = 0, bool IsNoCriteria = false)
//            {
//                if (IsNoCriteria)
//                {
//                    SetNoCriteria();
//                }

//                //若是沒有 Where 條件, 在產生 sql 時會有 exception.
//                return SQL.ExecuteSQL(Sql, DBC, timeout);
//            }

//            public int Execute(SqlConnection conn, Int32 timeout = 0, bool IsNoCriteria = false)
//            {
//                if (IsNoCriteria)
//                {
//                    SetNoCriteria();
//                }

//                //若是沒有 Where 條件, 在產生 sql 時會有 exception.
//                return SQL.ExecuteSQL(Sql, conn, timeout);
//            }
//            public int Execute(SqlTransaction tran, Int32 timeout = 0, bool IsNoCriteria = false,
//                                bool Is_SkipSysLog = false, string IdFieldName = "Id")
//            {
//                if (IsNoCriteria)
//                {
//                    SetNoCriteria();
//                }

//                var OldContent = "";
//                if (SQL.IsWriteSysLog && (!Is_SkipSysLog))
//                {
//                    OldContent = SQL.strXMLFromDataRow(this.UpdateId, this.tableName, tran, IdFieldName);
//                }

//                //若是沒有 Where 條件, 在產生 sql 時會有 exception.
//                var res = -1;
//                try
//                {
//                    res = SQL.ExecuteSQL(Sql, tran, timeout);
//                }
//                catch (Exception ex)
//                {
//                    WU.WriteText(Sql);
//                    WU.WriteText("<br>" + ex.ToString());

//                    throw new Exception(ex.ToString() + ", " + ex.StackTrace);
//                }

//                //記錄 Log
//                if (SQL.IsWriteSysLog && (!Is_SkipSysLog))
//                {
//                    var NewContent = SQL.strXMLFromDataRow(this.UpdateId, this.tableName, tran, IdFieldName);

//                    SQL.InsertSysLog(this.UpdateId, this.tableName, this.ModifierId, OldContent, NewContent, tran);
//                }

//                return res;
//            }

//            /// <summary>
//            /// 給 SetF 使用
//            /// </summary>
//            /// <param name="PostFix"></param>
//            /// <returns></returns>
//            public UpdateSQLBuilder SetPostFix(string PostFix)
//            {
//                _PostFix = PostFix;
//                return this;
//            }
//        }

//        public class InsertSQLBuilder : BaseSQLBuilder
//        {
//            /// <summary>
//            /// tableName 不可包含 [ 或 ] 符號
//            /// </summary>
//            /// <param name="tableName"></param>
//            public InsertSQLBuilder(string tableName)
//            {
//                this.tableName = tableName;
//            }

//            public InsertSQLBuilder Set(Field F)
//            {
//                this.ltFields.Add(F);
//                return this;
//            }

//            public InsertSQLBuilder GetTableStructure()
//            {
//                this._GetStructure();
//                return this;
//            }

//            /// <summary>
//            /// 設定 CreatorId, 和 ModifierId
//            /// </summary>
//            /// <param name="UserId"></param>
//            /// <param name="CreatorIdFieldName">預設為 CreatorId, 可改為空白, 表示不設定</param>
//            /// <param name="ModifierIdFieldName">預設為 ModifierId, 可改為空白, 表示不設定</param>
//            /// <param name="CreatorIpFieldName">預設為 CreatorIp, 可改為空白, 表示不設定</param>
//            /// <param name="ModifierIpFieldName">預設為 ModifierIp, 可改為空白, 表示不設定</param>
//            /// <returns></returns>
//            public InsertSQLBuilder SetUser(Int32 UserId,
//                string CreatorIdFieldName = "CreatorId",
//                string ModifierIdFieldName = "ModifierId",
//                string CreatorIpFieldName = "CreatorIp",
//                string ModifierIpFieldName = "ModifierIp")
//            {
//                if (CreatorIdFieldName != "")
//                {
//                    this.ltFields.Add(new Field(CreatorIdFieldName, UserId));
//                }

//                if (ModifierIdFieldName != "")
//                {
//                    this.ltFields.Add(new Field(ModifierIdFieldName, UserId));
//                }

//                if (CreatorIpFieldName != "")
//                {
//                    this.ltFields.Add(new Field(CreatorIpFieldName, WU.ClientIP));
//                }

//                if (ModifierIpFieldName != "")
//                {
//                    this.ltFields.Add(new Field(ModifierIpFieldName, WU.ClientIP));
//                }

//                return this;
//            }

//            public InsertSQLBuilder Set(string FieldName, object Value, bool IsNotUnicode = false)
//            {
//                this.ltFields.Add(new Field(FieldName, Value, IsNotUnicode));
//                return this;
//            }

//            /// <summary>
//            /// 避免無法判斷 Set(object, strin) 和 Set(string, object), 所以加了一個 Set(string, string)
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public InsertSQLBuilder Set(string FieldName, string Value, bool IsNotUnicode = false)
//            {
//                this.ltFields.Add(new Field(FieldName, Value, IsNotUnicode));
//                return this;
//            }

//            /// <summary>
//            /// 若是資料表中含有 ModifierIp, CreatorIp 這兩個欄位, 也會自動帶入 WU.ClientIP
//            /// </summary>
//            /// <param name="Values"></param>
//            /// <param name="UserId">會試著更新以下欄位(注意大小寫): UpdaterId, CreatorId, ModifierId</param>
//            /// <returns></returns>
//            public InsertSQLBuilder Set(Dictionary<string, object> Values, Int32 UserId =  -1)
//            {
//                foreach(string FieldName in Values.Keys)
//                {
//                    var V = Values[FieldName];
//                    if (Convert.IsDBNull(V))
//                    {
//                        V = null;
//                    }
//                    this.ltFields.Add(new Field(FieldName, V));
//                }

//                if (UserId >= 0 && dicFields.ContainsKey("UpdaterId"))
//                {
//                    this.ltFields.Add(new Field("UpdaterId", UserId));
//                }

//                if (UserId >= 0 && dicFields.ContainsKey("CreatorId"))
//                {
//                    this.ltFields.Add(new Field("CreatorId", UserId));
//                }

//                if (UserId >= 0 && dicFields.ContainsKey("ModifierId"))
//                {
//                    this.ltFields.Add(new Field("ModifierId", UserId));
//                }

//                if (dicFields.ContainsKey("ModifierIp"))
//                {
//                    this.ltFields.Add(new Field("ModifierIp", WU.ClientIP));
//                }

//                if (dicFields.ContainsKey("CreatorIp"))
//                {
//                    this.ltFields.Add(new Field("CreatorIp", WU.ClientIP));
//                }

//                return this;
//            }

//            public InsertSQLBuilder SetGetDate(string FieldName)
//            {
//                this.ltFields.Add(new Field(FieldName, "GetDate()", Field.Type.function_));
//                return this;
//            }

//            /// <summary>
//            /// 注意, 會同時設定 UpdaterId, CreatorId, 兩個欄位  (Follow CRUD naming)
//            /// </summary>
//            /// <param name="MemberId"></param>
//            /// <returns></returns>
//            public InsertSQLBuilder SetAll(int MemberId = -1)
//            {
//                this._SetAll(MemberId, MemberId);
//                return this;
//            }

//            /// <summary>
//            /// 把 Values 的所有欄位(Properties and Fields)填入(注意, 預設會帶入 Id)
//            /// </summary>
//            /// <param name="Values"></param>
//            /// <param name="AutoFieldName">這個欄位不會被加入</param>
//            /// <param name="onlyFields"></param>
//            /// <returns></returns>
//            public InsertSQLBuilder Set(object Values, string AutoFieldName = null, string onlyFields = null)
//            {
//                //不要 Global 的 Field 或 Property
//                BindingFlags bindingFlags = BindingFlags.Public |
//                          BindingFlags.NonPublic |
//                          BindingFlags.Instance;

//                List<string> fields = null;

//                if (!string.IsNullOrEmpty(onlyFields))
//                {
//                    fields = onlyFields.Split(',').ToList();
//                }

//                foreach (var pf in Values.GetType().GetProperties(bindingFlags))
//                {
//                    if (
//                        (string.IsNullOrEmpty(AutoFieldName) || pf.Name.ToLower() != AutoFieldName.ToLower())
//                        && (fields == null || fields.Contains(pf.Name))
//                        )
//                    {
//                        this.Set(pf.Name, pf.GetValue(Values, null));
//                    }
//                }

//                foreach (var pf in Values.GetType().GetFields())
//                {
//                    if (!pf.IsDefined(typeof(CompilerGeneratedAttribute), false)) //去除掉 Compiler 會生出來的欄位 https://stackoverflow.com/questions/31528140/get-with-reflection-fields-that-are-not-generated-by-the-compiler
//                    {
//                        if ((string.IsNullOrEmpty(AutoFieldName) || pf.Name.ToLower() != AutoFieldName.ToLower())
//                        && (fields == null || fields.Contains(pf.Name)))
//                        {
//                            this.Set(pf.Name, pf.GetValue(Values));
//                        }
//                    }
//                }

//                return this;
//            }

//            public int SetAllAndExecute()
//            {
//                this._SetAll();
//                return this.Execute();
//            }

//            public int SetAllAndInsert()
//            {
//                this._SetAll();
//                return this.Insert();
//            }

//            /// <summary>
//            /// 從 form 中讀取資料. 這裡不做資料檢查
//            /// </summary>
//            /// <param name="FieldName"></param>
//            /// <returns></returns>
//            public InsertSQLBuilder Set(string FieldName)
//            {
//                this._Set(FieldName);

//                return this;
//            }

//            /// <summary>
//            /// 給 SetF 使用
//            /// </summary>
//            /// <param name="PostFix"></param>
//            /// <returns></returns>
//            public InsertSQLBuilder SetPostFix(string PostFix)
//            {
//                _PostFix = PostFix;
//                return this;
//            }

//            /// <summary>
//            /// 預設會加 ";\r\n"
//            /// </summary>
//            public override string Sql
//            {
//                get
//                {
//                    if (ltFields.Count == 0)
//                    {
//                        throw new Exception("沒有指定欄位");
//                    }

//                    string fields = "";
//                    string values = "";
//                    foreach (Field F in ltFields)
//                    {
//                        fields += "," + F.Name;
//                        values += "," + F.SqlValue;
//                    }

//                    return " Insert Into " + tableName.SqlObj() + "(" + fields.Substring(1) + ") values(" + values.Substring(1) + ") " + _terminator;
//                }
//            }

//            /// <summary>
//            /// 預設會加上 ";\r\n"
//            /// </summary>
//            /// <returns></returns>
//            public override string ToString()
//            {
//                return Sql;
//            }

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <param name="timeout"></param>
//            /// <param name="IsReturnIdentity"></param>
//            /// <returns></returns>
//            public int Execute(string DBC = null, Int32 timeout = 0, bool IsReturnIdentity = false)
//            {
//                return SQL.ExecuteSQL(Sql, DBC, timeout, IsReturnIdentity);
//            }

//            public int Execute(SqlTransaction tran, Int32 timeout = 0, bool IsReturnIdentity = false)
//            {
//                return SQL.ExecuteSQL(Sql, tran, timeout, IsReturnIdentity);
//            }

//            public int Execute(SqlConnection conn, Int32 timeout = 0, bool IsReturnIdentity = false)
//            {
//                return Su.SQL.ExecuteSQL(Sql, conn, timeout, IsReturnIdentity);
//            }
//            /// <summary>
//            /// 會回傳 identity
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <param name="timeout"></param>
//            /// <returns></returns>
//            public int Insert(string DBC = null, Int32 timeout = 0)
//            {
//                return SQL.ExecuteSQL(Sql, DBC, timeout, true);
//            }
//        }

//        public class Criteria
//        {

//            /// <summary>
//            /// _Criteria 要保持 " And " 開頭 (因為常常會做 += 的動作)
//            /// </summary>
//            string _Criteria = "";

//            public Criteria(string fieldAndOP, object value, bool IsNotUnicode = false)
//            {
//                this.And(fieldAndOP, value, IsNotUnicode);
//            }

//            public Criteria(string sql)
//            {
//                this.And(sql);
//            }

//            public Criteria()
//            {
//            }

//            public Criteria And(Criteria oC)
//            {
//                _Criteria += " And " + oC.ToString() + " ";

//                return this;
//            }

//            /// <summary>
//            /// (現有的條件) or (新增的條件1) or (新增的條件2) or (新增的條件3)....
//            /// </summary>
//            /// <param name="ltCriteria"></param>
//            /// <returns></returns>
//            public Criteria Or(List<Criteria> ltCriteria)
//            {
//                if (ltCriteria is null || ltCriteria.Count == 0)
//                {
//                    return this;
//                }

//                // temp 會是 " Or " 開頭
//                string temp = "";
//                foreach (var oC in ltCriteria)
//                {
//                    temp += " Or " + oC + " ";
//                }

//                if (this._Criteria != "")
//                {
//                    this._Criteria = " " + this.ToString() + " " + temp;
//                }
//                else
//                {
//                    this._Criteria = temp.Substring(4);
//                }

//                this._Criteria = " And " + this.ToString() + " ";

//                return this;
//            }

//            public Criteria Or(string OrCriteria, bool IsCheckInection = true)
//            {
//                if (IsCheckInection && OrCriteria.IsSQLInjection())
//                {
//                    throw new Exception("有可能發生 SQL Injection. " + OrCriteria);
//                }

//                OrCriteria = OrCriteria.Trim();

//                if (OrCriteria.ToLower().StartsWith("or "))
//                {
//                    OrCriteria = OrCriteria.Substring(3);
//                }

//                if (string.IsNullOrWhiteSpace(_Criteria))
//                {
//                    this._Criteria = " And " + OrCriteria + " ";
//                }
//                else
//                {
//                    this._Criteria = " And ( " + this.ToString() + " Or " + OrCriteria + " ) ";
//                }
//                return this;
//            }

//            /// <summary>
//            /// (現有的條件) or (新增的條件)
//            /// </summary>
//            /// <param name="oC"></param>
//            /// <returns></returns>
//            public Criteria Or(Criteria oC)
//            {
//                if (string.IsNullOrWhiteSpace(_Criteria))
//                {
//                    this._Criteria = " And " + oC.ToString() + " ";
//                }
//                else
//                {
//                    this._Criteria = " And ( " + this.ToString() + " Or " + oC.ToString() + " ) ";
//                }

//                return this;
//            }

//            /// <summary>
//            /// (現有的條件) or (新增的條件)
//            /// </summary>
//            /// <param name="fieldAndOP"></param>
//            /// <param name="value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public Criteria Or(string fieldAndOP, object value, bool IsNotUnicode = false)
//            {
//                return Or(new Criteria(fieldAndOP, value, IsNotUnicode));
//            }

//            /// <summary>
//            /// fieldName 會被加上 []
//            /// </summary>
//            /// <param name="fieldName"></param>
//            /// <returns></returns>
//            public Criteria OrIsNull(string fieldName)
//            {
//                if (fieldName.IsSQLInjection())
//                {
//                    throw new Exception(fieldName + " 可能有 sql injection");
//                }

//                _Criteria += " Or (" + fieldName.SqlField() + " is null) ";
//                return this;
//            }

//            /// <summary>
//            /// 這裡沒有做 SQL Injection 的檢查, 要避免使用.
//            /// </summary>
//            /// <param name="AndCriteria">不要用 And 開頭</param>
//            /// <param name="IsCheckInection"></param>
//            public Criteria And(string AndCriteria, bool IsCheckInection = true)
//            {
//                if (IsCheckInection && AndCriteria.IsSQLInjection())
//                {
//                    throw new Exception("有可能發生 SQL Injection. " + AndCriteria);
//                }

//                AndCriteria = AndCriteria.Trim();

//                if (AndCriteria.ToLower().StartsWith("and "))
//                {
//                    AndCriteria = AndCriteria.Substring(4);
//                }

//                _Criteria += " And (" + AndCriteria + ") ";

//                return this;
//            }

//            /// <summary>
//            /// 可用的 operator, 記得前後要加逗號.
//            /// </summary>
//            const string ValidOPs = ",>,<,=,>=,<=,is,in,like,<>,";

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="fieldAndOP"></param>
//            /// <param name="value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public Criteria And(string fieldAndOP, object value, bool IsNotUnicode = false)
//            {
//                if (fieldAndOP.IsSQLInjection())
//                {
//                    throw new Exception(fieldAndOP + " 可能有 sql injection");
//                }

//                fieldAndOP = fieldAndOP.Trim();

//                string[] arrFieldAndOP = fieldAndOP.Split(' ');
//                if (fieldAndOP.ToLower().EndsWith("not in"))
//                {
//                    if (arrFieldAndOP.Length != 3)
//                    {
//                        throw new Exception("not in Error");
//                    }
//                }
//                else if (fieldAndOP.ToLower().EndsWith("not like"))
//                {
//                    if (arrFieldAndOP.Length != 3)
//                    {
//                        throw new Exception("not like Error");
//                    }
//                }
//                else
//                {
//                    if (arrFieldAndOP.Length > 2)
//                    {
//                        throw new Exception("fieldAndOP 只能有一個空白");
//                    }

//                    if (arrFieldAndOP.Length < 2)
//                    {
//                        throw new Exception("在 fieldAndOP 中, 你可能忘了指定 operator.");
//                    }

//                    if (!ValidOPs.Contains("," + arrFieldAndOP[1].ToLower() + ","))
//                    {
//                        throw new Exception("不認識的 Operator '" + arrFieldAndOP[1].ToLower() + "'");
//                    }
//                }

//                if (arrFieldAndOP[1].ToLower() == "in")
//                {
//                    if (value.ToString().IsSQLInjection())
//                    {
//                        throw new Exception(value + " 可能有 sql injection");
//                    }
//                    if (!value.ToString().Trim().StartsWith("("))
//                    {
//                        value = "(" + value + ")";
//                    }
//                    _Criteria += " And (" + arrFieldAndOP[0].SqlField() + " " + arrFieldAndOP[1] + " " + value + ") ";
//                }
//                else if (fieldAndOP.ToLower().EndsWith("not in"))
//                {
//                    if (value.ToString().IsSQLInjection())
//                    {
//                        throw new Exception(value + " 可能有 sql injection");
//                    }

//                    if (!value.ToString().Trim().StartsWith("("))
//                    {
//                        value = "(" + value + ")";
//                    }

//                    _Criteria += " And (" + arrFieldAndOP[0].SqlField() + " not in " + value + ") ";
//                }
//                else if (fieldAndOP.ToLower().EndsWith("not like"))
//                {
//                    _Criteria += " And (" + arrFieldAndOP[0].SqlField() + " not like " + SQL.SqlValue(value, IsNotUnicode) + ") ";
//                }
//                else
//                {
//                    if (value == null && arrFieldAndOP[1].Trim() == "=")
//                    {
//                        _Criteria += " And (" + arrFieldAndOP[0].SqlField() + " is null) ";
//                    }
//                    else
//                    {
//                        _Criteria += " And (" + arrFieldAndOP[0].SqlField() + " " + arrFieldAndOP[1] + " " + SQL.SqlValue(value, IsNotUnicode) + ") ";
//                    }

//                }
//                return this;
//            }

//            /// <summary>
//            /// fieldName 會被加上 []
//            /// </summary>
//            /// <param name="fieldName"></param>
//            /// <returns></returns>
//            public Criteria AndIsNull(string fieldName)
//            {
//                if (fieldName.IsSQLInjection())
//                {
//                    throw new Exception(fieldName + " 可能有 sql injection");
//                }

//                _Criteria += " And (" + fieldName.SqlField() + " is null) ";
//                return this;
//            }

//            /// <summary>
//            /// fieldName 會被加上 []
//            /// </summary>
//            /// <param name="fieldName"></param>
//            /// <returns></returns>
//            public Criteria AndIsNotNull(string fieldName)
//            {
//                if (fieldName.IsSQLInjection())
//                {
//                    throw new Exception(fieldName + " 可能有 sql injection");
//                }

//                _Criteria += " And (" + fieldName.SqlField() + " is Not null) ";
//                return this;
//            }

//            /// <summary>
//            /// 在條件前加上 Where 傳回, 若為空條件, 則回傳空字串.
//            /// </summary>
//            /// <returns></returns>
//            public string Where()
//            {
//                if (_Criteria == "")
//                {
//                    return "";
//                }
//                else
//                {
//                    return " Where " + _Criteria.Substring(4);
//                }
//            }


//            /// <summary>
//            /// 沒有 Where, 只有條件式, 也沒有 And 開頭. 會用 () 包起來
//            /// </summary>
//            /// <returns></returns>
//            public override string ToString()
//            {
//                if (_Criteria == "")
//                {
//                    return "";
//                }
//                else
//                {
//                    return " (" + _Criteria.Substring(4) + ") ";
//                }
//            }
//        }

//        /// <summary>
//        /// Select 或 Delete 可以用這個
//        /// </summary>
//        public class SQLBuilder : BaseSQLBuilder
//        {
//            public enum LockLevel
//            {
//                None = 0,
//                NoLock = 100
//            }

//            /// <summary>
//            /// _sql = "select " + _sql.Substring(1) + " from " + Table.SqlObj() + " " + Lock + " ";
//            /// </summary>
//            string _sql = "";

//            /// <summary>
//            /// 加上[], 並自動做一些安全檢查
//            /// </summary>
//            /// <param name="Fields">有子查詢的請直接指定 select part, Fields 有可能被 sql injection, 盡量不要用外面傳進來的文字, 注意, [ 和 ] 都會被取代掉. 且前後不可有空白</param>
//            /// <param name="Table"></param>
//            /// <param name="Level">先明確指定 LockLevel, 過一段時間再來考決定預設值.</param>
//            public SQLBuilder(string Fields, string Table, LockLevel Level)
//            {
//                if (Fields.Contains(";") || Fields.Contains("--"))
//                {
//                    throw new Exception("為避免 SQL injection. Fields 不可有 ';' 或 '--'");
//                }

//                var arrFields = Fields.Split(',');
//                foreach (string f in arrFields)
//                {
//                    if (f.Trim() == "*")
//                    {
//                        _sql += "," + f;
//                    }
//                    else
//                    {
//                        _sql += "," + f.SqlObj();
//                    }
//                }

//                string Lock = "";
//                switch (Level)
//                {
//                    case LockLevel.NoLock:
//                        Lock = "(NoLock)";
//                        break;
//                }

//                _sql = "select " + _sql.Substring(1) + " from " + Table.SqlObj() + " " + Lock + " ";
//            }

//            public SQLBuilder TrueWith(string Field)
//            {
//                criteria.And(Field + " =", "Y");

//                return this;
//            }

//            public SQLBuilder FalseWith(string Field)
//            {
//                criteria.And(Field + " =", "N");

//                return this;
//            }

//            public SQLBuilder And(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP, Value, IsNotUnicode);

//                return this;
//            }

//            /// <summary>
//            /// 欄位名稱: StartDate 和 EndDate, 比對: WU.GetDateValue("StartDate"), WU.GetDateValue("EndDate")
//            /// </summary>
//            /// <returns></returns>
//            public SQLBuilder DuplicateDate()
//            {
//                criteria.And("StartDate <=", WU.GetDateValue("EndDate"));
//                criteria.And("EndDate <=", WU.GetDateValue("StartDate"));
//                return this;
//            }

//            /// <summary>
//            /// 三個變數的 And
//            /// </summary>
//            /// <param name="Field"></param>
//            /// <param name="sQLOP"></param>
//            /// <param name="Value"></param>
//            /// <returns></returns>
//            public SQLBuilder A3(string Field, SQLOP sQLOP, object Value)
//            {
//                criteria.And(Field + Operand(sQLOP), Value);
//                return this;
//            }

//            /// <summary>
//            /// 等於 And("Is_Deleted =", "N")
//            /// </summary>
//            /// <returns></returns>
//            public SQLBuilder NotDeleted()
//            {
//                criteria.And("Is_Deleted =", "N");
//                return this;
//            }

//            public SQLBuilder DateOK()
//            {
//                criteria.And("StartDate <= ", DateTime.Now).And("EndDate >=", DateTime.Now);
//                return this;
//            }

           

//            public SQLBuilder And_BT(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " >", Value, IsNotUnicode);

//                return this;
//            }

//            public SQLBuilder And_BE(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " >=", Value, IsNotUnicode);

//                return this;
//            }

//            public SQLBuilder And_ST(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " <", Value, IsNotUnicode);

//                return this;
//            }

//            public SQLBuilder And_SE(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " <=", Value, IsNotUnicode);

//                return this;
//            }

//            /// <summary>
//            /// And 的同義 function
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public SQLBuilder Where(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                return this.And(FieldAndOP, Value, IsNotUnicode);
//            }

//            public SQLBuilder Where_EQ(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                return this.And_EQ(FieldAndOP, Value, IsNotUnicode);
//            }

//            /// <summary>
//            /// 相等, criteria.And(FieldAndOP + " =", Value)
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsNotUnicode"></param>
//            /// <returns></returns>
//            public SQLBuilder And_EQ(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                criteria.And(FieldAndOP + " =", Value, IsNotUnicode);

//                return this;
//            }


//            /// <summary>
//            /// 取一個 NoLock 的 SQL Statement
//            /// </summary>
//            /// <param name="Fields"></param>
//            /// <param name="Table"></param>
//            /// <returns></returns>
//            public static SQLBuilder NoLock(string Fields, string Table)
//            {
//                return new SQLBuilder(Fields, Table, LockLevel.NoLock);
//            }




//            /// <summary>
//            /// 取一個沒有 Lock 修飾詞的 Statement
//            /// </summary>
//            /// <param name="Fields"></param>
//            /// <param name="Table"></param>
//            /// <returns></returns>
//            public static SQLBuilder None(string Fields, string Table)
//            {
//                return new SQLBuilder(Fields, Table, LockLevel.None);
//            }



//            /// <summary>
//            /// Select Builder.
//            /// </summary>
//            /// <param name="SelectPart"> Select XX, YYY from TableName, 
//            /// SelectPart 沒有空白時, 會視為 Table Name, 自動轉為 Select * from [XXXXX] </param>
//            public SQLBuilder(string SelectPart)
//            {
//                if (SelectPart.IsSQLInjection())
//                {
//                    throw new Exception("可能會有 SQL Injectin: " + SelectPart);
//                }

//                if (!SelectPart.Contains(" "))
//                {
//                    SelectPart = "Select * from " + SelectPart.SqlObj();
//                }

//                //有機會補一個可以加 Where 的 SelectPart

//                this._sql = SelectPart + " ";
//            }

//            public SQLBuilder(string SelectPart, string FieldAndOP, object Value)
//            {
//                if (SelectPart.IsSQLInjection())
//                {
//                    throw new Exception("可能會有 SQL Injectin: " + SelectPart);
//                }

//                this._sql = SelectPart + " ";

//                this.criteria.And(FieldAndOP, Value);
//            }

//            public SQLBuilder(string SelectPart, string criteriaSQL)
//            {
//                if (SelectPart.IsSQLInjection())
//                {
//                    throw new Exception("可能會有 SQL Injectin: " + SelectPart);
//                }

//                this._sql = SelectPart + " ";

//                this.criteria.And(criteriaSQL);
//            }

//            string _OrderBy = "";

//            /// <summary>
//            /// 不需加 order by
//            /// </summary>
//            /// <param name="orderby"></param>
//            /// <returns></returns>
//            public SQLBuilder OrderBy(string orderby)
//            {
//                if (orderby.IsSQLInjection())
//                {
//                    throw new Exception("可能會有 SQL Injectin: " + orderby);
//                }

//                _OrderBy = " " + orderby.Trim();

//                if (!_OrderBy.ToLower().Trim().StartsWith("order by"))
//                {
//                    _OrderBy = " order by " + _OrderBy;
//                }

//                return this;
//            }

//            string _GroupBy = "";

//            public SQLBuilder GroupBy(string GroupBy)
//            {
//                if (GroupBy.IsSQLInjection())
//                {
//                    throw new Exception("可能會有 SQL Injectin: " + GroupBy);
//                }

//                _GroupBy = " " + GroupBy.Trim();

//                if (!_GroupBy.ToLower().Trim().StartsWith("group by"))
//                {
//                    _GroupBy = " group by " + _GroupBy;
//                }

//                return this;
//            }

//            /// <summary>
//            /// 參考 Criteria 的 Or
//            /// </summary>
//            /// <param name="oC"></param>
//            /// <returns></returns>
//            public SQLBuilder Or(Criteria oC)
//            {
//                this.criteria.Or(oC);

//                return this;
//            }

//            public SQLBuilder Or(string FieldAndOP, object Value, bool IsNotUnicode = false)
//            {
//                this.criteria.Or(new Criteria(FieldAndOP, Value, IsNotUnicode));

//                return this;
//            }

//            public SQLBuilder OrIsNull(string FieldName)
//            {
//                criteria.OrIsNull(FieldName);

//                return this;
//            }

//            public SQLBuilder And(Criteria oC)
//            {
//                criteria.And(oC);

//                return this;
//            }


//            public SQLBuilder AndIsNull(string FieldName)
//            {
//                criteria.AndIsNull(FieldName);

//                return this;
//            }

//            public SQLBuilder AndIsNotNull(string FieldName)
//            {
//                criteria.AndIsNotNull(FieldName);

//                return this;
//            }

//            /// <summary>
//            /// 傳入空白字串則不動作
//            /// </summary>
//            /// <param name="FieldAndOP"></param>
//            /// <param name="Value"></param>
//            /// <param name="IsAddOneDate"></param>
//            /// <returns></returns>
//            public SQLBuilder AddDateText(string FieldAndOP, string Value, bool IsAddOneDate = false)
//            {
//                if (Value.IsDate())
//                {
//                    if (IsAddOneDate)
//                    {
//                        this.And(FieldAndOP, Value.ToDate().AddDays(1));
//                    }
//                    else
//                    {
//                        this.And(FieldAndOP, Value.ToDate());
//                    }
//                }
//                return this;
//            }


//            public SQLBuilder And(string AndCriteria, bool IsCheckInection = true)
//            {
//                criteria.And(AndCriteria, IsCheckInection);

//                return this;
//            }

//            //public DataTable DT(int DBCId)
//            //{
//            //    return SSQL.DtFromSql(Sql, DBCId);
//            //}

//            //public DataTable MySQLDT(string DBC = null)
//            //{
//            //    return MySQL.DtFromSql(Sql, DBC);
//            //}

//            public DataTable DT(string DBC = null)
//            {
//                return DBServer.DefaultType switch
//                {
//                    DBServer.Type.MSSQL => SQL.DtFromSql(Sql, DBC),
//                    //DBServer.Type.MYSQL => MySQL.DtFromSql(Sql, DBC),
//                    _ => throw new Exception("請設定 Su.StringExtension.DefaultDBServer, 目前只支援 MSSQL 和 MYSQL"),
//                };
//            }

//            //public DataTable DT(MySqlTransaction tran)
//            //{
//            //    return MySQL.DtFromSql(Sql, tran);
//            //}

//            public DataTable DT(SqlTransaction tran)
//            {
//                return SQL.DtFromSql(Sql, tran);
//            }

//            public DataTable PagerDT(int CurrentPage, int PageSize = 20, string DBC = null)
//            {
//                int StartIndex = (CurrentPage - 1) * PageSize;

//                if (_OrderBy == "")
//                {
//                    //一定要有 Order By 才能用 OFFSET, 就假設有 id 吧. 沒 id 就會丟 Exception 了.
//                    _OrderBy = " order by id ";
//                }

//                var PageSql = Sql + " OFFSET " + StartIndex + " ROWS FETCH NEXT " + PageSize + " ROWS ONLY";

//                DataTable dt = null;
//                try
//                {
//                    dt = SQL.DtFromSql(PageSql, DBC);
//                }
//                catch (Exception ex)
//                {
//                    if (ex.ToString().Contains("Incorrect syntax near 'OFFSET'."))
//                    {
//                        var rankColumn = ", ROW_NUMBER() OVER (" + _OrderBy + ") AS x ";

//                        PageSql = "SELECT * " +
//                                  " FROM (" + _sql.Replace(" from ", rankColumn + " from ").Replace(" From ", rankColumn + " From ") + criteria.Where() + _GroupBy + ") AS tbl" +
//                                  " WHERE tbl.x Between " + StartIndex + " AND " + (StartIndex + PageSize);

//                        dt = SQL.DtFromSql(PageSql, DBC);
//                    }
//                }
//                return dt;
//            }

//            public int GetCount(string DBC = null)
//            {
//                return (int)Su.SQL.GetSingleValue(this.CountSql, DBC);
//            }

//            public PageDT2 GetPageDT2(int PageNum, int PageSize = 20, string DBC = null)
//            {
//                var oRes = new PageDT2();
//                oRes.TotalRecord = GetCount(DBC);
//                oRes.PageSize = PageSize;

//                if (oRes.TotalRecord % PageSize == 0)
//                {
//                    oRes.TotalPage = oRes.TotalRecord / PageSize;
//                }
//                else
//                {
//                    oRes.TotalPage = (oRes.TotalRecord / PageSize) + 1;
//                }

//                if (oRes.TotalPage < PageNum)
//                {
//                    PageNum = oRes.TotalPage;
//                }

//                if (oRes.TotalRecord == 0)
//                { 
//                    //無資料固定回傳頁數1
//                    PageNum = 1;
//                }

//                oRes.CurrentPage = PageNum;

//                if (oRes.TotalRecord > 0)
//                {
//                    oRes.DT = PagerDT(oRes.CurrentPage, PageSize, DBC);
//                }

//                oRes.SetSQL(Sql);

//                return oRes;
//            }



//            /// <summary>
//            /// 找不到資料時, 回傳 null
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <returns></returns>
//            public DataRow FirstRow(string DBC = null)
//            {
//                var DT = SQL.DtFromSql(Sql, DBC);
//                if (DT.Rows.Count == 0)
//                {
//                    return null;
//                }
//                else
//                {
//                    return DT.Rows[0];
//                }
//            }

//            public DataRow FirstRow(SqlTransaction Tran)
//            {
//                var DT = SQL.DtFromSql(Sql, Tran);
//                if (DT.Rows.Count == 0)
//                {
//                    return null;
//                }
//                else
//                {
//                    return DT.Rows[0];
//                }
//            }

//            public DataTable DT(SQL oSQL)
//            {
//                return oSQL.DTFromSQL_(Sql);
//            }

//            /// <summary>
//            /// 找不到時會回傳 null
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <returns></returns>
//            public object GetSingleValue(string DBC = null)
//            {
//                return SQL.GetSingleValue(this.Sql, DBC);
//            }

//            /// <summary>
//            /// 找不到時會回傳 null
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <returns></returns>
//            public T GetSingleValue<T>(string DBC = null)
//            {
//                return (T)SQL.GetSingleValue(this.Sql, DBC);
//            }

//            /// <summary>
//            /// 找不到時會回傳 NotFound(預設為 null)
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <param name="NotFound"></param>
//            /// <returns></returns>
//            public string GetString(string DBC = null, string NotFound = null)
//            {
//                var obj = SQL.GetSingleValue(this.Sql, DBC);

//                if (obj == null || Convert.IsDBNull(obj))
//                {
//                    return NotFound;
//                }
//                else
//                {
//                    return (string)obj;
//                }
//            }

//            /// <summary>
//            /// 若找不到資料或是 DBNull 時, 回傳 null;
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <returns></returns>
//            public Int32? GetInt32_N(string DBC = null)
//            {
//                var obj = SQL.GetSingleValue(this.Sql, DBC);

//                if (obj == null || Convert.IsDBNull(obj))
//                {
//                    return null;
//                }
//                else
//                {
//                    return (Int32)obj;
//                }
//            }

//            /// <summary>
//            /// Null 會回傳 0(可用 Default 調整)
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <param name="Default"></param>
//            /// <returns></returns>
//            public Int32 GetInt32(string DBC = null, Int32 Default = 0)
//            {
//                var obj = SQL.GetSingleValue(this.Sql, DBC);

//                if (obj == null || Convert.IsDBNull(obj))
//                {
//                    return Default;
//                }
//                else
//                {
//                    return (Int32)obj;
//                }
//            }

//            public Int32 GetInt32(SqlTransaction tran, Int32 Default = 0)
//            {
//                var obj = SQL.GetSingleValue(this.Sql, tran);

//                if (obj == null || Convert.IsDBNull(obj))
//                {
//                    return Default;
//                }
//                else
//                {
//                    return (Int32)obj;
//                }
//            }
//            /// <summary>
//            /// 要用 SelectCount
//            /// </summary>
//            /// <param name="DBC"></param>
//            /// <returns></returns>
//            public bool IsNonZero(string DBC = null)
//            {
//                return (GetInt32(DBC, 0) != 0);
//            }



//            public override string Sql
//            {
//                get
//                {
//                    return _sql + criteria.Where() + _GroupBy + _OrderBy;
//                }
//            }

//            public string CountSql
//            {
//                get
//                {
//                    return "select count(*) " + FromAndWhere;
//                }
//            }

//            public string FromAndWhere
//            {
//                get
//                {
//                    // _sql => Select XX, YYY from TableName
//                    // column 可能包含 subquery，所以要捉最後一個 From
//                    int k = _sql.ToLower().LastIndexOf(" from");

//                    return _sql.Substring(k + 1) + criteria.Where();

//                    //string s = _sql + criteria.Where();
//                    //int K = s.ToLower().IndexOf(" from");
//                    //if (K > -1)
//                    //{
//                    //    return s.Substring(K + 1);
//                    //}
//                    //else
//                    //{
//                    //    return s;
//                    //}
//                }
//            }

//            public string Fields
//            {
//                get
//                {
//                    string s = _sql;
//                    s = s.Substring(0, s.ToLower().IndexOf(" from"));
//                    s = s.Substring("select ".Length);
//                    return s;
//                }
//            }

//            public string OrderByFields
//            {
//                get
//                {
//                    if (string.IsNullOrEmpty(_OrderBy))
//                    {
//                        return "";
//                    }

//                    return _OrderBy.Substring(" order by ".Length);
//                }
//            }

//            /// <summary>
//            /// 預設會加上 ";\r\n"
//            /// </summary>
//            /// <returns></returns>
//            public override string ToString()
//            {
//                return Sql + _terminator;
//            }
//        }
//    }
//}
