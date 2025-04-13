
using Core.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using NPOI.SS.Formula.Functions;
namespace AdminApi
{
    /// <summary>
    /// 必需在 program.cs 才有全面性，而且要在 SetAuthorizationFilter 之後執行，所以這個 filter 定義為 action filter 來執行。
    /// (在 program.cs 註冊的 AuthorizationFilter 會先於綁定在 Controller 和 Action 的 AuthorizationFilter 執行)
    /// </summary>
    public class CheckAuthorizationFilter : IActionFilter, IOrderedFilter
    {
        private readonly AuthHelper _authHelper;

        public CheckAuthorizationFilter(AuthHelper authHelper)
        {
            _authHelper = authHelper;
        }

        /// <summary>
        /// 預期要在所有 ActionFilter 之前執行(檔案類型檢查可以放在前面)
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_authHelper.IsLogin)
            {
                _authHelper.CheckPageAuthorization(context, Core.Constants.AdminPermission.Admin,
                    Core.Constants.AdminPermission.UnLimited,
                    _authHelper.AdminPermissions);
            }
            else
            {
                _authHelper.CheckPageAuthorization(context,
                    Core.Constants.AdminPermission.Admin,
                    Core.Constants.AdminPermission.UnLimited,
                    []);
            }
        }
    }
}