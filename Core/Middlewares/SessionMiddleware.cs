using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Core.Helpers;

namespace Core.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SystemHelper _systemHelper;
        public SessionMiddleware(RequestDelegate next, SystemHelper systemHelper)
        {
            _next = next;
            _systemHelper = systemHelper;
        }

        private readonly Dictionary<string, string> RunningSession = [];

        public async Task InvokeAsync(HttpContext context)
        {
            var routeData = context.GetRouteData();
            var controllerName = routeData.Values["controller"]?.ToString();
            var actionName = routeData.Values["action"]?.ToString();

            var isSequence = Core.Helpers.SessionHelper.IsSequence(controllerName, actionName);
            var currentProcessGuid = Guid.NewGuid().ToString();

            try
            {
                //處理必需排隊的 Request, 例如寫入訂單
                if (isSequence && ! string.IsNullOrEmpty(_systemHelper.ComputerUid))
                {
                    lock (Su.LockerProvider.GetLocker("Session_" + _systemHelper.ComputerUid))
                    {
                        //暫時不處理多主機的問題, 未來多主機要用 DB 管控
                        var waitCount = 0;
                        var waitAt = DateTime.Now;
                        while (RunningSession.ContainsKey(_systemHelper.ComputerUid) && waitCount < 2000) //最多等 20 秒
                        {
                            System.Threading.Thread.Sleep(10);
                            waitCount++;
                        }

                        if (RunningSession.ContainsKey(_systemHelper.ComputerUid))
                        {
                            Su.FileLogger.AddDailyLog("session", $"SessionMiddleware, Wait for {(DateTime.Now - waitAt).TotalMilliseconds}");
                        }
                        else
                        {
                            RunningSession.Add(_systemHelper.ComputerUid, currentProcessGuid);
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (isSequence && !string.IsNullOrEmpty(_systemHelper.ComputerUid) && RunningSession.ContainsKey(_systemHelper.ComputerUid))
                {
                    RunningSession.Remove(_systemHelper.ComputerUid);
                }
            }
        }
    }

    public static class SessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }
}
