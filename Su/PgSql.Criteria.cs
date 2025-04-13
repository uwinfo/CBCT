//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Reflection;
//using System.Text;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Collections;

//namespace Su
//{
//    public partial class PgSql
//    {
//        public enum CriteriaType
//        {
//            General = 0,
//            OR = 1,
//            And = 2,
//            SingleSql = 3
//        }

//        /// <summary>
//        /// 特定 Criteria
//        /// </summary>
//        public enum ConstIdEnum
//        {
//            /// <summary>
//            /// 一般 Criteria
//            /// </summary>
//            NotConstCriteria = 0,

//            /// <summary>
//            /// 在 Or List 之中, 有一個 True Criteria, 則回傳 True Criteria
//            /// 在 And List 之中, True Criteria 會被忽略
//            /// </summary>
//            TrueCriteria = 1,

//            /// <summary>
//            /// 在 Or List 之中, False Criteria 會被忽略
//            /// 在 And List 之中, 有一個 False Criteria, 則傳回 False Criteria
//            /// </summary>
//            FalseCriteria = 2
//        }

//        public class Criteria
//        {
//            public readonly ConstIdEnum ConstId = ConstIdEnum.NotConstCriteria;
//            readonly CriteriaType Type = CriteriaType.General;
//            readonly List<Criteria> Criterias = null;
//            readonly string FinalSql = null;
//            const string ValidOPs = ",!=,>,<,=,>=,<=,is,in,like,<>,";
//            public readonly string FieldAndOP = "";
//            public object Value = new object();
//            public Criteria(string fieldAndOP, object value)
//            {
//                this.FieldAndOP = fieldAndOP.Trim();

//                if(this.FieldAndOP.ToLower().EndsWith(" in") || this.FieldAndOP.ToLower().EndsWith(" not in"))
//                {
//                    // in 的 value 必需是 List
//                    if (value.GetType().ToString() == "System.String")
//                    {
//                        throw new Exception("value 必需是 enumerable 的物件");
//                    }

//                    IEnumerable? enumerable = value as IEnumerable;
//                    if (enumerable == null)
//                    {
//                        throw new Exception("value 必需是 enumerable 的物件");
//                    }

//                    var a = value.PgSqlValue();
//                }

//                if (string.IsNullOrEmpty(FieldAndOP))
//                {
//                    throw new Exception("field and operation should not be empty.");
//                }

//                this.Value = value;
//            }

//            /// <summary>
//            /// 僅內部使用, 建立 And 或 Or 的 List
//            /// </summary>
//            /// <param name="a"></param>
//            /// <param name="b"></param>
//            /// <param name="type"></param>
//            private Criteria(Criteria a, Criteria b, CriteriaType type)
//            {
//                Criterias = new List<Criteria>();
//                Criterias.Add(a);
//                Criterias.Add(b);
//                this.Type = type;
//            }

//            /// <summary>
//            /// 直接寫條件 SQL, CriteriaType 必需為 CriteriaType.SingleSql
//            /// </summary>
//            /// <param name="criteria"></param>
//            /// <param name="type"></param>
//            public Criteria(string criteria, CriteriaType type, bool isCheckInection = true)
//            {
//                if (type != CriteriaType.SingleSql)
//                {
//                    throw new Exception("Only single-sql type is allowed.");
//                }

//                if (string.IsNullOrEmpty(criteria))
//                {
//                    throw new Exception("Criteria should not be empty.");
//                }

//                if (isCheckInection)
//                {
//                    criteria.CheckPgSqlInjection();
//                }

//                this.Type = type;

//                FinalSql = criteria;
//            }

//            /// <summary>
//            /// 僅內部使用, 用於建立 True 或 False 的 Criteria
//            /// </summary>
//            /// <param name="criteria"></param>
//            /// <param name="constId"></param>
//            private Criteria(string criteria, ConstIdEnum constId)
//            {
//                this.Type = CriteriaType.SingleSql;

//                FinalSql = criteria;

//                ConstId = constId;
//            }

//            /// <summary>
//            /// 用於 Operator (1 = 1)
//            /// </summary>
//            static Criteria GetTrueCriteria()
//            {
//                return new Criteria("1 = 1", ConstIdEnum.TrueCriteria);
//            }

//            /// <summary>
//            /// 用於 Operator
//            /// </summary>
//            static Criteria GetFalseCriteria()
//            {
//                return new Criteria("1 <> 1", ConstIdEnum.FalseCriteria);
//            }

//            /// <summary>
//            /// 
//            /// </summary>
//            /// <param name="a"></param>
//            /// <param name="b"></param>
//            /// <returns></returns>
//            public static Criteria operator |(Criteria a, Criteria b)
//            {
//                // True 就直接回傳
//                if (a.ConstId == ConstIdEnum.TrueCriteria)
//                {
//                    return a;
//                }

//                if (b.ConstId == ConstIdEnum.TrueCriteria)
//                {
//                    return b;
//                }

//                // false 就直接回傳另一個條件
//                if (a.ConstId == ConstIdEnum.FalseCriteria)
//                {
//                    return b;
//                }

//                if (b.ConstId == ConstIdEnum.FalseCriteria)
//                {
//                    return a;
//                }

