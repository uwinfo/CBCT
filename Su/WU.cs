using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;

namespace Su
{
    public class ResponEndException : Exception
    {
        public ResponEndException(string message) : base(message)
        {

        }

        public ResponEndException(): base()
        {

        }
    }

    //在 ASP.NET Core 中，HttpContext.Current 已經被淘汰，現在是 透過依賴注入或控制器基底類別 ControllerBase 裡的 HttpContext 屬性 來存取
    //public static class CurrentContext
    //{
    //    private static IHttpContextAccessor m_httpContextAccessor;
    //    private static IApplicationBuilder m_app;

    //    /// <summary>
    //    /// 在叫用前, 必先使用 builder.Services.AddHttpContextAccessor(); 注入一個 IHttpContextAccessor
    //    /// 初始化, 使用方式, 在 program.cs 中, 加入 Su.HttpContext.Configure(app);
    //    /// </summary>
    //    /// <param name="app"></param>
    //    public static void Configure(IApplicationBuilder app)
    //    {
    //        m_app = app;
    //        m_httpContextAccessor = m_app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
    //    }

    //    static DefaultHttpContext _MockContext;
    //    public static void SetMockContext()
    //    {
    //        _MockContext = new DefaultHttpContext();
    //        //WU.MockSession = new Hashtable();
    //    }

    //    public static IDictionary<object, object> Items
    //    {
    //        get
    //        {
    //            if (Current == null)
    //            {
    //                return null;
    //            }

    //            return Current.Items;
    //        }
    //    }

    //    public static HttpRequest Request
    //    {
    //        get
    //        {
    //            if(Current == null)
    //            {
    //                return null;
    //            }

    //            return Current.Request;
    //        }
    //    }

    //    public static Microsoft.AspNetCore.Http.HttpContext Current
    //    {
    //        get
    //        {
    //            if(_MockContext != null)
    //            {
    //                return _MockContext;
    //            }

    //            if (m_httpContextAccessor == null)
    //            {
    //                return null;
    //            }

    //            return m_httpContextAccessor.HttpContext;
    //        }
    //    }
    //}

