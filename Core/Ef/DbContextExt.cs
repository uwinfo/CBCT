using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using Su;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Npgsql;

namespace Core.Ef
{
    /// <summary>
    /// 
    /// </summary>
    public static class DbContextExt
    {

        /// <summary>
        /// sql 中的 conditionMark 會被 conditions 合成的字串取代，前且前方會放置 where 或 and (可由 conditionPrefix 控制)。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditionMark"></param>
        /// <param name="conditions"></param>
        /// <param name="conditionPrefix"> 放在條件式前方的字串，傳入非 "and" 時，一率使用 "where"</param>
        /// <returns></returns>
        public static string ReplaceSqlCondition(this string sql, string conditionMark, List<string> conditions, string conditionPrefix = "where")
        {
            if (conditions.Count > 0)
            {
                //conditionPrefix 不會被直接加入 sql 中，以阻斷 injection 的機會
                sql = sql.Replace(conditionMark, $" {(conditionPrefix == "and" ? "and" : "where")} {string.Join(" and ", conditions)}");
            }
            else
            {
                sql = sql.Replace(conditionMark, "");
            }
            return sql;
        }

        /// <summary>
        /// 執行的 SQL 會是一個非查詢的 SQL
        /// 除了 insert 開頭之外，SQL 內容必需有 "where"，以避免誤動作
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sql">注意，變數要用 @xxx 的格式 </param>
        /// <param name="parameterObject"></param>
        /// <param name="isCheckSql"></param>
        /// <param name="isNormalize"></param>
        public static int ExecuteSafeSql(this DbContext context, string sql, object? parameterObject = null, bool isCheckSql = true, bool isNormalize = true)
        {
            if (isNormalize)
            {
                sql = sql.SqlNormalize();
            }

            if (isCheckSql)
            {
                sql.CheckPgSqlInjection();
            }

            if (!sql.ToLower().StartsWith("insert ") && !sql.ToLower().Contains("where"))
            {
                throw new Exception("除了 insert 指令之外，SQL 內容必需有 where");
            }

            if (parameterObject != null)
            {
                var parameters = new List<NpgsqlParameter>();
                foreach (PropertyInfo prop in parameterObject.GetType().GetProperties())
                {
                    object value = prop.GetValue(parameterObject) ?? DBNull.Value;  // 处理 null 值
                    parameters.Add(new NpgsqlParameter("@" + prop.Name, value));
                }
                return context.Database.ExecuteSqlRaw(sql, parameters);
            }

            return context.Database.ExecuteSqlRaw(sql);
        }