//                //建立 Or Criteria
//                if (a.Type == CriteriaType.OR)
//                {
//                    a.Criterias.Add(b);
//                    return a;
//                }

//                if (b.Type == CriteriaType.OR)
//                {
//                    b.Criterias.Add(a);
//                    return b;
//                }

//                return new Criteria(a, b, CriteriaType.OR);
//            }

//            /// <summary>
//            /// 使用這個有小缺點, b 還是會被建立或 evaluate. 使用三元運算式會比較有效率些. 但用 or operator 會比較易讀
//            /// 若 A 成立則回傳 null, 否則回傳 b (return a ? null : b;)
//            /// </summary>
//            /// <param name="a"></param>
//            /// <param name="b"></param>
//            /// <returns></returns>
//            public static Criteria operator |(bool a, Criteria b)
//            {
//                return a ? GetTrueCriteria() : b;
//            }

//            /// <summary>
//            /// 同 a | b, bool 在後面.
//            /// </summary>
//            /// <param name="b"></param>
//            /// <param name="a"></param>
//            /// <returns></returns>
//            public static Criteria operator |(Criteria b, bool a)
//            {
//                return a | b;
//            }

//            /// <summary>
//            /// 若 a 為 false, 直接回傳一個不會成立的條件.
//            /// </summary>
//            /// <param name="a"></param>
//            /// <param name="b"></param>
//            /// <returns></returns>
//            public static Criteria operator &(bool a, Criteria b)
//            {
//                if (a)
//                {
//                    return b;
//                }
//                else
//                {
//                    return GetFalseCriteria();
//                }
//            }

//            /// <summary>
//            /// 同 a and b, bool 在後面
//            /// </summary>
//            /// <param name="b"></param>
//            /// <param name="a"></param>
//            /// <returns></returns>
//            public static Criteria operator &(Criteria b, bool a)
//            {
//                return a & b;
//            }

//            /// <summary>
//            /// And Operation
//            /// </summary>
//            /// <param name="a"></param>
//            /// <param name="b"></param>
//            /// <returns></returns>
//            public static Criteria operator &(Criteria a, Criteria b)
//            {
//                // false 就直接回傳
//                if (a.ConstId == ConstIdEnum.FalseCriteria)
//                {
//                    return a;
//                }

//                if (b.ConstId == ConstIdEnum.FalseCriteria)
//                {
//                    return b;
//                }

//                //true 就回傳另一個
//                if (a.ConstId == ConstIdEnum.TrueCriteria)
//                {
//                    return b;
//                }

//                if (b.ConstId == ConstIdEnum.TrueCriteria)
//                {
//                    return a;
//                }

//                //建立 And List
//                if (a.Type == CriteriaType.And)
//                {
//                    a.Criterias.Add(b);
//                    return a;
//                }

//                if (b.Type == CriteriaType.And)
//                {
//                    b.Criterias.Add(a);
//                    return b;
//                }

//                return new Criteria(a, b, CriteriaType.And);
//            }

//            public string Sql
//            {
//                get
//                {
//                    if (Type == CriteriaType.SingleSql)
//                    {
//                        return "(" + FinalSql + ")";
//                    }

//                    if (Type == CriteriaType.OR)
//                    {
//                        return "(" + Criterias.Select(x => "(" + x.Sql + ")").ToOneString(" or ") + ")";
//                    }

//                    if (Type == CriteriaType.And)
//                    {
//                        return "(" + Criterias.Select(x => "(" + x.Sql + ")").ToOneString(" and ") + ")";
//                    }

//                    string fieldAndOP = FieldAndOP.Trim();

//                    //檢查 fieldAndOP 的格式
//                    string[] arrFieldAndOP = fieldAndOP.Split(' ');
//                    if (fieldAndOP.EndsWith("not in"))
//                    {
//                        if (arrFieldAndOP.Length != 3)
//                        {
//                            throw new Exception("'not in' fieldAndOP 只能有兩個空白");
//                        }
//                    }
//                    else if (fieldAndOP.EndsWith("not like"))
//                    {
//                        if (arrFieldAndOP.Length != 3)
//                        {
//                            throw new Exception("'not like' fieldAndOP 只能有兩個空白");
//                        }
//                    }
//                    else
//                    {
//                        if (arrFieldAndOP.Length > 2)
//                        {
//                            throw new Exception("fieldAndOP 只能有一個空白");
//                        }

//                        if (arrFieldAndOP.Length < 2)
//                        {
//                            throw new Exception("在 fieldAndOP 中, 你可能忘了指定 operator.");
//                        }

//                        if (!ValidOPs.Contains("," + arrFieldAndOP[1].ToLower() + ","))
//                        {
//                            throw new Exception("不認識的 Operator '" + arrFieldAndOP[1].ToLower() + "'");
//                        }
//                    }

//                    if (Value == null && arrFieldAndOP[1].Trim() == "=")
//                    {
//                        return arrFieldAndOP[0].PgSqlColumnName() + " is null";
//                    }
//                    else
//                    {
//                        return arrFieldAndOP[0].PgSqlColumnName() + " " + arrFieldAndOP[1] + " " + PgSqlValue;
//                    }
//                }
//            }

//            public string PgSqlValue
//            {
//                get
//                {
//                    return Value.PgSqlValue();
//                }
//            }
//        }
//    }
    

//}
