using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Su
{
    public partial class MsSql
    {
        public enum CriteriaType
        {
            General = 0,
            OR = 1,
            And = 2,
            SingleSql = 3
        }

        /// <summary>
        /// 特定 Criteria
        /// </summary>
        public enum ConstIdEnum
        {
            /// <summary>
            /// 一般 Criteria
            /// </summary>
            NotConstCriteria = 0,

            /// <summary>
            /// 在 Or List 之中, 有一個 True Criteria, 則回傳 True Criteria
            /// 在 And List 之中, True Criteria 會被忽略
            /// </summary>
            TrueCriteria = 1,

            /// <summary>
            /// 在 Or List 之中, False Criteria 會被忽略
            /// 在 And List 之中, 有一個 False Criteria, 則傳回 False Criteria
            /// </summary>
            FalseCriteria = 2
        }

        public class Criteria
        {
            public readonly ConstIdEnum ConstId = ConstIdEnum.NotConstCriteria;
            readonly CriteriaType Type = CriteriaType.General;
            readonly List<Criteria> Criterias = null;
            readonly string FinalSql = null;
            const string ValidOPs = ",!=,>,<,=,>=,<=,is,in,like,<>,";
            public readonly string FieldAndOP = "";
            public readonly bool IsNotUnicode = false;
            public object Value = new object();
            public Criteria(string fieldAndOP, object value, UnicodeType unicode = UnicodeType.ByFieldName)
            {
                this.FieldAndOP = fieldAndOP.Trim();

                if (string.IsNullOrEmpty(FieldAndOP))
                {
                    throw new Exception("field and operation should not be empty.");
                }

                if (FieldAndOP.ToLower().StartsWith("is_") && unicode == UnicodeType.ByFieldName)
                {
                    IsNotUnicode = true;
                }
                this.Value = value;
            }

            private Criteria(Criteria a, Criteria b, CriteriaType type)
            {
                Criterias = new List<Criteria>();
                Criterias.Add(a);
                Criterias.Add(b);
                this.Type = type;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sql"></param>
            /// <param name="parameters">轉為 sql value 之後, 取代 sql 中的 {XXX}</param>
            /// <param name="dbObjects">轉為 sql object 之後, 取代 sql 中的 [XXX]</param>
            /// <param name="isCheckInection"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public static Criteria GetSinglCriteria(string sql, object parameters, object dbObjects = null, bool isCheckInection = true)
            {
                if (isCheckInection && sql.IsMsSqlInjection())
                {
                    throw new Exception("有可能發生 SQL Injection. " + sql);
                }

                var sourceProperties = parameters.GetType().GetProperties().ToList();

                foreach (var srcItem in sourceProperties)
                {
                    sql = sql.Replace("{" + srcItem.Name + "}", srcItem.GetValue(parameters).MsSqlValue());
                }

                if (dbObjects != null)
                {
                    sourceProperties = dbObjects.GetType().GetProperties().ToList();

                    foreach (var srcItem in sourceProperties)
                    {
                        sql = sql.Replace("[" + srcItem.Name + "]", srcItem.GetValue(dbObjects).ToString().MsSqlObj());
                    }
                }

                return new Criteria(sql, CriteriaType.SingleSql, false); //已經檢查過了, 不要再做 sql injection 檢查
            }

            /// <summary>
            /// 直接寫條件 SQL, CriteriaType 必需為 CriteriaType.SingleSql
            /// </summary>
            /// <param name="criteria"></param>
            /// <param name="type"></param>
            private Criteria(string criteria, CriteriaType type, bool isCheckInection = true)
            {
                if (type != CriteriaType.SingleSql)
                {
                    throw new Exception("Only single-sql type is allowed.");
                }

                if (string.IsNullOrEmpty(criteria))
                {
                    throw new Exception("Criteria should not be empty.");
                }

                if (isCheckInection && criteria.IsMsSqlInjection())
                {
                    throw new Exception("There may be sql injection in criteria.");
                }

                this.Type = type;

                FinalSql = criteria;
            }

            private Criteria(string criteria, ConstIdEnum constId)
            {
                this.Type = CriteriaType.SingleSql;

                FinalSql = criteria;

                ConstId = constId;
            }

            /// <summary>
            /// 用於 Operator (1 = 1)
            /// </summary>
            static Criteria GetTrueCriteria()
            {
                return new Criteria("1 = 1", ConstIdEnum.TrueCriteria);
            }

            /// <summary>
            /// 用於 Operator
            /// </summary>
            static Criteria GetFalseCriteria()
            {
                return new Criteria("1 <> 1", ConstIdEnum.TrueCriteria);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator |(Criteria a, Criteria b)
            {
                // True 就直接回傳
                if (a.ConstId == ConstIdEnum.TrueCriteria)
                {
                    return a;
                }

                if (b.ConstId == ConstIdEnum.TrueCriteria)
                {
                    return b;
                }

                // false 就直接回傳另一個條件
                if (a.ConstId == ConstIdEnum.FalseCriteria)
                {
                    return b;
                }

                if (b.ConstId == ConstIdEnum.FalseCriteria)
                {
                    return a;
                }

                //建立 Or Criteria
                if (a.Type == CriteriaType.OR)
                {
                    a.Criterias.Add(b);
                    return a;
                }

                if (b.Type == CriteriaType.OR)
                {
                    b.Criterias.Add(a);
                    return b;
                }

                return new Criteria(a, b, CriteriaType.OR);
            }

            /// <summary>
            /// 使用這個有小缺點, b 還是會被建立或 evaluate. 使用三元運算式會比較有效率些. 但用 or operator 會比較易讀
            /// 若 A 成立則回傳 null, 否則回傳 b (return a ? null : b;)
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator |(bool a, Criteria b)
            {
                return a ? GetTrueCriteria() : b;
            }

            /// <summary>
            /// 同 a | b, bool 在後面.
            /// </summary>
            /// <param name="b"></param>
            /// <param name="a"></param>
            /// <returns></returns>
            public static Criteria operator |(Criteria b, bool a)
            {
                return a | b;
            }

            /// <summary>
            /// 若 a 為 false, 直接回傳一個不會成立的條件.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator &(bool a, Criteria b)
            {
                if (a)
                {
                    return b;
                }
                else
                {
                    return GetFalseCriteria();
                }
            }

            /// <summary>
            /// 同 a and b, bool 在後面
            /// </summary>
            /// <param name="b"></param>
            /// <param name="a"></param>
            /// <returns></returns>
            public static Criteria operator &(Criteria b, bool a)
            {
                return a & b;
            }

            /// <summary>
            /// And Operation
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Criteria operator &(Criteria a, Criteria b)
            {
                // false 就直接回傳
                if (a.ConstId == ConstIdEnum.FalseCriteria)
                {
                    return a;
                }

                if (b.ConstId == ConstIdEnum.FalseCriteria)
                {
                    return b;
                }

                //true 就回傳另一個
                if (a.ConstId == ConstIdEnum.TrueCriteria)
                {
                    return b;
                }

                if (b.ConstId == ConstIdEnum.TrueCriteria)
                {
                    return a;
                }

                //建立 And List
                if (a.Type == CriteriaType.And)
                {
                    a.Criterias.Add(b);
                    return a;
                }

                if (b.Type == CriteriaType.And)
                {
                    b.Criterias.Add(a);
                    return b;
                }

                return new Criteria(a, b, CriteriaType.And);
            }

            public string Sql
            {
                get
                {
                    if (Type == CriteriaType.SingleSql)
                    {
                        return "(" + FinalSql + ")";
                    }

                    if (Type == CriteriaType.OR)
                    {
                        return "(" + Criterias.Select(x => "(" + x.Sql + ")").ToOneString(" or ") + ")";
                    }

                    if (Type == CriteriaType.And)
                    {
                        return "(" + Criterias.Select(x => "(" + x.Sql + ")").ToOneString(" and ") + ")";
                    }

                    string fieldAndOP = FieldAndOP.Trim();

                    //檢查 fieldAndOP 的格式
                    string[] arrFieldAndOP = fieldAndOP.Split(' ');
                    if (fieldAndOP.EndsWith("not in"))
                    {
                        if (arrFieldAndOP.Length != 3)
                        {
                            throw new Exception("'not in' fieldAndOP 只能有兩個空白");
                        }
                    }
                    else if (fieldAndOP.EndsWith("not like"))
                    {
                        if (arrFieldAndOP.Length != 3)
                        {
                            throw new Exception("'not like' fieldAndOP 只能有兩個空白");
                        }
                    }
                    else
                    {
                        if (arrFieldAndOP.Length > 2)
                        {
                            throw new Exception("fieldAndOP 只能有一個空白");
                        }

                        if (arrFieldAndOP.Length < 2)
                        {
                            throw new Exception("在 fieldAndOP 中, 你可能忘了指定 operator.");
                        }

                        if (!ValidOPs.Contains("," + arrFieldAndOP[1].ToLower() + ","))
                        {
                            throw new Exception("不認識的 Operator '" + arrFieldAndOP[1].ToLower() + "'");
                        }
                    }

                    if (arrFieldAndOP[1].ToLower() == "in")
                    {
                        if (Value.ToString().IsMsSqlInjection())
                        {
                            throw new Exception(Value + " 可能有 sql injection");
                        }
                        if (!Value.ToString().Trim().StartsWith("("))
                        {
                            Value = "(" + Value + ")";
                        }
                        return arrFieldAndOP[0].MsSqlField() + " " + arrFieldAndOP[1] + " " + Value.ToString();
                    }
                    else if (fieldAndOP.EndsWith("not in"))
                    {
                        if (Value.ToString().IsMsSqlInjection())
                        {
                            throw new Exception(Value + " 可能有 sql injection");
                        }

                        if (!Value.ToString().Trim().StartsWith("("))
                        {
                            Value = "(" + Value + ")";
                        }
                        return arrFieldAndOP[0].MsSqlField() + " not in " + Value.ToString();
                    }
                    else if (fieldAndOP.EndsWith("not like"))
                    {
                        return arrFieldAndOP[0].MsSqlField() + " not like " + MsSqlValue;
                    }
                    else
                    {
                        if (Value == null && arrFieldAndOP[1].Trim() == "=")
                        {
                            return arrFieldAndOP[0].MsSqlField() + " is null";
                        }
                        else
                        {
                            return arrFieldAndOP[0].MsSqlField() + " " + arrFieldAndOP[1] + " " + MsSqlValue;
                        }
                    }
                }
            }

            public string MsSqlValue
            {
                get
                {
                    return Value.MsSqlValue(IsNotUnicode);
                }
            }
        }
    }
    

}