        /// <summary>
        /// 先取得 Datatable, 再由 Datatable 轉為物件 List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="isCheckSql"></param>
        /// <param name="isNormalize">trim, 把換行改為 空白</param>
        /// <returns></returns>
        public static List<T> GetList<T>(this DbContext dbContext, string sql, object? parameterObject = null, bool isCheckSql = true, bool isNormalize = true)
        {
            var dt = DtFromSql(dbContext, sql, parameterObject, isCheckSql, isNormalize);

            return dt.GetList<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="sql">參數必需為 @xxx 的格式 </param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public static DataTable DtFromSql(this DbContext dbContext, string sql, object? parameterObject = null, bool isCheckSql = true, bool isNormalize = true)
        {
            if (isNormalize)
            {
                sql = sql.SqlNormalize();
            }

            if (isCheckSql)
            {
                sql.CheckPgSqlInjection();
            }

            var dataTable = new DataTable();

            var connection = dbContext.Database.GetDbConnection();

            using (var command = connection.CreateCommand())
            {
                // 设置 SQL 查询
                command.CommandText = sql;
                command.CommandType = CommandType.Text;

                // 添加查询参数
                if (parameterObject != null)
                {
                    var parameters = new List<NpgsqlParameter>();
                    foreach (PropertyInfo prop in parameterObject.GetType().GetProperties())
                    {
                        object value = prop.GetValue(parameterObject) ?? DBNull.Value;  // 处理 null 值
                        parameters.Add(new NpgsqlParameter("@" + prop.Name, value));
                    }
                    command.Parameters.AddRange(parameters.ToArray());
                }

                // 打开连接
                bool isClose = false;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    isClose = true;
                }

                try
                {
                    // 执行查询并将结果加载到 DataTable
                    using (var reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (isClose && connection.State == ConnectionState.Open)
                    {
                        //自已開的要自己關
                        connection.Close();
                    }
                }
            }

            return dataTable;
        }

        /// <summary>
        /// 最後會 detach 物件, entry.State = EntityState.Detached;
        /// </summary>
        /// <typeparam name="T">Table</typeparam>
        /// <param name="dto">參數物件</param>
        /// <param name="dbContext">資料庫實例</param>
        /// <returns>新建立模型</returns>
        public static T Create<T>(this DbContext dbContext, object dto) where T : class
        {
            var entity = dto.CopyTo<T>();

            var entry = dbContext.Set<T>().Add(entity);
            dbContext.SaveChanges();

            entry.State = EntityState.Detached;

            return entity;
        }

        /// <summary>
        /// 若是傳入物件不是 T, 使用 dto.CopyTo 來建立一個新的物件 T; 若是傳入物件即為 T, 則預設不會使用 dto.CopyTo 來建立新物件
        /// 新增後, 會 Detached 該物件;
        /// </summary>
        /// <typeparam name="T">Table 相對的類別</typeparam>
        /// <param name="dto">參數物件</param>
        /// <param name="dbContext">資料庫實例</param>
        /// <param name="modifierInfo">修改人資訊，為避免遺漏，若沒有相關欄位，必需手動傳入 null。</param>
        /// <param name="isAddHistory">是否自動增加 Log, 使用時, 會預設 isCommit 為 Y, 所以使用時最好要加上 transaction。</param>
        /// <typeparam name="isAlwaysCopy">Table</typeparam>
        /// <returns>新建立模型</returns>
        public static async Task<T> CreateAsync<T>(this DbContext dbContext, object dto,
            object? additionInfo = null,
            bool isAlwaysCopy = false,
            string? creatorAndModifierUid = null) where T : class
        {
            T entity = (!isAlwaysCopy && dto is T) ? (T)dto : dto.CopyTo<T>();

            if (additionInfo != null)
            {
                additionInfo.CopyTo(entity);
            }

            if (!string.IsNullOrEmpty(creatorAndModifierUid))
            {
                new
                {
                    CreatedAt = DateTime.Now,
                    CreatorUid = creatorAndModifierUid,
                    ModifiedAt = DateTime.Now,
                    ModifierUid = creatorAndModifierUid,
                }
                .CopyTo(entity);
            }

            var entry = dbContext.Set<T>().Add(entity);

            await dbContext.SaveChangesAsync();

            entry.State = EntityState.Detached;

            return entity;
        }

        /// <summary>
        /// Update 指令, 
        /// isAddHistory 為 true 時, 使用預設欄位 Id, 
        /// 要注意大小寫。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="columns"></param>
        /// <param name="onlyColumns"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="modifierUid"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this DbContext dbContext,
            Dictionary<Su.PgSql.ColumnName, object> columns,
            string? onlyColumns = null,
            string primaryKeys = "Uid",
            string? modifierUid = null) where T : class
        {
            var dictionary = columns.ToDictionary(r => r.Key.ToString(), r => (object?)r.Value);
            return await dbContext.UpdateAsync<T>(columns: dictionary, onlyColumns, primaryKeys, modifierUid);
        }

        /// <summary>
        /// Update 指令, 會 SaveChangesAsync
        /// isAddHistory 為 true 時, 使用預設欄位 Id, 
        /// 要注意大小寫。
        /// </summary>
        /// <typeparam name="T">Table</typeparam>
        /// <param name="dbContext">資料庫實例</param>
        /// <param name="columns">要修改的欄位和內容</param>
        /// <param name="onlyColumns"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="modifierUid"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this DbContext dbContext,
            Dictionary<string, object?> columns,
            string? onlyColumns = null,
            string primaryKeys = "Uid",
            string? modifierUid = null) where T : class
        {
            string[] primaryKeyArray = primaryKeys.Split(',');

            T entity = (T)Activator.CreateInstance(typeof(T))!;
            var entityType = typeof(T);

            //先設定 PrimaryKeys, 再把 entity Attach 到 Conetxt 上。
            foreach (var key in primaryKeyArray)
            {
                entityType.GetProperty(key)!.SetValue(entity, columns[key]);
            }

            try
            {
                dbContext.Set<T>().Attach(entity);
            }
            catch (Exception ex)
            {
                Su.Debug.WriteLine(ex.FullInfo());
                throw;
            }

            //attach entity to db context
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry = dbContext.Entry(entity);
            List<string>? onlyColumnsList = onlyColumns?.Split(',').ToList();

            foreach (string columnName in columns.Keys)
            {
                if (!primaryKeyArray.Contains(columnName))
                {
                    if (onlyColumnsList == null || onlyColumnsList.Contains(columnName))
                    {
                        if (entityType.GetProperty(columnName) == null)
                        {
                            throw new Exception("找不到欄位: " + columnName);
                        }
                        else
                        {
                            entityType.GetProperty(columnName)!.SetValue(entity, columns[columnName]);
                            entry.Property(columnName).IsModified = true;
                        }
                    }
                }
            }

            if (modifierUid != null)
            {
                entityType.GetProperty("ModifierUid")!.SetValue(entity, modifierUid);
                entry.Property("ModifierUid").IsModified = true;
                if (entityType.GetProperty("ModifiedAt") != null)
                {
                    entityType.GetProperty("ModifiedAt")!.SetValue(entity, DateTime.Now);
                    entry.Property("ModifiedAt").IsModified = true;
                }
            }

            var c = await dbContext.SaveChangesAsync();

            entry.State = EntityState.Detached;

            return c;
        }


