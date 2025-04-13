using Microsoft.AspNetCore.Http;
using Su;

namespace Core.Middlewares
{
    /// <summary>
    /// 啟動 Cache Motitor
    /// </summary>
    public class CacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PgSqlCache _pgSqlCache;
        public CacheMiddleware(RequestDelegate next, PgSqlCache pgSqlCache)
        {
            _next = next;
            _pgSqlCache = pgSqlCache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            _pgSqlCache.StartUpdateTableCache();
        }
    }
}
