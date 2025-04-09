using Microsoft.AspNetCore.Mvc.Filters;
using Su;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Core.Filters
{
    /// <summary>
    /// 可自定型別的 SetAuthorization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AddPermissionWithTypeFilter<T> : Attribute, IAuthorizationFilter
    {
        T[] Permissions;

        /// <summary>
        /// 註冊多個權限
        /// </summary>
        /// <param name="authCodes"></param>
        public AddPermissionWithTypeFilter(T[] authCodes)
        {
            //註冊可執行權限, construtor 不能呼叫 Sc.RegisterPageAuthCodes(AuthCodes)，所以先記錄下來，在 OnAuthorization 事件中執行 Sc.RegisterPageAuthCodes
            Permissions = authCodes;
        }

        /// <summary>
        /// 這個要在 CheckAuthorizationFilter 之前執行
        /// 註冊單一權限時使用
        /// </summary>
        /// <param name="permission"></param>
        public AddPermissionWithTypeFilter(T permission)
        {
            //註冊可執行權限, construtor 不能呼叫 Sc.RegisterPageAuthCodes(AuthCodes);
            Permissions = new T[] { permission };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //註冊可執行權限，
            //為了讓 SetAuthorizationFilter 可以綁定在 Controller 和 Action 身上，所以這裡只註冊不執行實際檢查。(可能會有多個 SetAuthorizationFilter)
            Core.Helpers.AuthHelper.AddPagePermissions(Permissions);
        }
    }
}