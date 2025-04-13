using Core.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Su;
using System.Net;
using System.Web;

namespace Core.Middlewares
{
    public class ErrorHandleMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, env.EnvironmentName);
            }
        }

        /// <summary>
        /// Custom Exception 不會記錄錯誤
        /// 其它 Exception 一率記錄在 Exception 之中，並保留 30 天。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <param name="envName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Task HandleExceptionAsync(HttpContext context, Exception ex, string envName)
        {
            var code = HttpStatusCode.InternalServerError;

            string internalErrorMessage;
            if (ex.InnerException != null && ex.InnerException.Message.ToLower().Contains("duplicate entry"))
            {
                internalErrorMessage = $"嘗試建立重複的資料: {ex.InnerException.Message.Replace("Duplicate entry", "").Replace("for key", ", 資料庫索引名稱")}";
            }
            else
            {
                if (envName == "Production")
                {
                    internalErrorMessage = "發生錯誤，請連絡管員";
                }
                else
                {
                    internalErrorMessage = HttpUtility.JavaScriptStringEncode(ex.FullInfo());
                    if (ex.InnerException != null)
                    {
                        internalErrorMessage += "; InnerException: " + ex.InnerException.ToString();
                    }
                }
            }

            Dictionary<string, List<string>> invalidatedPayload = [];
            string? debugInfo = null;
            string? errorCode = null;
            bool isAddStackTrace = true;
            string? message = null;

            if (ex.GetType() == typeof(CustomException))
            {
                CustomException customEx = (CustomException)ex;

                isAddStackTrace = customEx.isAddStackTrace;

                if(customEx.Code != 0)
                {
                    code = customEx.Code;
                }

                if(customEx.InvalidatedPayload != null) {
                    invalidatedPayload = customEx.InvalidatedPayload;
                }

                message = customEx.Message;

                //if (customEx.ErrorCode == ERROR_CODE.MESSAGE)
                //{
                //    message = customEx.Message;
                //}
                //else 
                //{
                //    message = Core.Helpers.ErrorHelper.GetErrorMessageByCode(customEx.ErrorCode);
                //}

                if (customEx.Option != null)
                {
                    foreach (string key in customEx.Option.Keys)
                    {
                        string value = string.Empty;
                        customEx.Option.TryGetValue(key, out value!);
                        internalErrorMessage = internalErrorMessage.Replace("{" + key + "}", value);
                    }
                }

                //errorCode = customEx.ErrorCode.ToString();
                debugInfo = customEx.debugInfo;
            }
            else
            {
                message = "發生錯誤，請連絡管理員";
                LogHelper.AddExceptionLog("ErrorHandleMiddleware", ex, isThrowExctption: true);
            }

            var result = new
            {
                message,
                stackTrace = isAddStackTrace ? ex.StackTrace : null,
                invalidatedPayload,
                errorCode,
                debugInfo,
                internalErrorMessage,
                logDir = Su.FileLogger.OneDayLogDirectory
            };

            try
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                return context.Response.WriteAsync(JsonConvert.SerializeObject(result));
            }
            catch (Exception)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
