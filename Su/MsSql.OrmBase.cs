using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Su.StringExtension;

namespace Su
{
    public partial class MsSql
    {
        /// <summary>
        /// OrmBase 的摘要描述
        /// </summary>
        public abstract class MsSqlOrmBase
        {
            /// <summary>
            /// 記錄這個 Orm 的 tablename
            /// </summary>
            protected abstract string TableNameInDb { get; }

            private SqlBuilder GetBuilder()
            {
                return TableNameInDb.MsSqlBuilder();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="onlyFields"></param>
            /// <param name="skipFields"></param>
            /// <param name="dbc"></param>
            /// <param name="timeout"></param>
            /// <param name="CreatorId"></param>
            /// <returns>回傳 Identity</returns>
            public int Add(string onlyFields = null, string skipFields = "Id", Sql.DbId dbId = null, int timeout = 0, int? CreatorId = null)
            {
                if (onlyFields == null)
                {
                    onlyFields = string.Join(",", ModifiedFields);
                }
                else
                {
                    //取交集
                    onlyFields = string.Join(",", onlyFields.Split(',').Where(x => ModifiedFields.Contains(x)));
                }

                var b = GetBuilder()
                    .SetObject(this, onlyFields: onlyFields, skipFields: skipFields);

                if (CreatorId != null)
                {
                    b.SetObject(new
                    {
                        CreatorId,
                        CreateDate = DateTime.Now,
                        ModifierId = CreatorId,
                        ModifyDate = DateTime.Now
                    });
                }

                return b.Insert(dbId, timeout);
            }

            /// <summary>
            /// obj 必需有 Id 欄位, 注意大小寫.
            /// </summary>
            /// <param name="onlyFields"></param>
            /// <param name="IdFieldName"></param>
            /// <param name="dbc"></param>
            /// <param name="timeout"></param>
            /// <param name="ModifierId"></param>
            /// <param name="skipFields"></param>
            /// <returns></returns>
            public int Update(string onlyFields = null, string IdFieldName = "Id", Sql.DbId dbId = null, int timeout = 0, int? ModifierId = null, string skipFields = null)
            {
                try
                {
                    return GetUpdateBuilder(onlyFields, IdFieldName, skipFields).Update(dbId, timeout, ModifierId);
                }
                catch (Exception ex)
                {
                    throw new Exception("IdFieldName: " + IdFieldName + ", " + ex.FullInfo());
                }
            }

            /// <summary>
            /// 物件轉為 Update Builder, 再進行 update.
            /// 也可以利用 AddUpdateSqlToQueue, 稍後在 transaction 中執行.
            /// </summary>
            /// <param name="onlyFields"></param>
            /// <param name="IdFieldName"></param>
            /// <param name="skipFields"></param>
            /// <returns></returns>
            public SqlBuilder GetUpdateBuilder(string onlyFields = null, string IdFieldName = "Id", string skipFields = null)
            {
                if (onlyFields == null)
                {
                    onlyFields = string.Join(",", ModifiedFields);
                }
                else
                {
                    //取交集
                    onlyFields = string.Join(",", onlyFields.Split(',').Where(x => ModifiedFields.Contains(x)));
                }

                if (!string.IsNullOrEmpty(skipFields))
                {
                    var arr = skipFields.ToLower().Split(',');
                    onlyFields = string.Join(",", onlyFields.Split(',').Where(x => !arr.Contains(x.ToLower())));
                }

                Su.Debug.WriteLine("GetUpdateBuilder onlyFields: " + onlyFields);

                return GetBuilder()
                        .Where(IdFieldName + " = ", this.GetPropertyValue(IdFieldName))
                        .SetObject(this, onlyFields, IdFieldName);
            }

            /// <summary>
            /// 被修改的欄位
            /// </summary>
            protected readonly HashSet<string> ModifiedFields = new HashSet<string>();

            protected void SetDirty(string name)
            {
                if (!ModifiedFields.Contains(name))
                {
                    ModifiedFields.Add(name);
                }
            }

            public void ClearModifiedFields()
            {
                ModifiedFields.Clear();
            }

            public string GetModifiedFields()
            {
                return string.Join(",", ModifiedFields);
            }
        }
    }
}
