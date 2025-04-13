using Microsoft.EntityFrameworkCore;
using Core.Models;
using System.Net;
using Su;

namespace Core.Helpers
{
    public class AuthHelper
    {
        private readonly HttpContextWrapper _httpContext;
        private readonly HttpContextHelper _httpContextHelper;
        public AuthHelper(HttpContextWrapper httpContext, HttpContextHelper httpContextHelper)
        {
            _httpContext = httpContext;
            _httpContextHelper = httpContextHelper;
        }
       
        private const string PagePermissionKey = "PagePermissionKey";
        public void AddPagePermissions<T>(T[] permissions)
        {
            if (permissions == null || _httpContext.Items == null)
                return;

            if (!_httpContext.Items.ContainsKey(PagePermissionKey))
            {
                _httpContext.Items[PagePermissionKey] = new List<T>();
            }

            ((List<T>)_httpContext.Items[PagePermissionKey]).AddRange(permissions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="userPassAllAuthCode"></param>
        /// <param name="pageByPassAuthCode">不用檢查的權限</param>
        /// <param name="userAuthorizedCodes"></param>
        /// <exception cref="CustomException"></exception>
        public void CheckPageAuthorization<T>(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context,
            T userPassAllAuthCode,
            T pageByPassAuthCode,
            IEnumerable<T> userAuthorizedCodes)
        {
            if (!IsPageAuthorized(userPassAllAuthCode, pageByPassAuthCode, userAuthorizedCodes))
            {
                string? controllerName = context.RouteData.Values["controller"]?.ToString();
                string? actionName = context.RouteData.Values["action"]?.ToString();
                if (!IsLogin)
                {
                    throw new CustomException("請先登入", 
                        HttpStatusCode.Unauthorized,
                        debugInfo: (new { controllerName, actionName }).Json());
                }

                if (_httpContextHelper.IsLocalhost)
                {
                    var pagePermissions = (List<T>)_httpContext.Items[PagePermissionKey];
                    if (pagePermissions != null)
                    {
                        throw new CustomException($"權限不足; pagePermissions: {pagePermissions.Select(x => x.ToString()).ToOneString(",")}",
                            HttpStatusCode.Unauthorized,
                            debugInfo: (new { controllerName, actionName }).Json());
                    }
                }

                throw new CustomException($"權限不足",
                    HttpStatusCode.Unauthorized, 
                    debugInfo: (new { controllerName, actionName }).Json());
            }
        }

        /// <summary>
        /// 使用自己擁有的 Permission 去比對 RegisterPage 的 Permission;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adminPermission">可通過作何權限檢查的 Permission，後台通常是 Admin，前台是登入即可</param>
        /// <param name="unlimitedPermission">不用檢查權限的 Permission，通常是 UnLimited 或 不檢查</param>
        /// <param name="userPermissions">使用者被授權的程式碼，因為前後台的 Is Login 判斷方式不同，所以直接傳入已授權的清單</param>
        /// <returns></returns>
        public bool IsPageAuthorized<T>(T adminPermission, T unlimitedPermission, IEnumerable<T> userPermissions)
        {
            //每一頁只會執行一次
            string isPageAuthorizedKey = "IsPageAuthorized";
            if (_httpContext.Items[isPageAuthorizedKey] != null)
            {
                return (bool)_httpContext.Items[isPageAuthorizedKey];
            }

            List<T> pagePermissions;
            if (_httpContext.Items[PagePermissionKey] == null)
            {
                //未設定時, 預設登入後才可以執行
                pagePermissions = new List<T> { adminPermission };
            }
            else
            {
                pagePermissions = (List<T>)_httpContext.Items[PagePermissionKey];
            }

            if (pagePermissions.Contains(unlimitedPermission))
            {
                _httpContext.Items[isPageAuthorizedKey] = true;
                return true;
            }

            if (userPermissions.Contains(adminPermission))
            {
                _httpContext.Items[isPageAuthorizedKey] = true;
                return true;
            }

            //Su.Debug.AppendLog("userPermissions: " + userPermissions.Select(x => x.ToString()).ToOneString(","));
            if (userPermissions.Any(a => pagePermissions.Contains(a)))
            {
                _httpContext.Items[isPageAuthorizedKey] = true;
                return true;
            }

            _httpContext.Items[isPageAuthorizedKey] = false;
            return false;
        }

        /// <summary>
        /// 判斷是否已登入
        /// </summary>
        /// <returns></returns>
        public bool IsLogin
        {
            get
            {
                return LoginInfo != null;
            }
        }

        /// <summary>
        /// 登入者的 Uid
        /// </summary>
        public string? LoginUid
        {
            get
            {
                return LoginInfo?.LoginUid;
            }
        }

        /// <summary>
        /// 由 Cookie 取得登入者的權限(可以放到 4K, 希望不會爆掉)
        /// </summary>
        public List<Core.Constants.AdminPermission> AdminPermissions
        {
            get
            {
                //Su.Debug.AppendLog("===AdminPermissions Start===");
                const string itemKey = "_LoginAdminUserPermissions_";
                if (_httpContext.Items.ContainsKey(itemKey))
                {
                    return (List<Core.Constants.AdminPermission>)_httpContext.Items[itemKey];
                }

                if (!IsLogin)
                {
                    _httpContext.Items[itemKey] = new List<Core.Constants.AdminPermission>();
                    return (List<Core.Constants.AdminPermission>)_httpContext.Items[itemKey];
                }

                var permissionStr = LoginInfo!.PermissionCodes;
                //Su.Debug.AppendLog("permissionStr == null: "+ string.IsNullOrWhiteSpace(permissionStr));
                if (string.IsNullOrWhiteSpace(permissionStr))
                {
                    _httpContext.Items[itemKey] = new List<Core.Constants.AdminPermission>()
                    {
                        Core.Constants.AdminPermission.Login
                    };
                    //Su.Debug.AppendLog("AAAAAA");
                    return (List<Core.Constants.AdminPermission>)_httpContext.Items[itemKey];
                }

                var permissions = new List<Core.Constants.AdminPermission>();
                foreach (var item in permissionStr.Split('^'))
                {
                    if (Enum.TryParse(item, out Core.Constants.AdminPermission permission))
                    {
                        permissions.Add(permission);
                    }
                }
                //Su.Debug.AppendLog("permissions.Count: " + permissions.Count);
                if (!permissions.Any(x => x.ToString() == Core.Constants.AdminPermission.Login.ToString()))
                {
                    permissions.Add(Core.Constants.AdminPermission.Login);
                }

                //本機測試時，自動加入 Admin 權限
                //if (Su.Wu.IsLocalhost)
                //{
                //    permissions.Add(Constants.AdminPermission.Admin);
                //}

                _httpContext.Items[itemKey] = permissions;
                //Su.Debug.AppendLog("...AdminPermissions End...");
                return (List<Core.Constants.AdminPermission>)_httpContext.Items[itemKey];
            }
        }

        /// <summary>
        /// 取得登入者的資訊。(由 Cookie 中取得)
        /// </summary>
        /// <returns></returns>
        public LoginInfo? LoginInfo
        {
            get
            {
                var key = "GetLoginUserInfo";
                Su.Debug.AppendLog("===== LoginInfo Start =====");
                if (_httpContext.Items.ContainsKey(key))
                {
                    Su.Debug.AppendLog("A.return Current.Items.ContainsKey: " + key);
                    return (LoginInfo?)_httpContext.Items[key];
                }

                var authCookie = _httpContext.GetCookie(Su.Wu.AuthCookieName);
                Su.Debug.AppendLog("B.authCookie, AuthCookieName: " + Su.Wu.AuthCookieName);
                if (string.IsNullOrEmpty(authCookie))
                {
                    Su.Debug.AppendLog("C.return IsNullOrEmpty: " + authCookie);
                    _httpContext.Items[key] = null;
                    return null;
                }

                try
                {
                    var loginData = Su.Encryption.AesDecryptCookie(authCookie);
                    Su.Debug.AppendLog("D.loginData: " + loginData);
                    var arr = loginData.Split(',');

                    Su.Debug.AppendLog("E.arr.Count: " + arr.Count());
                    var res = new LoginInfo
                    {
                        LoginAt = arr[0].ToDate(),
                        LoginUid = arr[1],
                        PermissionCodes = arr[2]
                    };

                    if (res.LoginAt < DateTime.Now.AddDays(-3)) //三天內未使用，就視為已登出
                    {
                        Su.Debug.AppendLog("F.三天內未使用，就視為已登出, " + res.LoginAt);
                        _httpContext.Items[key] = null;
                        return null;
                    }

                    if (res.LoginAt < DateTime.Now.AddDays(-1))
                    {
                        //一天內未登出，重新再發 Cookie
                        Su.Debug.AppendLog("G.一天內未登出，重新再發 Cookie, " + res.LoginAt);
                        AddLoginCookie(res.LoginUid, res.PermissionCodes);
                    }

                    _httpContext.Items[key] = res;
                    Su.Debug.AppendLog("...LoginInfo End...res: " + res);
                    return res;
                }
                catch (Exception ex)
                {
                    _httpContext.Items[key] = null;
                    Su.Debug.AppendLog("...LoginInfo End..." + ex.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// 後台登入用
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public void AdminLogIn(string userUid, Core.Ef.CBCTContext ct)
        {
            var roleUids = ct.AdminRoleAdminUsers.Where(x => x.DeletedAt == null && x.AdminUserUid == userUid).Select(x => x.AdminRoleUid);
            var permissionUids = ct.AdminRoleAdminPermissions.Where(x => x.DeletedAt == null && roleUids.Contains(x.AdminRoleUid))
                .Select(x => x.PermissionUid);
            var permissionCodes = ct.AdminPermissions.Where(p => p.DeletedAt == null && permissionUids.Contains(p.Uid))
                .Select(x => x.Code)
                .ToList()
                .ToOneString("^");

            //不要放 Json, 字串會太長,
            AddLoginCookie(userUid, permissionCodes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginUid"></param>
        /// <param name="permissionCodes">用 ^ 分隔</param>
        public void AddLoginCookie(string loginUid, string permissionCodes)
        {
            _httpContext.SetCookie(Su.Wu.AuthCookieName, Su.Encryption.AesEncryptCookie($"{DateTime.Now.ISO8601()},{loginUid},{permissionCodes}"));
        }

        /// <summary>
        /// 登出(清除 Cookie)
        /// </summary>
        public void LogOut()
        {
            _httpContext.RemoveCookie(Su.Wu.AuthCookieName);
        }

        /// <summary>
        /// 登入的管理者資料
        /// </summary>
        public Dtos.AdminUserDto? LoginAdmin
        {
            get
            {
                if (!IsLogin) { return null; }

                string key = "LoginAdminUsers";
                if (!_httpContext.Items.ContainsKey(key))
                {
                    _httpContext.Items[key] = AdminUserHelper.GetOne(Core.Ef.CBCTContext.NewDbContext, LoginUid)
                        ?? throw new Exception("使用者不存在: " + LoginUid);
                }

                return (Dtos.AdminUserDto)_httpContext.Items[key];
            }
        }

        /// <summary>
        /// 登入的醫師資料
        /// </summary>
        public Core.Ef.Doctor? LoginDoctor
        {
            get
            {
                if (!IsLogin) { return null; }

                string key = "LoginDoctors";
                if (!_httpContext.Items.ContainsKey(key))
                {
                    _httpContext.Items[key] = DoctorHelper.GetOne(Core.Ef.CBCTContext.NewDbContext, LoginUid)
                        ?? throw new Exception("使用者不存在: " + LoginUid);
                }

                return (Core.Ef.Doctor?)_httpContext.Items[key];
            }
        }
    }
}
