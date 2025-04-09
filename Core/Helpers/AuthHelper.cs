//using Core.Dtos;
using Microsoft.EntityFrameworkCore;
using Core.Models;
using System.Net;
using Su;

namespace Core.Helpers
{
    public class AuthHelper
    {
        const string PagePermissionKey = "PagePermissionKey";

        /// <summary>
        /// 前後台的權限類別不同
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        public static void AddPagePermissions<T>(T[] permissions)
        {
            // 有可能是 swagger 進來的
            if (permissions == null || CurrentContext.Current == null)
            {
                return;
            }

            if (!CurrentContext.Current.Items.ContainsKey(PagePermissionKey))
            {
                CurrentContext.Current.Items[PagePermissionKey] = new List<T>();
            }

            ((List<T>)CurrentContext.Current.Items[PagePermissionKey]).AddRange(permissions);
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
        public static void CheckPageAuthorization<T>(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context,
            T userPassAllAuthCode,
            T pageByPassAuthCode,
            IEnumerable<T> userAuthorizedCodes)
        {
            if (!Core.Helpers.AuthHelper.IsPageAuthorized(userPassAllAuthCode, pageByPassAuthCode, userAuthorizedCodes))
            {
                string? controllerName = context.RouteData.Values["controller"]?.ToString();
                string? actionName = context.RouteData.Values["action"]?.ToString();
                if (!Core.Helpers.AuthHelper.IsLogin)
                {
                    throw new CustomException("請先登入", 
                        HttpStatusCode.Unauthorized,
                        debugInfo: (new { controllerName, actionName }).Json());
                }

                if (Su.Wu.IsLocalhost)
                {
                    var pagePermissions = (List<T>)CurrentContext.Current.Items[PagePermissionKey];
                    throw new CustomException($"權限不足; pagePermissions: {pagePermissions.Select(x => x.ToString()).ToOneString(",")}", 
                        HttpStatusCode.Unauthorized, 
                        debugInfo: (new { controllerName, actionName }).Json());
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
        public static bool IsPageAuthorized<T>(T adminPermission, T unlimitedPermission, IEnumerable<T> userPermissions)
        {
            //每一頁只會執行一次
            string isPageAuthorizedKey = "IsPageAuthorized";
            if (CurrentContext.Current.Items[isPageAuthorizedKey] != null)
            {
                return (bool)CurrentContext.Current.Items[isPageAuthorizedKey];
            }

            List<T> pagePermissions;
            if (CurrentContext.Current.Items[PagePermissionKey] == null)
            {
                //未設定時, 預設登入後才可以執行
                pagePermissions = new List<T> { adminPermission };
            }
            else
            {
                pagePermissions = (List<T>)CurrentContext.Current.Items[PagePermissionKey];
            }

            if (pagePermissions.Contains(unlimitedPermission))
            {
                CurrentContext.Current.Items[isPageAuthorizedKey] = true;
                return true;
            }

            if (userPermissions.Contains(adminPermission))
            {
                CurrentContext.Current.Items[isPageAuthorizedKey] = true;
                return true;
            }

            //Su.Debug.AppendLog("userPermissions: " + userPermissions.Select(x => x.ToString()).ToOneString(","));
            if (userPermissions.Any(a => pagePermissions.Contains(a)))
            {
                CurrentContext.Current.Items[isPageAuthorizedKey] = true;
                return true;
            }

            CurrentContext.Current.Items[isPageAuthorizedKey] = false;
            return false;
        }

        /// <summary>
        /// 記錄登入 Cookie 用
        /// </summary>
        static string? _authCookieName = null;

        static string AuthCookieName
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

        /// <summary>
        /// 記錄登入 Cookie 用, 只允許 "Admin" 或 "Www"
        /// </summary>
        static string? _Site = null;

        public static string Site
        {
            set
            {
                if (_Site != null)
                {
                    throw new Exception("Site 只能設定一次");
                }

                if (value != Core.Constants.Site.ADMIN && value != Core.Constants.Site.WWW)
                {
                    throw new Exception("Site 的值錯誤");
                }
                _Site = value;
            }
            get
            {
                if (string.IsNullOrEmpty(_Site))
                {
                    throw new Exception("未設定 Site");
                }
                return _Site;
            }
        }

        /// <summary>
        /// 判斷是否已登入
        /// </summary>
        /// <returns></returns>
        public static bool IsLogin
        {
            get
            {
                switch(Site)
                {
                    case Core.Constants.Site.ADMIN:
                        return AdminLoginInfo != null;
                    //case Core.Constants.Site.WWW:
                    //    return GeneralLoginInfo != null;
                }

                return false;
            }
        }

        /// <summary>
        /// 後台登入者的 Uid
        /// </summary>
        public static string? AdminUserUid
        {
            get
            {
                return AdminLoginInfo?.AdminUserUid;
            }
        }

        ///// <summary>
        ///// 前台登入者的 Uid
        ///// </summary>
        //public static string? MemberUid
        //{
        //    get
        //    {
        //        return GeneralLoginInfo?.LoginUid;
        //    }
        //}

        /// <summary>
        /// 由 Cookie 取得登入者的權限(可以放到 4K, 希望不會爆掉)
        /// </summary>
        public static List<Core.Constants.AdminPermission> AdminPermissions
        {
            get
            {
                //Su.Debug.AppendLog("===AdminPermissions Start===");
                const string itemKey = "_LoginAdminUserPermissions_";
                if (Su.CurrentContext.Items.ContainsKey(itemKey))
                {
                    return (List<Core.Constants.AdminPermission>)Su.CurrentContext.Items[itemKey];
                }

                if (!IsLogin)
                {
                    Su.CurrentContext.Items[itemKey] = new List<Core.Constants.AdminPermission>();
                    return (List<Core.Constants.AdminPermission>)Su.CurrentContext.Items[itemKey];
                }

                var permissionStr = AdminLoginInfo!.AdminPermissionCodes;
                //Su.Debug.AppendLog("permissionStr == null: "+ string.IsNullOrWhiteSpace(permissionStr));
                if (string.IsNullOrWhiteSpace(permissionStr))
                {
                    Su.CurrentContext.Items[itemKey] = new List<Core.Constants.AdminPermission>()
                    {
                        Core.Constants.AdminPermission.Login
                    };
                    //Su.Debug.AppendLog("AAAAAA");
                    return (List<Core.Constants.AdminPermission>)Su.CurrentContext.Items[itemKey];
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

                Su.CurrentContext.Items[itemKey] = permissions;
                //Su.Debug.AppendLog("...AdminPermissions End...");
                return (List<Core.Constants.AdminPermission>)Su.CurrentContext.Items[itemKey];
            }
        }

        /// <summary>
        /// 取得 Admin 登入者的資訊。(由 Cookie 中取得)
        /// </summary>
        /// <returns></returns>
        public static AdminLoginInfo? AdminLoginInfo
        {
            get
            {
                var key = "GetLoginUserInfo";
                if (Su.CurrentContext.Current.Items.ContainsKey(key))
                {
                    return (AdminLoginInfo?)Su.CurrentContext.Current.Items[key];
                }

                var authCookie = Su.CurrentContext.Current.Request.Cookies[AuthCookieName];
                if (string.IsNullOrEmpty(authCookie))
                {
                    Su.CurrentContext.Current.Items[key] = null;
                    return null;
                }

                try
                {
                    var loginData = Su.Encryption.AesDecryptCookie(authCookie);
                    var arr = loginData.Split(',');

                    var res = new AdminLoginInfo
                    {
                        LoginAt = arr[0].ToDate(),
                        AdminUserUid = arr[1],
                        AdminPermissionCodes = arr[2]
                    };

                    if (res.LoginAt < DateTime.Now.AddDays(-1)) //三天內未使用，就視為已登出
                    {
                        Su.CurrentContext.Current.Items[key] = null;
                        return null;
                    }

                    if (res.LoginAt < DateTime.Now.AddDays(-1))
                    {
                        //一天內未登出，重新再發 Cookie
                        Helpers.AuthHelper.AddAdminLoginCookie(res.AdminUserUid, res.AdminPermissionCodes);
                    }

                    Su.CurrentContext.Current.Items[key] = res;
                    return res;
                }
                catch (Exception)
                {
                    Su.CurrentContext.Current.Items[key] = null;
                    return null;
                }
            }
        }

        ///// <summary>
        ///// 取得官網, 團購網登入者的資訊。(由 Cookie 中取得)
        ///// </summary>
        ///// <returns></returns>
        //public static GeneralLoginInfo? GeneralLoginInfo
        //{
        //    get
        //    {
        //        var key = "GetLoginUserInfo";
        //        if (Su.CurrentContext.Current.Items.ContainsKey(key))
        //        {
        //            return (GeneralLoginInfo?)Su.CurrentContext.Current.Items[key];
        //        }

        //        var authCookie = Su.CurrentContext.Current.Request.Cookies[AuthCookieName];
        //        if (string.IsNullOrEmpty(authCookie))
        //        {
        //            Su.CurrentContext.Current.Items[key] = null;
        //            return null;
        //        }

        //        try
        //        {
        //            var loginData = Su.Encryption.AesDecryptCookie(authCookie);
        //            var arr = loginData.Split(',');

        //            var res = new GeneralLoginInfo(arr[0].ToDate(), arr[1]);

        //            if (res.LoginAt < DateTime.Now.AddDays(-1)) //三天內未使用，就視為已登出
        //            {
        //                Su.CurrentContext.Current.Items[key] = null;
        //                return null;
        //            }

        //            if (res.LoginAt < DateTime.Now.AddDays(-1))
        //            {
        //                //一天內未登出，重新再發 Cookie
        //                Helpers.AuthHelper.AddLoginCookie(res.LoginUid);
        //            }

        //            Su.CurrentContext.Current.Items[key] = res;
        //            return res;
        //        }
        //        catch (Exception)
        //        {
        //            Su.CurrentContext.Current.Items[key] = null;
        //            return null;
        //        }
        //    }
        //}

        /// <summary>
        /// 後台登入用
        /// </summary>
        /// <param name="userUid"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static void AdminLogIn(string userUid, Core.Ef.CBCTContext ct)
        {
            var roleUids = ct.AdminRoleAdminUsers.Where(x => x.DeletedAt == null && x.AdminUserUid == userUid).Select(x => x.AdminRoleUid);
            var permissionUids = ct.AdminRoleAdminPermissions.Where(x => x.DeletedAt == null && roleUids.Contains(x.AdminRoleUid))
                .Select(x => x.PermissionUid);
            var permissionCodes = ct.AdminPermissions.Where(p => p.DeletedAt == null && permissionUids.Contains(p.Uid))
                .Select(x => x.Code)
                .ToList()
                .ToOneString("^");

            //不要放 Json, 字串會太長,
            AddAdminLoginCookie(userUid, permissionCodes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adminUserUid"></param>
        /// <param name="adminPermissionCodes">用 ^ 分隔</param>
        public static void AddAdminLoginCookie(string adminUserUid, string adminPermissionCodes)
        {
            Su.Wu.AddCookie(AuthCookieName, Su.Encryption.AesEncryptCookie($"{DateTime.Now.ISO8601()},{adminUserUid},{adminPermissionCodes}"));
        }

        /// <summary>
        /// 團購網, 前台登入用
        /// </summary>
        /// <param name="memberUid"></param>
        public static void AddLoginCookie(string memberUid)
        {
            Su.Wu.AddCookie(AuthCookieName, Su.Encryption.AesEncryptCookie($"{DateTime.Now.ISO8601()},{memberUid}"));
        }

        /// <summary>
        /// 登出(清除 Cookie)
        /// </summary>
        public static void LogOut()
        {
            Su.Wu.RemoveCookie(AuthCookieName);
        }

        ///// <summary>
        ///// CreatorName = AuthHelper.LoginAdmin.Name,
        ///// CreatorUid
        ///// CreatedAt
        ///// ModifierName
        ///// ModifierUid
        ///// ModifyAt
        ///// Uid
        ///// </summary>
        ///// <param name="target"></param>
        ///// <param name="additionalInfo">其它相關欄位的物件</param>
        ///// <exception cref="Exception"></exception>
        //public static T SetCreatorInfoAndUid<T>(T target, object? additionalInfo = null)
        //{
        //    if (AuthHelper.LoginAdmin == null)
        //    {
        //        throw new Exception("請先登入。");
        //    }

        //    if (additionalInfo != null)
        //    {
        //        Su.ObjUtil.CopyTo(additionalInfo, target, skips: "Uid");
        //    }

        //    var creatorObject = new
        //    {
        //        CreatorName = AuthHelper.LoginAdmin.Name,
        //        CreatorUid = AuthHelper.LoginAdmin.Uid,
        //        CreatedAt = DateTime.Now,
        //        ModifierName = AuthHelper.LoginAdmin.Name,
        //        ModifierUid = AuthHelper.LoginAdmin.Uid,
        //        ModifyAt = DateTime.Now,
        //        Uid = Guid.NewGuid().ToString(),
        //    };

        //    Su.ObjUtil.CopyTo(creatorObject, target);

        //    return target;
        //}

        //public static void SetDeleteInfo(object target)
        //{
        //    if (AuthHelper.LoginAdmin == null)
        //    {
        //        throw new Exception("請先登入。");
        //    }

        //    var deleteInfo = new
        //    {
        //        DeleterName = AuthHelper.LoginAdmin.Name,
        //        DeleterUid = AuthHelper.LoginAdmin.Uid,
        //        DeletedAt = DateTime.Now,
        //    };

        //    Su.ObjUtil.CopyTo(deleteInfo, target);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="target"></param>
        ///// <param name="additionalInfo"></param>
        //public static void SetModiferInfo(object target, object? additionalInfo = null)
        //{
        //    if (AuthHelper.LoginAdmin == null)
        //    {
        //        throw new Exception("請先登入。");
        //    }

        //    if(additionalInfo != null)
        //    {
        //        Su.ObjUtil.CopyTo(additionalInfo, target, skips: "Uid");
        //    }

        //    var modifyInfo = new
        //    {
        //        ModifierName = AuthHelper.LoginAdmin.Name,
        //        ModifierUid = AuthHelper.LoginAdmin.Uid,
        //        ModifyAt = DateTime.Now,
        //    };

        //    Su.ObjUtil.CopyTo(modifyInfo, target);
        //}

        /// <summary>
        /// 登入的管理者資料
        /// </summary>
        public static Dtos.AdminUserDto? LoginAdmin
        {
            get
            {
                if (!IsLogin)
                {
                    return null;
                }

                string key = "LoginAdminUsers";
                if (!Su.CurrentContext.Items.ContainsKey(key))
                {
                    CurrentContext.Items[key] = AdminUserHelper.GetOne(Core.Ef.CBCTContext.NewDbContext, AdminUserUid)
                        ?? throw new Exception("使用者不存在: " + AdminUserUid);
                }

                return (Dtos.AdminUserDto)Su.CurrentContext.Items[key];
            }
        }

        ///// <summary>
        ///// Create Member From Line Profile
        ///// </summary>
        ///// <param name="memberLine"></param>
        ///// <returns></returns>
        //public static Core.Ef.Member CreateMemberFromLineProfile(Core.Ef.MemberLine memberLine)
        //{
        //    var member = new Core.Ef.Member();
        //    member.Uid = memberLine.MemberUid;
        //    member.Nickname = memberLine.DisplayName;
        //    member.Source = "line";

        //    return member;
        //}

        //public static async Task<bool> LineLoginAsync(string code, string state, string lineCallBackUrl)
        //{
        //    var lineLoginInfo = new Core.Dtos.LineDtos.LoginInfo
        //    {
        //        Code = code,
        //        State = state,
        //        CallBackUrl = lineCallBackUrl
        //    };

        //    try
        //    {
        //        var profile = await LineHelper.GetLineProfileAsync(lineLoginInfo);
        //        if (profile == null)
        //        {
        //            return false;
        //        }

        //        var ct = Core.Ef.CBCTContext.NewDbContext;
        //        var memberLine = await ct.MemberLines.Where(ml => ml.LineUid == profile.userId).FirstOrDefaultAsync();
        //        if (memberLine == null)
        //        {
        //            //建立 member_line
        //            memberLine = new Core.Ef.MemberLine();
        //            memberLine.LineUid = profile.userId;
        //            memberLine.MemberUid = Guid.NewGuid().ToString();
        //            memberLine.DisplayName = profile.displayName;
        //            memberLine.PictureUrl = profile.pictureUrl;
        //            ct.MemberLines.Add(memberLine);

        //            //建立 member
        //            ct.Members.Add(CreateMemberFromLineProfile(memberLine));
        //            await ct.SaveChangesAsync();
        //        }
        //        else if (!ct.Members.Any(m => m.Uid == memberLine.MemberUid))
        //        {
        //            ct.Members.Add(CreateMemberFromLineProfile(memberLine));
        //            await ct.SaveChangesAsync();
        //        }

        //        AddLoginCookie(memberLine.MemberUid);
        //    }
        //    catch (Exception ex)
        //    {
        //        Su.FileLogger.AddDailyLog("LineLogin", ex.FullInfo());

        //        throw new CustomException(HttpStatusCode.BadRequest, $"Line 登入錯誤: {ex.Message}");
        //    }

        //    return true;
        //}

        //public static Core.Ef.Member? LoginMember
        //{
        //    get
        //    {
        //        const string itemKey = "_LoginMember_";
        //        if (Su.CurrentContext.Items.ContainsKey(itemKey))
        //        {
        //            return (Core.Ef.Member?)Su.CurrentContext.Items[itemKey];
        //        }

        //        if (string.IsNullOrEmpty(AuthHelper.MemberUid))
        //        {
        //            Su.CurrentContext.Items[itemKey] = null;
        //            return null;
        //        }

        //        var member = Core.Ef.CBCTContext.NewDbContext.Members.Where(m => m.Uid == AuthHelper.MemberUid).FirstOrDefault();
        //        if (member == null) //這個情況應該很少發生，直接回傳，不做 Cache
        //        {
        //            return null;
        //        }

        //        Su.CurrentContext.Items[itemKey] = member;

        //        return (Core.Ef.Member?)Su.CurrentContext.Items[itemKey];
        //    }
        //}
    }
}
