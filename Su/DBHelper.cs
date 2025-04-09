using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.ChangeTracking;
//using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Functions;
using Su;
using System.Collections.Generic;

namespace Su
{
    public class PagerClass
    {
        public int CurrentPage { get; set; } = 0;
        public int TotalPage { get; set; } = 0;
        public int TotalRecord { get; set; } = 0;
        public int StartItemIndex { get; set; } = 0;
        public int EndItemIndex { get; set; } = 0;
        public int StartPage { get; set; } = 0;
        public int EndPage { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalRecord"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageItemCount">分頁項目的數量</param>
        public PagerClass(int totalRecord, int currentPage, int pageSize, int pageItemCount = 10)
        {
            TotalRecord = totalRecord;
            CurrentPage = currentPage;

            if (TotalRecord > 0)
            {
                if ((TotalRecord % pageSize) == 0)
                {
                    TotalPage = TotalRecord / pageSize;
                }
                else
                {
                    TotalPage = (TotalRecord / pageSize) + 1;
                }

                if (CurrentPage > TotalPage)
                {
                    CurrentPage = TotalPage;
                }

                if (CurrentPage == 1)
                {
                    StartItemIndex = 1;
                }
                else
                {
                    if (TotalPage > 0)
                    {
                        StartItemIndex = ((pageSize * (CurrentPage - 1)) + 1);
                    }
                }

                if (CurrentPage < TotalPage)
                {
                    EndItemIndex = pageSize * CurrentPage;
                }
                else
                {
                    EndItemIndex = TotalRecord;
                }

                if (TotalRecord > 0)
                {
                    if (CurrentPage > pageItemCount)
                    {
                        //無條件進位
                        StartPage = (Convert.ToInt32(Math.Ceiling(float.Parse(CurrentPage.ToString()) / float.Parse(pageItemCount.ToString()))) - 1) * pageItemCount + 1;
                    }
                    else
                    {
                        StartPage = 1;
                    }
                }

                EndPage = Math.Min(TotalPage, StartPage + (pageItemCount - 1));
            }
        }
    }

    public class TransactionPageList<T> : PageList<T>
    {
        public long TotalAmount { get; set; }

        public TransactionPageList(IEnumerable<T> list, int totalRecord, int currentPage, int pageSize, long totalAmount) : base(list, totalRecord, currentPage, pageSize)
        {
            TotalAmount = totalAmount;
        }
    }

    public class MemberPageList<T>: PageList<T>
    {       
        public long VipQty;
        public long GeneralQty;
        public long TotalQty;

        public MemberPageList(IEnumerable<T> list, int totalRecord, int currentPage, int pageSize, long vipQty) : base(list, totalRecord, currentPage, pageSize)
        {
            VipQty = vipQty;
        }
    }

    public class PageList<T>
    {
        public IEnumerable<T>? List { get; set; }
        public int TotalRecord;
        public int CurrentPage;
        public int PageSize;

