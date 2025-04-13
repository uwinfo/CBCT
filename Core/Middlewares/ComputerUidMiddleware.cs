using Core.Helpers;
using Microsoft.AspNetCore.Http;

namespace Core.Middlewares
{
    public class ComputerUidMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SystemHelper _systemHelper;
        public ComputerUidMiddleware(RequestDelegate next, SystemHelper systemHelper)
        {
            _next = next;
            _systemHelper = systemHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(Constants.System.WwwComputerUidCookieName, out string? computerUid) && !string.IsNullOrEmpty(computerUid))
            {
                _systemHelper.ComputerUid = computerUid;
            }
            else
            {                
                var newComputerId = Guid.NewGuid().ToString("N");
                context.Response.Cookies.Append(Constants.System.WwwComputerUidCookieName, newComputerId, new CookieOptions
                {
                    HttpOnly = true, // Prevents client-side scripts from accessing the cookie
                    Secure = true,   // Ensures the cookie is sent only over HTTPS
                    SameSite = SameSiteMode.Strict, // Prevents cross-site cookie usage
                    Expires = DateTimeOffset.UtcNow.AddYears(100) // Optional: Set cookie expiration
                });

                _systemHelper.ComputerUid = newComputerId;
            }

            await _next.Invoke(context);
        }
    }
}
