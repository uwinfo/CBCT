using Microsoft.AspNetCore.Http;

namespace Su
{
    public class HttpContextWrapper
    {
        //在 ASP.NET Core 中，HttpContext.Current 已經被淘汰，現在是 透過依賴注入或控制器基底類別 ControllerBase 裡的 HttpContext 屬性 來存取
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextWrapper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IDictionary<object, object> Items => _httpContextAccessor.HttpContext.Items;

        private Microsoft.AspNetCore.Http.HttpContext? Context => _httpContextAccessor.HttpContext;
        public void SetCookie(string key, string value, int days = 7, bool httpOnly = true, bool secure = true, SameSiteMode sameSite = SameSiteMode.None, string path = "/")
        {
            if (Context == null) return;

            var options = new CookieOptions
            {
                Path = path,
                HttpOnly = httpOnly,
                Secure = secure,
                SameSite = sameSite,
                Expires = DateTimeOffset.UtcNow.AddDays(days)
            };

            Context.Response.Cookies.Append(key, value, options);
        }

        // 你也可以加個 GetCookie(string key) 方法來讀取 Cookie
        /// <summary>
        /// 找不到會回傳 ""
        /// </summary>
        public string? GetCookie(string key)
        {
            //本頁面重設過的 Cookie 優先。
            if (Items.ContainsKey("Cookie_" + key))
            {
                return (string)Items["Cookie_" + key];
            }

            //return Context?.Request.Cookies[key];
            string Value = "";
            bool IsOK = (bool)Context?.Request.Cookies.TryGetValue(key, out Value);
            return Value;
        }

        public void RemoveCookie(string name)
        {
            SetCookie(name, "", -1);
        }
    }
}