    public class HttpContextHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetClientIp
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                // 如果有經過 Proxy (像是 nginx)，先讀 X-Forwarded-For
                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // X-Forwarded-For 可能是多個 IP，用逗號分隔，取第一個
                    return forwardedFor.Split(',')[0];
                }

                // 否則回傳 RemoteIpAddress
                return context.Connection.RemoteIpAddress?.ToString();
            }
        }

        public bool IsLocalhost
        {
            get
            {
                return (GetClientIp == "127.0.0.1" || GetClientIp == "::1" || GetClientIp == "::ffff:127.0.0.1");
            }
        }
    }
    public class Wu
    {
        //public static bool IsTestDomain
        //{
        //    get
        //    {
        //        return CurrentContext.Current.Request.Host.Host == "localhost"
        //            || CurrentContext.Current.Request.Host.Host.Contains("bike.idv.tw")
        //            ;
        //    }
        //}

        ///// <summary>
        ///// 獲取當前請求完整的Url地址
        ///// </summary>
        ///// <returns></returns>
        //public static string GetCompleteUrl()
        //{
        //    return new StringBuilder()
        //         .Append(CurrentContext.Current.Request.Scheme)
        //         .Append("://")
        //         .Append(CurrentContext.Current.Request.Host)
        //         .Append(CurrentContext.Current.Request.PathBase)
        //         .Append(CurrentContext.Current.Request.Path)
        //         .Append(CurrentContext.Current.Request.QueryString)
        //         .ToString();
        //}

        //public static bool IsLocalhost
        //{
        //    get
        //    {
        //        return (Su.Wu.DirectIp == "127.0.0.1" || Su.Wu.DirectIp == "::1" || Su.Wu.DirectIp == "::ffff:127.0.0.1");
        //    }
        //}

        //public static string GetSessionCaptcha(string SessionKey = "captcha")
        //{
        //    return Su.Wu.ReadSession(SessionKey);
        //}

        //static List<Brush> CaptchaBrushes = null;
        //public static FileStreamResult CreateCaptcha(string captcha)
        //{
        //    if (CaptchaBrushes == null)
        //    {
        //        CaptchaBrushes = new List<Brush>();
        //        CaptchaBrushes.Add(Brushes.White);
        //        CaptchaBrushes.Add(Brushes.Gold);
        //        CaptchaBrushes.Add(Brushes.LightSkyBlue);
        //        CaptchaBrushes.Add(Brushes.LimeGreen);
        //        CaptchaBrushes.Add(Brushes.AliceBlue);
        //        CaptchaBrushes.Add(Brushes.AntiqueWhite);
        //        CaptchaBrushes.Add(Brushes.BurlyWood);
        //        CaptchaBrushes.Add(Brushes.Silver);
        //    }

        //    int width = 90;
        //    int height = 45;

        //    //https://stackoverflow.com/questions/61365732/cannot-access-a-closed-stream-when-returning-filestreamresult-from-c-sharp-netc
        //    //Using statements close and unload the variable from memory set in the using statement which is why you are getting an error trying to access a closed memory stream.You don't need to use a using statement if you are just going to return the result at the end.

        //    //這個 memory stream 不用關閉或 dispose
        //    var ms = new MemoryStream();

        //    // 釋放所有在 GDI+ 所佔用的記憶體空間 ( 非常重要!! )
        //    using (Bitmap _bmp = new Bitmap(width, height))
        //    using (Graphics _graphics = Graphics.FromImage(_bmp))
        //    using (Font _font = new Font("Courier New", 24, FontStyle.Bold)) // _font 設定要出現在圖片上的文字字型、大小與樣式
        //    {
        //        // (封裝 GDI+ 繪圖介面) 所有繪圖作業都需透過 Graphics 物件進行操作
        //        _graphics.Clear(System.Drawing.Color.Black);

        //        // 如果想啟用「反鋸齒」功能，可以將以下這行取消註解
        //        //_graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        //        // 將亂碼字串「繪製」到之前產生的 Graphics 「繪圖板」上
        //        var x = 10;
        //        for(var i = 0; i < captcha.Length; i++)
        //        {
        //            _graphics.DrawString(captcha.Substring(i, 1), _font, CaptchaBrushes[Su.MathUtil.GetRandomInt(CaptchaBrushes.Count)], x, Su.MathUtil.GetRandomInt(15));
        //            x += 10 + Su.MathUtil.GetRandomInt(10);
        //        }

        //        // 畫線

        //        _graphics.DrawLine(new Pen(CaptchaBrushes[Su.MathUtil.GetRandomInt(CaptchaBrushes.Count)], 1), 
        //            Su.MathUtil.GetRandomInt(0, Convert.ToInt32((width * 0.9 / 2))), 0, Su.MathUtil.GetRandomInt(Convert.ToInt32(width / 2), Convert.ToInt32(width * 1.9 / 2)), height);

        //        _graphics.DrawLine(new Pen(CaptchaBrushes[Su.MathUtil.GetRandomInt(CaptchaBrushes.Count)], 1), 
        //            Su.MathUtil.GetRandomInt(Convert.ToInt32(width / 2), Convert.ToInt32(width * 1.9 / 2)), 0, Su.MathUtil.GetRandomInt(0, Convert.ToInt32((width * 0.9 / 2))), height);

        //        _graphics.DrawLine(new Pen(CaptchaBrushes[Su.MathUtil.GetRandomInt(CaptchaBrushes.Count)], 1), 
        //            0,
        //            Su.MathUtil.GetRandomInt(height / 2),
        //            width,
        //            height / 2 + Su.MathUtil.GetRandomInt(height / 2)
        //            );

        //        _graphics.DrawLine(new Pen(CaptchaBrushes[Su.MathUtil.GetRandomInt(CaptchaBrushes.Count)], 1),
        //           0,
        //           height / 2 + Su.MathUtil.GetRandomInt(height / 2),
        //           width,
        //            Su.MathUtil.GetRandomInt(height / 2)
        //           );

        //        _bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    }

        //    ms.Seek(0, SeekOrigin.Begin);

        //    // Controller 的型別為 FileResult
        //    return new FileStreamResult(ms, "image/jpeg")
        //    { FileDownloadName = $"{DateTime.Now.Ymdhmsf()}.jpg" };
        //}

        //public static string CreateCaptcha(string captchaPath, int Weight, int Height, string SessionKey = "captcha")
        //{
        //    string filename = DateTime.Now.Ymdhmsf() + ".gif";

        //    // 產生一個 4 個字元的亂碼字串，並直接寫入 Session 裡
        //    string captcha = Su.MathUtil.GetRandomInt(1000, 10000).ToString();
        //    Su.Wu.AddSession(SessionKey, captcha);

        //    //清除舊檔案, 最短只保留 3 分鐘.
        //    Su.FileUtility.RemoveOldFile(captchaPath, DateTime.Now.AddMinutes(-3));

        //    // 釋放所有在 GDI+ 所佔用的記憶體空間 ( 非常重要!! )
        //    using (Bitmap _bmp = new Bitmap(Weight, Height))
        //    using (Graphics _graphics = Graphics.FromImage(_bmp))
        //    using (Font _font = new Font("Courier New", 24, FontStyle.Bold)) // _font 設定要出現在圖片上的文字字型、大小與樣式
        //    {
        //        // (封裝 GDI+ 繪圖介面) 所有繪圖作業都需透過 Graphics 物件進行操作
        //        _graphics.Clear(System.Drawing.Color.Black);

        //        // 如果想啟用「反鋸齒」功能，可以將以下這行取消註解
        //        //_graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        //        // 將亂碼字串「繪製」到之前產生的 Graphics 「繪圖板」上
        //        _graphics.DrawString(captcha, _font, Brushes.White, 3, 3);

        //        // 畫兩條線
        //        var P = new Pen(Brushes.White, 1);
        //        _graphics.DrawLine(P, Su.MathUtil.GetRandomInt(0, Convert.ToInt32((Weight * 0.9 / 2))), 0, Su.MathUtil.GetRandomInt(Convert.ToInt32(Weight / 2), Convert.ToInt32(Weight * 1.9 / 2)), Height);
        //        _graphics.DrawLine(P, Su.MathUtil.GetRandomInt(Convert.ToInt32(Weight / 2), Convert.ToInt32(Weight * 1.9 / 2)), 0, Su.MathUtil.GetRandomInt(0, Convert.ToInt32((Weight * 0.9 / 2))), Height);

        //        _bmp.Save(captchaPath + filename);
        //        _bmp.Dispose();
        //    }

        //    return filename;
        //} 

        ///// <summary>
        ///// 不包括 https://www.abc.com  的部份.
        ///// </summary>
        //public static string URL
        //{
        //    get
        //    {
        //        var httpRequestFeature = Su.CurrentContext.Current.Features.Get<IHttpRequestFeature>();

        //        return httpRequestFeature.RawTarget;
        //    }
        //}

        ///// <summary>
        ///// {Su.CurrentContext.Current.Request.Scheme}://{Su.CurrentContext.Current.Request.Host}
        ///// </summary>
        //public static string Host
        //{
        //    get
        //    {
        //        return $"{Su.CurrentContext.Current.Request.Scheme}://{Su.CurrentContext.Current.Request.Host}";
        //    }
        //}

        //public static string FullURL
        //{
        //    get
        //    {
        //        return $"{Su.CurrentContext.Current.Request.Scheme}://{Su.CurrentContext.Current.Request.Host}{Su.CurrentContext.Current.Request.Path}{Su.CurrentContext.Current.Request.QueryString}";
        //    }
        //}

        public static ContentResult RedirectMessageContent(Controller controller, string msg, string nextURL = "/", int delaySeconds = 5)
        {
            return controller.Content(msg + ", " + delaySeconds + " 秒後跳轉.<script>window.setTimeout('location.href=\"" + nextURL + "\";', " + delaySeconds * 100 + ");</script>", "text/html", System.Text.Encoding.UTF8);
        }

        //static Random oRandomGenerator = new Random();
        public static String getRnd(Int32 CodeLength)
        {
            //char[] chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            ////生成驗證碼字串
            //String sCode = "";
            //for (int i = 0; i < CodeLength; i++)
            //{
            //    sCode += chars[oRandomGenerator.Next(chars.Length)];
            //}

            return TextFns.GetRandomString(CodeLength);
        }
        public static string getMD5str(string source)
        {
            Byte[] data2ToHash = ConvertStringToByteArray(source);
            byte[] hashvalue2 = new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(data2ToHash);
            return BitConverter.ToString(hashvalue2);
        }
        public static Byte[] ConvertStringToByteArray(String s)
        {
            return (new System.Text.UTF8Encoding()).GetBytes(s);
        }

        

        ///// <summary>
        ///// 注意, 物件會由 JSON 字串轉回.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static T ReadSession<T>(string name)
        //{
        //    string value = ReadSession(name);

        //    if (string.IsNullOrEmpty(value))
        //    {
        //        return default(T);
        //    }

        //    return value.JsonDeserialize<T>();
        //}

        //public static Hashtable MockSession;

        //public static void AddSession(string name, string value)
        //{
        //    if (value == null)
        //    {
        //        value = "";
        //    }

        //    if (MockSession != null)
        //    {
        //        //Unit Test
        //        MockSession[name] = value;
        //    }
        //    else
        //    {
        //        Su.CurrentContext.Current.Session.SetString(name, value);
        //    }
            
        //}

        ///// <summary>
        ///// 注意, 物件會被轉為 JSON 字串後再存入.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        //public static void AddSession(string name, object value)
        //{
        //    if (value == null)
        //    {
        //        value = "";
        //    }

        //    AddSession(name, value.Json());
        //}

        //public static string ReadSession(string name)
        //{
        //    string Value = "";
        //    if (MockSession != null)
        //    {
        //        //Unit Test
        //        return (string)MockSession[name];
        //    }
        //    else
        //    {
        //        try
        //        {
        //            Value = Su.CurrentContext.Current.Session.GetString(name);
        //        }
        //        catch (Exception)
        //        {

        //        }
        //        return Value;
        //    }
        //}

        //public static void RemoveCookie(string name)
        //{
        //    AddCookie(name, "", expire: DateTime.Now.AddDays(-1));
        //}

        ///// <summary>
        ///// 注意, 預設 HttpOnly = true, Secure = falsetrue
        ///// </summary>
        ///// <param name="name"></param>
        ///// <param name="value"></param>
        ///// <param name="IsHttpOnly"></param>
        ///// <param name="expire"></param>
        ///// <param name="Domain"></param>
        ///// <param name="IsSecure"></param>
        //public static void AddCookie(string name, string value, bool IsHttpOnly = true, DateTime? expire = null, string Domain = null, bool IsSecure = true, SameSiteMode sameSiteMode = SameSiteMode.None)
        //{
        //    var co = new Microsoft.AspNetCore.Http.CookieOptions();
        //    if (expire != null)
        //    {
        //        co.Expires = (DateTime)expire;
        //    }

        //    if (Domain != null)
        //    {
        //        co.Domain = Domain;
        //    }

        //    co.HttpOnly = IsHttpOnly;
        //    co.Secure = IsSecure;
        //    co.SameSite = sameSiteMode;

        //    Su.CurrentContext.Current.Response.Cookies.Append(name, value, co);

        //    if (expire != null && expire < DateTime.Now)
        //    {
        //        Su.CurrentContext.Current.Items["Cookie_" + name] = null;
        //    }
        //    else
        //    {
        //        Su.CurrentContext.Current.Items["Cookie_" + name] = value;
        //    }
        //}

        ///// <summary>
        ///// 找不到會回傳 ""
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static string ReadCookie(string name)
        //{
        //    //本頁面重設過的 Cookie 優先。
        //    if(Su.CurrentContext.Current.Items.ContainsKey("Cookie_" + name))
        //    {
        //        return (string)Su.CurrentContext.Current.Items["Cookie_" + name];
        //    }

        //    string Value = "";
        //    bool IsOK = Su.CurrentContext.Request.Cookies.TryGetValue(name, out Value);
        //    return Value;
        //}

        ///// <summary>
        ///// 設為 true 時, 會執行 CurrentContext.Current.Response.Buffer = false;
        ///// </summary>
        //public static bool IsFlusInPage
        //{
        //    get
        //    {
        //        if (Su.CurrentContext.Current.Items["IsFlusInPage"] == null)
        //        {
        //            return false;
        //        }

        //        return (bool)Su.CurrentContext.Current.Items["IsFlusInPage"];
        //    }
        //    set
        //    {
        //        Su.CurrentContext.Current.Items["IsFlusInPage"] = value;
        //    }
        //}

        //public static bool IsAddTimeFlagInPage
        //{
        //    get
        //    {
        //        if (Su.CurrentContext.Current.Items["IsAddTimeFlagInPage"] == null)
        //        {
        //            return false;
        //        }

        //        return (bool)Su.CurrentContext.Current.Items["IsAddTimeFlagInPage"];
        //    }
        //    set
        //    {
        //        Su.CurrentContext.Current.Items["IsAddTimeFlagInPage"] = value;
        //    }
        //}

        //public static void DebugWriteLine(string Message = "", bool IsHtmlEncode = false, bool IsFlush = false, bool IsAddTimeFlag = false)
        //{

        //    try
        //    {
        //        if (IsAddTimeFlag || IsAddTimeFlagInPage)
        //        {
        //            WriteText("<br>Debug: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "<br>");
        //        }

        //        if (IsHtmlEncode)
        //        {
        //            WriteText("Debug: " + HttpUtility.HtmlEncode(Message).Replace("\r\n", "<br>") + "<br>");
        //        }
        //        else
        //        {
        //            WriteText("Debug: " + Message.Replace("\r\n", "<br>") + "<br>");
        //        }

        //        //if (IsStop)
        //        //{
        //        //    CurrentContext.Current.Response.End();
        //        //}

        //        if (IsFlush || IsFlusInPage)
        //        {
        //            Su.CurrentContext.Current.Response.Body.FlushAsync();
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        ///// <summary>
        ///// WriteText 再補 br
        ///// </summary>
        ///// <param name="msg"></param>
        ///// <param name="IsFlush"></param>
        ///// <param name="httpStatusCode"></param>
        ///// <param name="contentType"></param>
        //public static void WriteLine(string msg, bool IsFlush = false, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.OK, string contentType = "text/html; charset=utf-8")
        //{
        //    WriteText(msg + "<br>", IsFlush, httpStatusCode, contentType);
        //}

        //public static void WriteText(string msg, bool IsFlush = false, System.Net.HttpStatusCode httpStatusCode = System.Net.HttpStatusCode.OK, string contentType = "text/html; charset=utf-8")
        //{
        //    if (Su.CurrentContext.Current.Items["IsSetResponseHeader"] == null)
        //    {
        //        Su.CurrentContext.Current.Response.StatusCode = (int)httpStatusCode;
        //        Su.CurrentContext.Current.Response.ContentType = contentType;

        //        //標記已設定.
        //        Su.CurrentContext.Current.Items["IsSetResponseHeader"] = true;
        //    }
        //    //Su.WU.AddLog(msg, FileName: "ClinicTime_WriteText_" + DateTime.Now.ToString("yyyyMMddHH"));
        //    Su.CurrentContext.Current.Response.WriteAsync(msg);
        //    if (IsFlush)
        //    {
        //        Su.CurrentContext.Current.Response.Body.FlushAsync();
        //    }
        //}

        ///// <summary>
        ///// ClientIP == "127.0.0.1" || ClientIP == "::1"
        ///// </summary>
        //public static bool IsLocalHost
        //{
        //    get
        //    {
        //        return ClientIP == "127.0.0.1" || ClientIP == "::1";
        //    }
        //}

        public static ContentResult ErrorResponse(HttpStatusCode  httpStatusCode,  object error)
        {

            var content = new ContentResult();
            content.StatusCode = (int)httpStatusCode;
            content.Content = error.Json(isCamelCase: true);
            content.ContentType = "application/json";

            return content;
        }

        //public static string UserAgent
        //{
        //    get
        //    {
        //        return CurrentContext.Request.Headers["User-Agent"].ToString();
        //    }
        //}

        ///// <summary>
        ///// 由 Proxy 送來的 Request, 必需把 RemoteIpAddress 放在 "CLIENT_REMOTE_ADDR" 的 Header 裡.
        ///// HTTP_X_FORWARDED_FOR 也要保有原的內容
        ///// </summary>
        //public static string IpxFromProxy
        //{
        //    get
        //    {
        //        try
        //        {

        //            if (CurrentContext.Current == null
        //            || CurrentContext.Current.Request == null
        //            || CurrentContext.Current.Request.Headers == null)
        //            {
        //                return null;
        //            }

        //            //先檢查是否有經過代理伺服器
        //            if (string.IsNullOrEmpty(CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"]))
        //            {
        //                if (string.IsNullOrEmpty(CurrentContext.Current.Request.Headers["CLIENT_REMOTE_ADDR"]))
        //                {
        //                    throw new Exception("由 Proxy 送來的 Request, 必需把 RemoteIpAddress 放在 \"CLIENT_REMOTE_ADDR\" 的 Header 裡. HTTP_X_FORWARDED_FOR 也要保有原的內容");
        //                }

        //                return CurrentContext.Current.Request.Headers["CLIENT_REMOTE_ADDR"];
        //            }

        //            return GetFirstXforwardedIp(CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"], CurrentContext.Current.Request.Headers["CLIENT_REMOTE_ADDR"]);
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 優先傳回 HTTP_X_FORWARDED_FOR 的第一個 IP, 無連線時(例如在 Thread 之中)會回傳 null
        ///// 但排除 127.0.0.1, 10.x.x.x, 192.168.x.x, 172.16.0.0 — 172.31.255.255
        ///// 沒有 HTTP_X_FORWARDED_FOR 時，回傳 REMOTE_ADDR
        ///// </summary>
        //public static string IpX
        //{
        //    get
        //    {
        //        try
        //        {
                    
        //            if (CurrentContext.Current == null
        //            || CurrentContext.Current.Request == null
        //            || CurrentContext.Current.Request.Headers == null)
        //            {
        //                return null;
        //            }

        //            //先檢查是否有經過代理伺服器
        //            if (string.IsNullOrEmpty(CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"]))
        //            {
        //                //CLIENT_REMOTE_ADDR
        //                return CurrentContext.Current.Connection.RemoteIpAddress.ToString();
        //            }

        //            return GetFirstXforwardedIp(CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"], CurrentContext.Current.Connection.RemoteIpAddress.ToString());
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 回傳 CurrentContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]
        ///// </summary>
        //public static string XforwardedFor
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (CurrentContext.Current == null
        //            || CurrentContext.Current.Request == null
        //            || CurrentContext.Current.Request.Headers == null)
        //            {
        //                return null;
        //            }

        //            return CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"];
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    }
        //}

        /// <summary>
        /// 排除 127.0.0.1, 10.x.x.x, 192.168.x.x, 172.16.0.0 — 172.31.255.255, fc00::/7
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIp(string ip)
        {
            if (ip == "127.0.0.1" || ip.StartsWith("192.168") || ip.StartsWith("10."))
            {
                return true;
            }

            if (ip.StartsWith("172.")) //172.16.0.0 — 172.31.255.255
            {
                var second = Int32.Parse(ip.Split('.')[1]);
                if (second >= 16 && second <= 32)
                {
                    return true;
                }
            }

            //IPv6: fc00::/7 = fc:00/8 + fd:00/8
            //fd: 自定義 subnet
            //fc: RFC 4193
            //- L(Local) bit(第 8 個 bit)  值為1代表Global ID是local自行設定。值為0代表Global ID是依照RFC 4193的演算法生成的。
            if (ip.ToLower().StartsWith("fc") || ip.ToLower().StartsWith("fd"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// X-Forwarded-For: client,proxy1,proxy2
        /// the rightmost IP address is the IP address of the most recent proxy and the leftmost IP address is the IP address of the originating client.
        /// 有時我們會抓最近的 Proxy IP (最無法仿冒的)當作 Client IP。(CDN 的前一個 IP)
        /// </summary>
        /// <param name="XforwardedIp"></param>
        /// <param name="ConnetedIp"></param>
        /// <returns></returns>
        public static string GetRightmostIp(string XforwardedIp, string ConnetedIp)
        {
            if (string.IsNullOrEmpty(XforwardedIp))
            {
                return ConnetedIp;
            }

            var ips = XforwardedIp.Split(',');
            return ips[^1];
        }

        ///// <summary>
        ///// 內部 API Server 用的 XforwardedIp
        ///// </summary>
        ///// <returns></returns>
        //public static string XforwardedIpForInnerApi()
        //{
        //    return CurrentContext.Current.Request.Headers[CLIENT_HTTP_X_FORWARDED_FOR].ToString();
        //}

        ///// <summary>
        ///// 提供接收 SendJson 呼叫的內部 API, 取得 CDN 的前一個 IP。若是沒有 X_FORWARDED_FOR ，則回傳連線用的 IP
        ///// </summary>
        ///// <returns></returns>
        //public static string RightmostIpForInnerApi()
        //{
        //    return GetRightmostIp(XforwardedIpForInnerApi(), DirectIpForInnerApi());
        //}

        ///// <summary>
        ///// 內部 API Server 用的 Direct Ip，
        ///// 使用 SendJson 時會自動帶一個 CLIENT_REMOTE_ADDR 的 Header。
        ///// 若不存在 CLIENT_REMOTE_ADDR，則回傳 DirectIp 的值。
        ///// </summary>
        ///// <returns></returns>
        //public static string DirectIpForInnerApi()
        //{
        //    if (string.IsNullOrEmpty(CurrentContext.Current.Request.Headers[CLIENT_REMOTE_ADDR]))
        //    {
        //        return DirectIp;
        //    }
        //    else
        //    {
        //        return CurrentContext.Current.Request.Headers[CLIENT_REMOTE_ADDR].ToString();
        //    }
        //}

        //public static string AllIpForInnerApi()
        //{
        //    return DirectIpForInnerApi() + (string.IsNullOrEmpty(XforwardedIpForInnerApi()) ? "" : $"({XforwardedIpForInnerApi()})");
        //}

        ///// <summary>
        ///// X-Forwarded-For: client,proxy1,proxy2, 這裡會回傳最左側的 IP (排除 Private IP 網段), 這裡抓的是理論上的使用者 IP, 但有可能被假的。
        ///// 這也不一定是 CDN 之後的第一個 IP 哦。the rightmost IP address is the IP address of the most recent proxy and the leftmost IP address is the IP address of the originating client.
        ///// </summary>
        ///// <param name="XforwardedIp"></param>
        ///// <param name="ConnetedIp"></param>
        ///// <returns></returns>
        //static string GetFirstXforwardedIp(string XforwardedIp, string ConnetedIp)
        //{
        //    var ips = XforwardedIp.Split(',');
        //    return ips.Where(x => !IsPrivateIp(x)).FirstOrDefault() ?? ConnetedIp;
        //}

        ///// <summary>
        ///// 記錄完整 IP 用: REMOTE_ADDR("HTTP_X_FORWARDED_FOR")
        ///// </summary>
        //public static string IpAll
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (CurrentContext.Current == null
        //            || CurrentContext.Current.Request == null
        //            || CurrentContext.Current.Request.Headers == null)
        //            {
        //                return null;
        //            }

        //            var ip = CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"];
        //            if (string.IsNullOrEmpty(ip))
        //            {
        //                return CurrentContext.Current.Request.HttpContext.Connection.RemoteIpAddress.ToString();
        //            }
        //            else
        //            {
        //                return CurrentContext.Current.Request.HttpContext.Connection.RemoteIpAddress.ToString() + "(" + ip + ")";
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    }
        //}


        ///// <summary>
        ///// 直接連線的 IP Connection.RemoteIpAddress
        ///// </summary>
        //public static string DirectIp
        //{
        //    get
        //    {
        //        //return CurrentContext.Current.Request.Headers["REMOTE_ADDR"];
        //        return CurrentContext.Current.Connection.RemoteIpAddress.ToString();
        //    }
        //}

        ///// <summary>
        ///// 要注意, 沒有 Response.End
        ///// contentType 會送出 application/json
        ///// </summary>
        ///// <param name="Obj"></param>
        ///// <param name="IsEnd"></param>
        ///// <param name="DateFormat"></param>
        //public static void WriteJSON(object Obj , bool IsEnd = true, string DateFormat = null)
        //{
        //    if (DateFormat == null)
        //    {
        //        WriteText(Newtonsoft.Json.JsonConvert.SerializeObject(Obj), contentType: "application/json; charset=utf-8" , IsFlush:true);
        //    }
        //    else
        //    {
        //        WriteText(Newtonsoft.Json.JsonConvert.SerializeObject(Obj, Newtonsoft.Json.Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = DateFormat }), contentType: "application/json; charset=utf-8", IsFlush: true);
        //    }
        //}

        ///// <summary>
        ///// 會自動把 IP 資訊加入 Header.
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="inputName"></param>
        ///// <param name="uploadFile"></param>
        ///// <param name="data"></param>
        ///// <param name="authorizationToken"></param>
        ///// <returns></returns>
        ///// <exception cref="Exception"></exception>
        //public static async Task<T> PostFileAsync<T>(string url, Dictionary<string, IFormFile> postFiles, Dictionary<string, string?> data = null, string authorizationToken = null)
        //{
        //    string? result = null;
        //    try
        //    {
        //        //如果出現: "無法對不會寫入資料的作業設定 Content-Length 或區塊編碼。" 請檢查 Method 是否正確。
        //        HttpClientHandler clientHandler = new HttpClientHandler();
        //        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

        //        using (HttpClient httpClient = new HttpClient(clientHandler))
        //        {
        //            MultipartFormDataContent form = new MultipartFormDataContent();

        //            if(authorizationToken != null)
        //            {
        //                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
        //            }

        //            if (CurrentContext.Current != null
        //                    && CurrentContext.Current.Request != null
        //                    && CurrentContext.Current.Request.Headers != null)
        //            {
        //                httpClient.DefaultRequestHeaders.Add("CLIENT_HTTP_X_FORWARDED_FOR", CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString());

        //                httpClient.DefaultRequestHeaders.Add("CLIENT_REMOTE_ADDR", CurrentContext.Current.Connection.RemoteIpAddress.ToString());
        //            }

        //            if(postFiles != null)
        //            {
        //                foreach (string key in postFiles.Keys)
        //                {
        //                    var uploadFile = postFiles[key];
        //                    if (uploadFile != null && uploadFile.Length > 0)
        //                    {
        //                        using var stream = uploadFile.OpenReadStream();
        //                        var streamContent = new StreamContent(stream);
        //                        var fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result);
        //                        form.Add(fileContent, key, uploadFile.FileName);
        //                    }
        //                }
        //            }

        //            if(data != null)
        //            {
        //                foreach (string key in data.Keys)
        //                {
        //                    if(data[key] != null)
        //                    {
        //                        var c = new StringContent(data[key]!);
        //                        form.Add(c, key);
        //                    }
        //                }
        //            }

        //            HttpResponseMessage response = await httpClient.PostAsync(url, form);

        //            result = await response.Content.ReadAsStringAsync();

        //            if (response.StatusCode != System.Net.HttpStatusCode.OK)
        //            {
        //                throw new Su.ApiException(response.StatusCode, result);
        //            }

        //            return result.JsonDeserialize<T>();
        //        }
        //    }
        //    catch (System.Net.WebException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Su.ApiException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"url: {url}, json: {data.Json()}, authorizationToken: {authorizationToken},  ex: {ex.FullInfo()}, result: {result}");
        //    }
        //}

        ///// <summary>
        ///// 用 x-www-form-urlencoded
        ///// 若是要略過 SSL 憑証檢查, 請在 global.asax 加入 ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(U2.WU.ValidateServerCertificate);
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <param name="nvc"></param>
        ///// <param name="oE"></param>
        ///// <returns></returns>
        //public static string Post(string uri, NameValueCollection nvc, Encoding oE = null)
        //{
        //    if ((oE == null))
        //    {
        //        oE = Encoding.GetEncoding("utf-8");
        //    }

        //    using (WebClient wc = new WebClient())
        //    {
        //        wc.Encoding = oE;
        //        wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

        //        return oE.GetString(wc.UploadValues(uri, "POST", nvc));
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="uri"></param>
        ///// <param name="oE">預設 utf-8</param>
        ///// <param name="TimeoutSec"></param>
        ///// <returns></returns>
        //public static string GetRemotePage(string uri, Encoding oE = null, int TimeoutSec = 0)
        //{
        //    if ((oE == null))
        //    {
        //        oE = Encoding.GetEncoding("utf-8");
        //    }

        //    var rq = WebRequest.Create(uri);
        //    rq.Method = "GET";
        //    rq.ContentType = "text/html";
        //    if (TimeoutSec > 0)
        //    {
        //        rq.Timeout = TimeoutSec * 1000;
        //    }

        //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //    using (var rp = rq.GetResponse())
        //    {
        //        using (var sr = new System.IO.StreamReader(rp.GetResponseStream(), oE))
        //        {
        //            return sr.ReadToEnd();
        //        }
        //    }
        //}

        //public static string GetFormValue(string Key, bool IsReturnNothing = false)
        //{
        //    if (Su.CurrentContext.Current.Request.HasFormContentType)
        //    {
        //        if (Su.CurrentContext.Current.Request.Form.ContainsKey(Key))
        //        {
        //            return Su.CurrentContext.Current.Request.Form[Key];
        //        }
        //    }

        //    if (IsReturnNothing)
        //        return null;
        //    else
        //        return "";
        //}

        //public static IFormFileCollection Files
        //{
        //    get
        //    {
        //        return Su.CurrentContext.Current.Request.Form.Files;
        //    }            
        //}

        ///// <summary>
        ///// 由 Form 或 Query String 中取得變數, 預設回傳空白
        ///// </summary>
        ///// <param name="Key"></param>
        ///// <param name="IsReturnNull"></param>
        ///// <returns></returns>
        //public static string GetValue(string Key, bool IsReturnNull = false)
        //{
        //    if (Su.CurrentContext.Current.Request.Query.ContainsKey(Key))
        //    {
        //        return Su.CurrentContext.Current.Request.Query[Key];
        //    }

        //    if (Su.CurrentContext.Current.Request.HasFormContentType)
        //    {
        //        if (Su.CurrentContext.Current.Request.Form.ContainsKey(Key))
        //        {
        //            return Su.CurrentContext.Current.Request.Form[Key];
        //        }
        //    }

        //    if (IsReturnNull)
        //        return null;
        //    else
        //        return "";
        //}

        //public static int GetIntValue(string Key, int? Defalut = null)
        //{
        //    string Value = GetValue(Key, true);

        //    if (Value == null || !Value.IsNumeric())
        //    {
        //        if (Defalut != null)
        //        {
        //            return (int)Defalut;
        //        }
        //        else
        //        {
        //            throw new Exception("Can't transfer '" + Value + "' to int.");
        //        }
        //    }

        //    return Value.Replace(",", "").ToInt32();
        //}

        //public static DateTime GetDateValue(string Key, DateTime? Default = null)
        //{
        //    string Value = GetValue(Key, true);

        //    if (Value == null || !Value.IsDate())
        //    {
        //        if (Default != null)
        //        {
        //            return (DateTime)Default;
        //        }
        //        else
        //        {
        //            throw new Exception("Can't transfer to datetime, : '" + Key + "', " + Value);
        //        }
        //    }

        //    return Value.ToDate();
        //}

        //public static bool IsNonEmptyFromQueryStringOrForm(string key)
        //{
        //    return !IsEmptyFromQueryStringOrForm(key);
        //}

        //public static bool IsEmptyFromQueryStringOrForm(string key)
        //{
        //    return string.IsNullOrEmpty(GetValue(key));
        //}

        //public static bool IsNumericFromQueryStringOrForm(string key)
        //{
        //    if (IsEmptyFromQueryStringOrForm(key))
        //    {
        //        return false;
        //    }

        //    return GetValue(key).IsNumeric();
        //}

        //public static bool IsIntFromQueryStringOrForm(string key)
        //{
        //    if (IsEmptyFromQueryStringOrForm(key))
        //    {
        //        return false;
        //    }

        //    return GetValue(key).IsInt();
        //}

        //public static bool IsFromMobile(string sHttpUserAgent = "")
        //{
        //    string key = "isFromMobile";

        //    //每一頁只要判斷一次. 重覆跑很花 CPU
        //    if (!CurrentContext.Items.ContainsKey(key))
        //    {
        //        if (sHttpUserAgent == "")
        //        {
        //            sHttpUserAgent = CurrentContext.Current.Request.Headers["User-Agent"];
        //        } 

        //        if (!string.IsNullOrEmpty(sHttpUserAgent))
        //        {
        //            Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //            Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|e\-|e\/|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|xda(\-|2|g)|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //            if ((b.IsMatch(sHttpUserAgent) || v.IsMatch(sHttpUserAgent.Substring(0, 4))))
        //            {
        //                CurrentContext.Items[key] = true;
        //            }
        //            else
        //            {
        //                CurrentContext.Items[key] = false;
        //            }
        //        }
        //        else
        //        {
        //            CurrentContext.Items[key] = false;
        //        }
        //    }

        //    return (bool)CurrentContext.Items[key];
        //}

        //public static bool IsDateFromQueryStringOrForm(string key, bool IsDateOnly = false)
        //{
        //    if (IsEmptyFromQueryStringOrForm(key))
        //    {
        //        return false;
        //    }

        //    return GetValue(key).IsDate();
        //}

        ///// <summary>
        ///// 取後 Post 過來的字串.
        ///// </summary>
        ///// <returns></returns>
        //public static async Task<string> ReadRequestInputStream()
        //{
        //    using (var reader = new System.IO.StreamReader(Su.CurrentContext.Current.Request.Body))
        //    {
        //        return await reader.ReadToEndAsync();
        //    }
        //}

        ///// <summary>
        ///// 原來的名字取的不好, 
        ///// </summary>
        ///// <returns></returns>
        //public static async Task<string> GetPostedString()
        //{
        //    return await ReadRequestInputStream();
        //}

        //public static async Task<dynamic> GetPostedDynamic()
        //{
        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(await ReadRequestInputStream());
        //}

        //public static async Task<T> GetPostedObject<T>()
        //{
        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(await ReadRequestInputStream());
        //}

        //public static void SetLogRoot(object value)
        //{
        //    throw new NotImplementedException();
        //}

        public static void InitialSetting(string authCookie, string LogRoot)
        {
            SetAuthCookieName(authCookie);
            SetLogRoot(LogRoot);
        }

        /// <summary>
        /// 記錄登入 Cookie 用
        /// </summary>
        static string? _authCookieName = null;
        public static string AuthCookieName
        {
            get
            {
                if (string.IsNullOrEmpty(_authCookieName))
                {
                    throw new Exception("請先使用 SetAuthCookieName 來設定 cookie name");
                }
                return _authCookieName;
            }
        }

        /// <summary>
        /// 設定登入 Cookie 的名稱，
        /// 注意，前後台必需使用不同的名稱。
        /// </summary>
        /// <param name="cookieName"></param>
        /// <exception cref="Exception"></exception>
        public static void SetAuthCookieName(string cookieName)
        {
            if (_authCookieName == null)
            {
                _authCookieName = cookieName;
            }
            else
            {
                throw new Exception("只能呼叫一次 SetAuthCookieName");
            }
        }



        private static string? _DataRoot = null;
        /// <summary>
        /// Rainbow站台會用到
        /// </summary>
        public static string? DataRoot
        {
            get
            {
                return _DataRoot;
            }
        }

        /// <summary>
        /// Rainbow站台會用到
        /// </summary>
        /// <param name="dataRoot"></param>
        /// <exception cref="Exception"></exception>
        public static void SetDataRoot(string dataRoot)
        {
            if (!dataRoot.EndsWith("\\") && !dataRoot.EndsWith("/"))
            {
                throw new Exception("DataRoot 請使用 \\ 或 / 結尾");
            }
            else
            {
                _DataRoot = dataRoot;
            }
        }

        //const string CLIENT_HTTP_X_FORWARDED_FOR = "CLIENT_HTTP_X_FORWARDED_FOR";
        //const string CLIENT_REMOTE_ADDR = "CLIENT_REMOTE_ADDR";

        //public static async Task<string> PostFile(string url, string inputName, string fileName,
        //    string authorizationToken = null, bool IsAddIpHeaders = true, Dictionary<string, string> datas = null)
        //{
        //    //參考 https://stackoverflow.com/questions/19954287/how-to-upload-file-to-server-with-http-post-multipart-form-data

        //    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        //    using (HttpClient httpClient = new HttpClient())
        //    {
        //        MultipartFormDataContent form = new MultipartFormDataContent();

        //        if (authorizationToken != null)
        //        {
        //            httpClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
        //        }

        //        if (IsAddIpHeaders)
        //        {
        //            httpClient.DefaultRequestHeaders.Add(CLIENT_HTTP_X_FORWARDED_FOR, CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"].ToString());
        //            httpClient.DefaultRequestHeaders.Add(CLIENT_REMOTE_ADDR, CurrentContext.Current.Request.Headers["REMOTE_ADDR"].ToString());
        //        }

        //        string fullPathToFile = fileName;

        //        using (FileStream fileStream = File.OpenRead(fullPathToFile))
        //        {
        //            var streamContent = new StreamContent(fileStream);
        //            var fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result);
        //            form.Add(fileContent, inputName, Path.GetFileName(fullPathToFile));

        //            if(datas != null)
        //            {
        //                foreach(string key in datas.Keys)
        //                {
        //                    var c = new StringContent(datas[key] ?? "");
        //                    form.Add(c, key);
        //                }
        //            }

        //            HttpResponseMessage response = await httpClient.PostAsync(url, form); //這裡沒有帶登入資訊

        //            if (response.IsSuccessStatusCode)
        //            {
        //                return response.Content.ReadAsStringAsync().Result;
        //            }
        //            else
        //            {
        //                var contentStr = response.Content.ReadAsStringAsync().Result;

        //                throw new HttpRequestException("上傳檔案發生錯誤, url: " + url, new Exception(contentStr), response.StatusCode);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="method"></param>
        ///// <param name="data"></param>
        ///// <param name="timeout"></param>
        ///// <param name="authorizationToken"></param>
        ///// <param name="IsAddIpHeaders"></param>
        ///// <param name="isSequencialInSession"></param>
        ///// <param name="headers"></param>
        ///// <param name="isShowNullObject">物件為null是否要列出</param>
        ///// <returns></returns>
        //public static HttpWebRequest GetRequest(string url, string method, object? data, int timeout = 30000,
        //    string? authorizationToken = null, bool IsAddIpHeaders = false,
        //    bool isSequencialInSession = false, Dictionary<string, string>? headers = null, bool isShowNullObject = true)
        //{
        //    byte[] postData = { };

        //    if (data != null)
        //    {
        //        var settings = new JsonSerializerSettings();
        //        if (!isShowNullObject)
        //        {
        //            settings.NullValueHandling = NullValueHandling.Ignore;
        //        }
        //        var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(data, settings);
        //        postData = Encoding.UTF8.GetBytes(dataJson);
        //    }

        //    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        //    HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
        //    request.Method = method;
        //    request.ContentType = "application/json";
        //    request.Timeout = timeout;
        //    request.ContentLength = postData.Length;

        //    if (authorizationToken != null)
        //    {
        //        request.Headers.Add("Authorization", authorizationToken);
        //    }

        //    if (isSequencialInSession)
        //    {
        //        request.Headers.Add("SequencialInSession", "true");
        //    }

        //    if (IsAddIpHeaders)
        //    {
        //        if (CurrentContext.Current != null
        //                && CurrentContext.Current.Request != null
        //                && CurrentContext.Current.Request.Headers != null)
        //        {
        //            request.Headers.Add(CLIENT_HTTP_X_FORWARDED_FOR, CurrentContext.Current.Request.Headers["HTTP_X_FORWARDED_FOR"]);

        //            request.Headers.Add(CLIENT_REMOTE_ADDR, CurrentContext.Current.Connection.RemoteIpAddress.ToString());
        //        }
        //    }

        //    if (headers != null)
        //    {
        //        foreach (string key in headers.Keys)
        //        {
        //            request.Headers.Add(key, headers[key]);
        //        }
        //    }

        //    if (method.ToUpper() != "GET")
        //    {
        //        // 寫入 Post Body Message 資料流
        //        using (Stream st = request.GetRequestStream())
        //        {
        //            st.Write(postData, 0, postData.Length);
        //        }
        //    }

        //    return request;
        //}

        ///// <summary>
        ///// 預設 timeout 30 秒
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="url"></param>
        ///// <param name="method"></param>
        ///// <param name="data"></param>
        ///// <param name="timeout">預設 30 秒(30000)</param>
        ///// <param name="authorizationToken"></param>
        ///// <param name="IsAddIpHeaders"></param>
        ///// <returns></returns>
        //public static async Task<T> SendJsonAsync<T>(string url, string method, object data, int timeout = 30000,
        //    string authorizationToken = null, bool IsAddIpHeaders = true, bool isSequencialInSession = true, bool isReturnString = false)
        //{
        //    HttpWebRequest request = GetRequest(url, method, data, timeout, authorizationToken, IsAddIpHeaders, isSequencialInSession);

        //    string result = "";
        //    try
        //    {
        //        //如果出現: "無法對不會寫入資料的作業設定 Content-Length 或區塊編碼。" 請檢查 Method 是否正確。

        //        // 取得回應資料
        //        using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
        //        {
        //            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
        //            {
        //                result = await sr.ReadToEndAsync();
        //            }
        //        }

        //        if(isReturnString)
        //        {
        //            return result.Json().JsonDeserialize<T>();
        //        }

        //        return result.JsonDeserialize<T>();
        //    }
        //    catch (System.Net.WebException ex)
        //    {
        //        throw ex;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"url: {url}, json: {data.Json()}, authorizationToken: {authorizationToken},  ex: {ex.FullInfo()}, result: {result}");
        //    }
        //}


        ///// <summary>
        ///// 預設 timeout 30 秒
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="url"></param>
        ///// <param name="method"></param>
        ///// <param name="data"></param>
        ///// <param name="timeout">預設 30 秒(30000)</param>
        ///// <param name="authorizationToken"></param>
        ///// <param name="IsAddIpHeaders"></param>
        ///// <returns></returns>
        //public static T SendJson<T>(string url, string method, object? data, int timeout = 30000,
        //    string? authorizationToken = null, bool IsAddIpHeaders = false,
        //    bool isSequencialInSession = false, Dictionary<string, string>? headers = null, bool isShowNullObject = true)
        //{
        //    HttpWebRequest request = GetRequest(url, method, data, timeout, authorizationToken, IsAddIpHeaders, isSequencialInSession, headers, isShowNullObject);

        //    string result = "";
        //    try
        //    {
        //        //如果出現: "無法對不會寫入資料的作業設定 Content-Length 或區塊編碼。" 請檢查 Method 是否正確。

        //        // 取得回應資料
        //        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        //        {
        //            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
        //            {
        //                result = sr.ReadToEnd();
        //            }
        //        }

        //        if (typeof(T) == typeof(string) && !result.StartsWith("\""))
        //        {
        //            return result.Json().JsonDeserialize<T>();
        //        }

        //        return result.JsonDeserialize<T>();
        //    }
        //    catch (System.Net.WebException ex)
        //    {
        //        if (ex.Response != null)
        //        {
        //            var stream = ex.Response.GetResponseStream();
        //            var responseText = new StreamReader(stream).ReadToEnd();
        //            try
        //            {
        //                //stream.Position = 0;
        //                stream.Seek(0, SeekOrigin.Begin);
        //            }
        //            catch (Exception)
        //            {
        //            }

        //            throw new Exception($"url: {url}\r\njson: {data?.Json(isIndented: true)}\r\nauthorizationToken: {authorizationToken}\r\nresult:{result}\r\nresponseText: {responseText}\r\nex: {ex.FullInfo()}");
        //        }

        //        throw new Exception($"url: {url}\r\njson: {data?.Json(isIndented: true)}\r\nauthorizationToken: {authorizationToken}\r\nresult:{result}\r\nex: {ex.FullInfo()}");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"url: {url}\r\njson: {data?.Json(isIndented: true)}\r\nauthorizationToken: {authorizationToken}\r\nresult:{result}\r\nex: {ex.FullInfo()}");
        //    }
        //}


        private static string? _LogRoot = null;
        public static string? LogRoot
        {
            get
            {
                return _LogRoot;
            }
        }

        /// <summary>
        /// 會判斷必需用 \ 結尾, 這個在 Linux 的版本可能有問題, 要再修正
        /// </summary>
        public static void SetLogRoot(string LogRoot)
        {
            if (!LogRoot.EndsWith("\\") && !LogRoot.EndsWith("/"))
            {
                throw new Exception("LogRoot 請使用 \\ 或 / 結尾");
            }
            else
            {
                _LogRoot = LogRoot;
            }

            if (LogRoot.EndsWith("\\"))
            {
                Su.FileLogger.OneDayLogDirectory = _LogRoot + "OneDayLog\\";
            }
            else
            {
                Su.FileLogger.OneDayLogDirectory = _LogRoot + "OneDayLog/";
            }
        }

        ///// <summary>
        ///// 會補上時間, 檔案會放在 _LogRoot + SubPath + FileName + ".log";
        ///// 發生 exception 會直接略過.
        ///// </summary>
        ///// <param name="msg"></param>
        ///// <param name="FileName">結尾會自動加上 ".log" </param>
        ///// <param name="SubPath">結尾會自動加上 "\", Linux 的系統可能會有問題.</param>
        ///// <param name="isThrowException"></param>
        //public static void AddLog(string msg, string FileName, string SubPath = "", bool isThrowException = false)
        //{
        //    if (_LogRoot == null)
        //    {
        //        throw new Exception("請先指定 Log Root.");
        //    }

        //    if (SubPath != "" && !SubPath.EndsWith(@"\") && !SubPath.EndsWith(@"/"))
        //    {
        //        if (_LogRoot.StartsWith("/"))
        //        {
        //            SubPath += "/";
        //        }
        //        else
        //        {
        //            SubPath += @"\";
        //        }
        //    }

        //    System.IO.Directory.CreateDirectory(_LogRoot + SubPath);
        //    string Filename = _LogRoot + SubPath + FileName + ".log";

        //    try
        //    {
        //        System.IO.File.AppendAllText(Filename, "\r\n\r\n" + DateTime.Now.ToString() + "\r\n" + msg);
        //    }
        //    catch (Exception)
        //    {
        //        if (isThrowException)
        //        {
        //            throw;
        //        }
        //    }
        //}
        public static void AppendLog(string FileName, string Info, bool IsAddDateTime = true, bool IsCRLF = true, bool IsThrowException = false)
        {
            try
            {
                StreamWriter w = File.AppendText(FileName);

                if (IsAddDateTime)
                {
                    if (IsCRLF)
                    {
                        w.WriteLine("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "\r\n" + Info);
                    }
                    else
                    {
                        w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "\r\n" + Info);
                    }
                }
                else if (IsCRLF)
                {
                    w.WriteLine("\r\n" + Info);
                }
                else
                {
                    w.WriteLine(Info);
                }

                w.Flush();
                w.Close();
            }
            catch (Exception)
            {
                if (IsThrowException)
                {
                    throw;
                }
            }
        }
    }
}
