using Core.Constants;
using Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdminApi
{
    //Attribute：只保存參數
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AddPermissionAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// 設定多個權限
        /// </summary>
        /// <param name="permissions"></param>
        public AddPermissionAttribute(params AdminPermission[] permissions)
            : base(typeof(AddPermissionWithTypeFilter<AdminPermission>))
        {
            Arguments = new object[] { permissions };
        }

        /// <summary>
        /// 設定單一權限
        /// </summary>
        /// <param name="permission"></param>
        public AddPermissionAttribute(AdminPermission permission)
            : base(typeof(AddPermissionWithTypeFilter<AdminPermission>))
        {
            Arguments = new object[] { new[] { permission } };
        }
    }

    //Filter：做實際注入與處理
    public class AddPermissionWithTypeFilter<T> : IAuthorizationFilter
    {
        private readonly AuthHelper _authHelper;
        private readonly T[] _permissions;

        public AddPermissionWithTypeFilter(AuthHelper authHelper, T[] permissions)
        {
            _authHelper = authHelper;
            _permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _authHelper.AddPagePermissions(_permissions);
        }
    }
}