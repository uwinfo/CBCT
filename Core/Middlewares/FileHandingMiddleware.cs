using Microsoft.AspNetCore.Http;

namespace Core.Middlewares
{
    public class FileHandingMiddleware
    {
        private readonly RequestDelegate _next;
        public FileHandingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.HasFormContentType && context.Request.Form.Files.Any())
            {
                Core.Helpers.SecurityHelper.CheckFileInfoAsync(context);
            }

            await _next.Invoke(context);
        }
    }
}
