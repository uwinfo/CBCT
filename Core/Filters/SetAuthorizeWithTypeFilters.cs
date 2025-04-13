using Core.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Core.Filters
{
    /// <summary>
    /// 可自定型別的 SetAuthorization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AddPermissionWithTypeFilter<T> : Attribute, IAuthorizationFilter
    {
        private readonly AuthHelper _authHelper;
        private readonly T[] _permissions;

        /// <summary>
        /// 註冊多個權限
        /// </summary>
        /// <param name="authHelper"></param>
        /// <param name="authCodes"></param>
        public AddPermissionWithTypeFilter(AuthHelper authHelper, T[] authCodes)
        {
            //註冊可執行權限, construtor 不能呼叫 Sc.RegisterPageAuthCodes(AuthCodes)，所以先記錄下來，在 OnAuthorization 事件中執行 Sc.RegisterPageAuthCodes
            _authHelper = authHelper;
            _permissions = authCodes;
        }

        /// <summary>
        /// 這個要在 CheckAuthorizationFilter 之前執行
        /// 註冊單一權限時使用
        /// </summary>
        /// <param name="authHelper"></param>
        /// <param name="permission"></param>
        public AddPermissionWithTypeFilter(AuthHelper authHelper, T permission)
        {
            //註冊可執行權限, construtor 不能呼叫 Sc.RegisterPageAuthCodes(AuthCodes);
            _authHelper = authHelper;
            _permissions = new T[] { permission };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //註冊可執行權限，
            //為了讓 SetAuthorizationFilter 可以綁定在 Controller 和 Action 身上，所以這裡只註冊不執行實際檢查。(可能會有多個 SetAuthorizationFilter)
            _authHelper.AddPagePermissions(_permissions);
        }
    }
}