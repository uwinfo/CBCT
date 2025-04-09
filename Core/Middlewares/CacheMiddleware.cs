using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Su;
using NPOI.SS.Formula.Functions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Routing;
//using Core.Helpers;

namespace Core.Middlewares
{
    /// <summary>
    /// 啟動 Cache Motitor
    /// </summary>
    public class CacheMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public CacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            Su.PgSqlCache.StartUpdateTableCache();
        }
    }
}