        /// <summary>
        /// Update 指令, 會 SaveChangesAsync
        /// </summary>
        /// <typeparam name="T">Table</typeparam>
        /// <param name="dbContext">資料庫實例</param>
        /// <param name="columns">要修改的欄位</param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this DbContext dbContext, params Su.PgSql.Column[] columns) where T : class
        {
            return await dbContext.UpdateAsync<T>(columns.ToDictionary(c => c.ColumnName, c => (object?)c.Value));
        }

        /// <summary>
        /// Update 指令
        /// </summary>
        /// <typeparam name="T">Table</typeparam>
        /// <param name="dbContext">資料庫實例</param>
        /// <param name="columns">要修改的欄位</param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this DbContext dbContext, IEnumerable<Su.PgSql.Column> columns) where T : class
        {
            return await dbContext.UpdateAsync<T>(columns.ToDictionary(c => c.ColumnName, c => (object?)c.Value));
        }

        /// <summary>
        /// Update 指令
        /// </summary>
        /// <typeparam name="T">Table</typeparam>
        /// <typeparam name="T1">參數物件型別</typeparam>
        /// <param name="id">指定更新ID</param>
        /// <param name="dto">參數物件</param>
        /// <param name="primaryKeys"></param>
        /// <param name="onlyColumns"></param>
        /// <param name="skipColumns"></param>
        /// <param name="modifierUid"></param>
        /// <param name="dbContext">資料庫實例</param>
        /// <param name="modifierInfo">修改人資訊，為避免遺漏，若沒有相關欄位，必需手動傳入 null。</param>
        public static async Task<int> UpdateAsync<T>(this DbContext dbContext, object dto,
            string primaryKeys = "Uid",
            string? onlyColumns = null,
            string? skipColumns = null,
            string? modifierUid = null) where T : class
        {
            var entry = SetUpdateObject<T>(dto, dbContext, primaryKeys, skipColumns, onlyColumns, modifierUid);
            var c = await dbContext.SaveChangesAsync();
            entry.State = EntityState.Detached;
            return c;
        }

        public static int Update<T>(this DbContext dbContext, object dto, string primaryKeys = "Uid", string? onlyColumns = null, string? skipColumns = null) where T : class
        {
            var entry = SetUpdateObject<T>(dto, dbContext, primaryKeys, skipColumns, onlyColumns);
            var c = dbContext.SaveChanges();
            entry.State = EntityState.Detached;
            return c;
        }

        static Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry SetUpdateObject<T>(object dto,
            DbContext dbContext,
            string primaryKeys,
            string? skipColumns,
            string? onlyColumns,
            string? modifierUid = null) where T : class
        {
            T entity = (T)Activator.CreateInstance(typeof(T))!;

            //先設定 PrimaryKeys, 再把 entity Attach 到 Conetxt 上。
            foreach (var key in primaryKeys.Split(','))
            {
                entity.GetType().GetProperty(key)!.SetValue(entity, dto.GetType().GetProperty(key)!.GetValue(dto, null));
            }

            try
            {
                dbContext.Set<T>().Attach(entity);
            }
            catch (Exception ex)
            {
                Su.Debug.WriteLine(ex.FullInfo());
                throw;
            }

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry = dbContext.Entry(entity);

            if (modifierUid != null)
            {
                var entityType = typeof(T);
                entityType.GetProperty("ModifierUid")!.SetValue(entity, modifierUid);
                entry.Property("ModifierUid").IsModified = true;
                if (entityType.GetProperty("ModifiedAt") != null)
                {
                    entityType.GetProperty("ModifiedAt")!.SetValue(entity, DateTime.Now);
                    entry.Property("ModifiedAt").IsModified = true;
                }
            }

            ObjUtil.CopyPropertiesTo(dto, entity, primaryKeys.Attach(skipColumns, ","), onlyColumns, entry);

            return entry;
        }