        public PagerClass Pager
        {
            get
            {
                return new PagerClass(TotalRecord, CurrentPage, PageSize);  
                //var totalPage = 0;
                //var startItemIndex = 0;
                //var endItemIndex = TotalRecord;
                //var startPage = 0;
                //var endPage = 0;

                //if (TotalRecord > 0)
                //{
                //    if ((TotalRecord % PageSize) == 0)
                //    {
                //        totalPage = TotalRecord / PageSize;
                //    }
                //    else
                //    {
                //        totalPage = (TotalRecord / PageSize) + 1;
                //    }

                //    if (CurrentPage > totalPage) CurrentPage = totalPage;

                //    if (CurrentPage == 1)
                //    {
                //        startItemIndex = 1;
                //    }
                //    else
                //    {
                //        if (totalPage > 0)
                //        {
                //            startItemIndex = ((PageSize * (CurrentPage - 1)) + 1);
                //        }
                //    }

                //    if (CurrentPage < totalPage)
                //    {
                //        endItemIndex = PageSize * CurrentPage;
                //    }

                //    var pageItemCount = 10;
                //    if (TotalRecord > 0)
                //    {
                //        if (CurrentPage > pageItemCount)
                //        {
                //            //無條件進位
                //            startPage = (Convert.ToInt32(Math.Ceiling(float.Parse(CurrentPage.ToString()) / float.Parse(pageItemCount.ToString()))) - 1) * pageItemCount + 1;
                //        }
                //        else
                //        {
                //            startPage = 1;
                //        }
                //    }

                //    endPage = startPage + (pageItemCount - 1);
                //    if (endPage > totalPage) endPage = totalPage;
                //}

                //return new PagerClass
                //{
                //    CurrentPage = CurrentPage,
                //    TotalPage = totalPage,
                //    TotalRecord = TotalRecord,
                //    StartItemIndex = startItemIndex,
                //    EndItemIndex = endItemIndex,
                //    StartPage = startPage,
                //    EndPage = endPage
                //};
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="page">傳入時允許 null 會比較方便 [FromQuery] 使用</param>
        /// <param name="pageSize">傳入時允許 null 會比較方便 [FromQuery] 使用</param>
        public PageList(IOrderedQueryable<T> query, int? page = 1, int? pageSize = 20)
        {
            page = page ?? 1;
            pageSize = pageSize ?? 20;

            page = Math.Max((int)page, 1);
            if(pageSize < 1)
            {
                pageSize = 20;
            }   

            var totalRecord = query.Count();
            if(totalRecord == 0)
            {
                page = 1;
            }
            else if(totalRecord <= (page - 1) * pageSize)
            {
                int r = totalRecord % (int)pageSize;
                if (r == 0)
                {
                    page = totalRecord / pageSize;
                }
                else
                {
                    page = ((totalRecord - r) / pageSize) + 1;
                }
            }

            var list = query.Skip((int)pageSize * ((int)page - 1))
                 .Take((int)pageSize).ToList();

            this.TotalRecord = totalRecord;
            this.CurrentPage = (int)page;
            this.PageSize = (int)pageSize;
            this.List = list;
        }

        public PageList(IEnumerable<T> list, int totalRecord, int currentPage, int pageSize)
        {
            //var oObj = new PageList<T>();
            this.TotalRecord = totalRecord;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.List = list;
        }

        public PagedDataTable ToPagedDataTable()
        {
            return new PagedDataTable(List.ToDataTable(), TotalRecord, CurrentPage, PageSize);
        }
    }

    public class PagedDataTable
    {
        public DataTable List; //為了讓 client 端變數名稱可以相容於 PageList，所以取名為 List
        public int TotalRecord;
        public int CurrentPage;
        public int PageSize;

        public PagerClass Pager
        {
            get
            {
                return new PagerClass(TotalRecord, CurrentPage, PageSize);
            }
        }

        public PagedDataTable(DataTable dt, int totalRecord, int currentPage, int pageSize)
        {
            this.TotalRecord = totalRecord;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.List = dt;
        }
    }


    //public class DBHelper
    //{
    //    ////queryFunc範例
    //    ////Expression<Func<T, bool>> queryFunc = (e) => "判斷式";
    //    //public static dynamic GetList<T>(Microsoft.EntityFrameworkCore.DbContext dbContext, Expression<Func<T, bool>>? queryFunc = null, string? sort = null, int? page = null, int? limit = null) where T : class
    //    //{
    //    //    var query = GetQuery(dbContext, queryFunc, sort);

    //    //    if (page != null && limit != null)
    //    //    {
    //    //        return GetPageList<T>(dbContext, query, (int)page, (int)limit);
    //    //    }

    //    //    return query.ToList();
    //    //}

    //    //public static PageList<T> GetPageList<T>(DBContext dbContext, IQueryable<T> query, int page, int limit) where T : class
    //    //{
    //    //    var totalRecord = query.Count();
    //    //    var lsRecord = query.Skip(limit * (page! - 1))
    //    //         .Take(limit).ToList();
    //    //    return GetPageList(lsRecord, totalRecord, page, limit);
    //    //}
    //    //public static dynamic GetList<T, T1>(DBContext dbContext, Expression<Func<T, bool>>? queryFunc = null, string? sort = null, int? page = null, int? limit = null) where T : class
    //    //{
    //    //    var query = GetQuery(dbContext, queryFunc, sort);

    //    //    if (page != null && limit != null)
    //    //    {
    //    //        return GetPageList<T, T1>(dbContext, query, (int)page, (int)limit);
    //    //    }

    //    //    return query.ToList().Select(x =>
    //    //    {
    //    //        T1 entity = (T1)Activator.CreateInstance(typeof(T1))!;
    //    //        DBReflection(entity, x);
    //    //        return entity;
    //    //    }).ToList();
    //    //}

    //    public static PageList<T1> GetPageList<T, T1>(DbContext dbContext, IQueryable<T> query, int page, int limit) where T : class
    //    {
    //        var totalRecord = query.Count();
    //        var lsRecord = query.Skip(limit * (page - 1))
    //                 .Take(limit).ToList().Select(x =>
    //                 {
    //                     T1 entity = (T1)Activator.CreateInstance(typeof(T1))!;
    //                     DBReflection(entity, x);
    //                     return entity;
    //                 }).ToList();
    //        return GetPageList(lsRecord, totalRecord, page, limit);
    //    }

    //    //public static IQueryable<T> GetQuery<T>(DbContext dbContext, Expression<Func<T, bool>>? queryFunc = null, string? sort = null) where T : class
    //    //{
    //    //    IQueryable<T> query = dbContext.Set<T>();

    //    //    T entity = (T)Activator.CreateInstance(typeof(T))!;

    //    //    if (entity.GetType().GetProperty("IsDeleted") != null)
    //    //    {
    //    //        query = query.Where("IsDeleted == \"N\"");
    //    //    }

    //    //    if (queryFunc != null)
    //    //    {
    //    //        query = query.Where(queryFunc);
    //    //    }

    //    //    if (sort != null)
    //    //    {
    //    //        query = query.OrderBy(sort);
    //    //    }
    //    //    return query;
    //    //}
    //    //public static T? GetOne<T>(int id, DBContext dbContext) where T : class
    //    //{
    //    //    return dbContext.Set<T>().Find(id);
    //    //}

    //    ///// <summary>
    //    ///// EF Get指令
    //    ///// </summary>
    //    ///// <typeparam name="T">Table</typeparam>
    //    ///// <typeparam name="T1">回傳物件型別</typeparam>
    //    ///// <param name="id">指定ID</param>
    //    ///// <param name="dbContext">資料庫實例</param>
    //    ///// <returns>回傳查詢結果並反射至T1物件欄位</returns>
    //    //public static T1? GetOne<T, T1>(int id, DBContext dbContext) where T : class
    //    //{
    //    //    var entity = dbContext.Set<T>().Find(id);
    //    //    if (entity == null)
    //    //    {
    //    //        return default(T1);
    //    //    }

    //    //    T1 result = (T1)Activator.CreateInstance(typeof(T1))!;
    //    //    DBReflection(result, entity);
    //    //    return result;
    //    //}

    //    /// <summary>
    //    /// EF Create指令
    //    /// </summary>
    //    /// <typeparam name="T">Table</typeparam>
    //    /// <typeparam name="T1">參數物件型別</typeparam>
    //    /// <param name="dto">參數物件</param>
    //    /// <param name="dbContext">資料庫實例</param>
    //    /// <param name="args">額外參數</param>
    //    /// <returns>新建立模型</returns>
    //    public static T Create<T, T1>(T1 dto, DbContext dbContext, object[]? args = null) where T : class
    //    {
    //        T entity = (T)Activator.CreateInstance(typeof(T))!;

    //        DBReflection(entity, dto);

    //        if (args != null && args.Any())
    //        {
    //            DBReflection(entity, args);
    //        }

    //        //var info = CreatorInfo.newOne();

    //        //DBReflection(entity, info);
    //        dbContext.Set<T>().Add(entity);
    //        dbContext.SaveChanges();

    //        return entity;
    //    }

    //    /// <summary>
    //    /// EF Create指令
    //    /// </summary>
    //    /// <typeparam name="T">Table</typeparam>
    //    /// <typeparam name="T1">參數物件型別</typeparam>
    //    /// <param name="dto">參數物件</param>
    //    /// <param name="dbContext">資料庫實例</param>
    //    /// <param name="args">額外參數</param>
    //    /// <returns>新建立模型</returns>
    //    public static T Create<T>(T dto, DbContext dbContext, object[]? args = null) where T : class
    //    {
    //        T entity = (T)Activator.CreateInstance(typeof(T))!;

    //        DBReflection(entity, dto);

    //        if (args != null && args.Any())
    //        {
    //            DBReflection(entity, args);
    //        }

    //        //var info = CreatorInfo.newOne();

    //        //DBReflection(entity, info);
    //        dbContext.Set<T>().Add(entity);
    //        dbContext.SaveChanges();

    //        return entity;
    //    }

    //    /// <summary>
    //    /// EF Update指令
    //    /// </summary>
    //    /// <typeparam name="T">Table</typeparam>
    //    /// <typeparam name="T1">參數物件型別</typeparam>
    //    /// <param name="id">指定更新ID</param>
    //    /// <param name="dto">參數物件</param>
    //    /// <param name="dbContext">資料庫實例</param>
    //    /// <param name="args">額外參數</param>
    //    public static void Update<T, T1>(int id, T1 dto, DbContext dbContext, object[]? args = null) where T : class
    //    {
    //        T entity = (T)Activator.CreateInstance(typeof(T))!;
    //        var fieldId = entity.GetType().GetProperty("Id");
    //        fieldId.SetValue(entity, id);

    //        dbContext.Set<T>().Attach(entity);
    //        var entry = dbContext.Entry(entity);

    //        DBReflection(entity, dto, entry);

    //        if (args != null && args.Any())
    //        {
    //            DBReflection(entity, args, entry);
    //        }

    //        //var info = ModifierInfo.newOne();

    //        //DBReflection(entity, info, entry);

    //        dbContext.SaveChanges();

    //        entry.State = EntityState.Detached;
    //    }
    //    /// <summary>
    //    /// EF Delete指令
    //    /// </summary>
    //    /// <typeparam name="T">Table</typeparam>
    //    /// <param name="id">指定刪除ID</param>
    //    /// <param name="dbContext">資料庫實例</param>
    //    public static void Delete<T>(int id, DbContext dbContext) where T : class
    //    {
    //        T entity = (T)Activator.CreateInstance(typeof(T))!;
    //        var fieldId = entity.GetType().GetProperty("Id");
    //        fieldId.SetValue(entity, id);

    //        dbContext.Set<T>().Attach(entity);
    //        var entry = dbContext.Entry(entity);

    //        var fieldIsDeleted = entity.GetType().GetProperty("IsDeleted");
    //        fieldIsDeleted.SetValue(entity, "Y");
    //        entry.Property("IsDeleted").IsModified = true;

    //        //var info = ModifierInfo.newOne();
    //        //DBReflection(entity, info, entry);

    //        dbContext.SaveChanges();
    //        entry.State = EntityState.Detached;
    //    }

    //    public static void DBReflection<T, T1>(T target, T1 ammunition, bool isSkipId = true)
    //    {
    //        foreach (var propOfDTO in ammunition.GetType().GetProperties())
    //        {
    //            var value = propOfDTO.GetValue(ammunition);
    //            PropertyInfo? prop = target.GetType().GetProperty(propOfDTO.Name);
    //            if (prop == null || (!IsNullable(prop) && value == null))
    //            {
    //                continue;
    //            }
    //            prop.SetValue(target, value);
    //        }
    //    }

    //    public static void DBReflection<T, T1>(T target, T1 ammunition, EntityEntry entry)
    //    {
    //        foreach (var propOfDTO in ammunition.GetType().GetProperties())
    //        {
    //            var value = propOfDTO.GetValue(ammunition);
    //            PropertyInfo? prop = target.GetType().GetProperty(propOfDTO.Name);
    //            if (prop == null || (!IsNullable(prop) && value == null))
    //            {
    //                continue;
    //            }
    //            prop.SetValue(target, value);
    //            entry.Property(propOfDTO.Name).IsModified = true;
    //        }
    //    }

    //    public static void DBReflection<T, T1>(T target, T1 ammunition, EntityEntry entry, List<string> lsDetach)
    //    {
    //        foreach (var propOfDTO in ammunition.GetType().GetProperties())
    //        {
    //            var value = propOfDTO.GetValue(ammunition);
    //            PropertyInfo? prop = target.GetType().GetProperty(propOfDTO.Name);
    //            if (prop == null || (!IsNullable(prop) && value == null))
    //            {
    //                continue;
    //            }
    //            prop.SetValue(target, value);
    //            entry.Property(propOfDTO.Name).IsModified = true;
    //            lsDetach.Add(prop.Name);
    //        }
    //    }
    //    public static bool IsNullable(PropertyInfo property)
    //    {
    //        return !property.PropertyType.IsValueType ? true : Nullable.GetUnderlyingType(property.PropertyType) != null;
    //    }

    //    //public static DBContext GetDBContext()
    //    //{
    //    //    DbContextOptionsBuilder<DBContext> builder = new DbContextOptionsBuilder<DBContext>();
    //    //    builder.UseSqlServer(Su.WU.DBConnectionString);
    //    //    return new DBContext(builder.Options);
    //    //}

    //    public static PageList<T> GetPageList<T>(IEnumerable<T> _list, int totalRecord, int pageNum, int pageSize)
    //    {
    //        var oObj = new PageList<T>();
    //        oObj._totalRecord = totalRecord;
    //        oObj._currentPage = pageNum;
    //        oObj._pageSize = pageSize;
    //        oObj.list = _list;
    //        return oObj;
    //    }

    //    public class PageList<T>
    //    {
    //        public IEnumerable<T>? list { get; set; }
    //        public int _totalRecord;
    //        public int _currentPage;
    //        public int _pageSize;

    //        public Pager pager
    //        {
    //            get
    //            {
    //                var totalPage = 0;
    //                var startItemIndex = 0;
    //                var endItemIndex = _totalRecord;
    //                var startPage = 0;
    //                var endPage = 0;

    //                if (_totalRecord > 0)
    //                {
    //                    if ((_totalRecord % _pageSize) == 0)
    //                    {
    //                        totalPage = _totalRecord / _pageSize;
    //                    }
    //                    else
    //                    {
    //                        totalPage = (_totalRecord / _pageSize) + 1;
    //                    }

    //                    if (_currentPage > totalPage) _currentPage = totalPage;

    //                    if (_currentPage == 1)
    //                    {
    //                        startItemIndex = 1;
    //                    }
    //                    else
    //                    {
    //                        if (totalPage > 0)
    //                        {
    //                            startItemIndex = ((_pageSize * (_currentPage - 1)) + 1);
    //                        }
    //                    }

    //                    if (_currentPage < totalPage)
    //                    {
    //                        endItemIndex = _pageSize * _currentPage;
    //                    }

    //                    var pageItemCount = 10;
    //                    if (_totalRecord > 0)
    //                    {
    //                        if (_currentPage > pageItemCount)
    //                        {
    //                            //無條件進位
    //                            startPage = (Convert.ToInt32(Math.Ceiling(float.Parse(_currentPage.ToString()) / float.Parse(pageItemCount.ToString()))) - 1) * pageItemCount + 1;
    //                        }
    //                        else
    //                        {
    //                            startPage = 1;
    //                        }
    //                    }

    //                    endPage = startPage + (pageItemCount - 1);
    //                    if (endPage > totalPage) endPage = totalPage;
    //                }

    //                return new Pager
    //                {
    //                    currentPage = _currentPage,
    //                    totalPage = totalPage,
    //                    totalRecord = _totalRecord,
    //                    startItemIndex = startItemIndex,
    //                    endItemIndex = endItemIndex,
    //                    startPage = startPage,
    //                    endPage = endPage
    //                };
    //            }
    //        }
    //        public class Pager
    //        {
    //            public int currentPage { get; set; }
    //            public int totalPage { get; set; }
    //            public int totalRecord { get; set; }
    //            public int startItemIndex { get; set; }
    //            public int endItemIndex { get; set; }
    //            public int startPage { get; set; }
    //            public int endPage { get; set; }
    //        }
    //    }
    //}
}