        /// <summary>
        /// 注意，這裡都是 AsNoTracking 的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="queryFunc"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetListAsync<T>(this DbContext dbContext,
            Expression<Func<T, bool>>? queryFunc = null, string? sort = null, bool IsNoTracking = true
            ) where T : class
        {
            return await GetQuery(dbContext, queryFunc, sort, IsNoTracking).ToListAsync();
        }

        public static List<dynamic> GetList<T, T1>(this DbContext dbContext, Expression<Func<T, bool>>? queryFunc = null, string? sort = null) where T : class
        {
            IQueryable<T> query = GetQuery(dbContext, queryFunc, sort);
            T1 viewEntity = (T1)Activator.CreateInstance(typeof(T1))!;
            List<string> viewEntityPropStr = viewEntity.GetType().GetProperties().Select(x => x.Name).ToList();
            return query.Select($"new {{{viewEntityPropStr.ToOneString(",")}}}").ToDynamicList();
        }

        public static async Task<PageList<dynamic>> GetPageListAsync<T, T1>(this DbContext dbContext, Expression<Func<T, bool>>? queryFunc, int page, int pageSize, string sort) where T : class
        {
            IQueryable<T> query = GetQuery(dbContext, queryFunc, sort);
            T1 viewEntity = (T1)Activator.CreateInstance(typeof(T1))!;
            List<string> viewEntityPropStr = viewEntity.GetType().GetProperties().Select(x => x.Name).ToList();

            var totalRecord = query.Count();
            var lsRecord = await query.Skip(pageSize * (page - 1))
                     .Take(pageSize).Select($"new {{{viewEntityPropStr.ToOneString(",")}}}").ToDynamicListAsync();

            return new PageList<dynamic>(lsRecord, totalRecord, page, pageSize);
        }

        public static async Task<PageList<dynamic>> GetPageListAsync<T>(this DbContext dbContext, Expression<Func<T, bool>>? queryFunc, Su.PgSql.ColumnName[] columns, int page, int pageSize, string sort) where T : class
        {
            IQueryable<T> query = GetQuery(dbContext, queryFunc, sort);
            //T1 viewEntity = (T1)Activator.CreateInstance(typeof(T1))!;
            //List<string> viewEntityPropStr = viewEntity.GetType().GetProperties().Select(x => x.Name).ToList();


            var totalRecord = query.Count();
            var lsRecord = await query.Skip(pageSize * (page - 1))
                     .Take(pageSize).Select($"new {{{columns.Select(c => c.OriginalName).ToOneString(",")}}}").ToDynamicListAsync();

            return new PageList<dynamic>(lsRecord, totalRecord, page, pageSize);
        }

        public static async Task<PageList<dynamic>> GetPageListAsync<T>(this DbContext dbContext, Expression<Func<T, bool>>? queryFunc, string columns, int page, int pageSize, string sort) where T : class
        {
            IQueryable<T> query = GetQuery(dbContext, queryFunc, sort);
            //T1 viewEntity = (T1)Activator.CreateInstance(typeof(T1))!;
            //List<string> viewEntityPropStr = viewEntity.GetType().GetProperties().Select(x => x.Name).ToList();

            var totalRecord = query.Count();
            var lsRecord = await query.Skip(pageSize * (page - 1))
                     .Take(pageSize).Select($"new {{{columns}}}").ToDynamicListAsync();

            return new PageList<dynamic>(lsRecord, totalRecord, page, pageSize);
        }


        /// <summary>
        /// queryFunc範例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="queryFunc"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PageList<T>> GetPageListAsync<T>(this DbContext dbContext, Expression<Func<T, bool>>? queryFunc, int page, int pageSize, string sort = null) where T : class
        {
            return await GetPageListAsync(GetQuery(dbContext, queryFunc, sort), (int)page, (int)pageSize);
        }

        public static async Task<PageList<T>> GetPageListAsync<T>(IQueryable<T> query, int page, int pageSize) where T : class
        {
            var totalRecord = await query.CountAsync();

            var lsRecord = await query.Skip(pageSize * (page - 1))
                 .Take(pageSize).ToListAsync();

            return new PageList<T>(lsRecord, totalRecord, page, pageSize);
        }

        /// <summary>
        /// 注意, 預設 IsNoTracking 為 true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="queryFunc"></param>
        /// <param name="sort"></param>
        /// <param name="IsNoTracking"></param>
        /// <returns></returns>
        public static IQueryable<T> GetQuery<T>(DbContext dbContext, Expression<Func<T, bool>>? queryFunc = null,
            string? sort = null, bool IsNoTracking = true) where T : class
        {
            IQueryable<T> query = dbContext.Set<T>();

            //T entity = (T)Activator.CreateInstance(typeof(T))!;

            if (queryFunc != null)
            {
                query = query.Where(queryFunc);
            }

            if (sort != null)
            {
                query = query.OrderBy(sort);
            }

            if (IsNoTracking)
            {
                return query.AsNoTracking();
            }
            else
            {
                return query;
            }
        }
    }
}
