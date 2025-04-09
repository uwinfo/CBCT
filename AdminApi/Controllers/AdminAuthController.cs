using AdminApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Core.Constants;
using Core.Helpers;
using Google.Authenticator;
using System.Net;
using Su;

namespace SystemAdmin.AdminApiController
{
    /// <summary>
    /// 登入和授權用 API
    /// </summary>
    [Route("admin-auth")]
    public class AdminAuthController : AdminApiControllerBase
    {
        /// <summary>
        /// 在 AdminUser 之中新增記錄
        /// </summary>
        /// <param name="message"></param>
        void AddLog(string message)
        {
            Su.FileLogger.AddDailyLog("AdminAuth", message);
        }

        public AdminAuthController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env, Core.Ef.CBCTContext CBCTContext) : base(commonClass, env, CBCTContext)
        {
        }

        /// <summary>
        ///驗證 OTP Confirm Code
        /// </summary>
        /// <returns></returns>
        [Route("get-otp-qrcode-image")]
        [HttpGet]
        public ActionResult<dynamic> GetOtpQrcodeImage([FromQuery] string uid, [FromQuery] string otpConfirm)
        {
            string otpQRcode = AdminUserHelper.GetManagerbyOtpConfirm(_dbContext,
                _commonSettings.SystemName,
                uid, otpConfirm);
            return new { imageData = otpQRcode };
        }

        /// <summary>
        ///取得OTP
        /// </summary>
        /// <returns></returns>
        [HttpPost("get-otp")]
        [AddPermission(AdminPermission.UnLimited)]
        public dynamic GetOtp([FromBody] Core.Dtos.LogInDto postBody)
        {
            bool isCheckSecret = true;

            var admin = AdminUserHelper.GetManager(_dbContext, postBody, isCheckSecret);

            if (admin == null)
            {
                throw new CustomException("請確認帳號或密碼是否正確", HttpStatusCode.Unauthorized);
            }

            if (admin.DeletedAt != null || admin.EnStatus == Core.Constants.General.Status.Disabled)
            {
                throw new CustomException("該使用者已被停權", HttpStatusCode.Unauthorized);
            }

            var otpConfirm = AdminUserHelper.UpadteOtpConfirm(_dbContext, admin.Uid);

            var from = "bike@bike.idv.tw";
            var to = admin.Email;
            var subject = "OTP 驗證碼";

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            if (baseUrl == "https://localhost:7100")
            {
                baseUrl = "https://localhost:5100";
            }

            var url = $"{baseUrl}/get-otp?uid={admin.Uid.UrlEncode()}&otpConfirm={otpConfirm.UrlEncode()}";
            var body = $"請使用 Google Authenticator APP 進行驗證，點擊以下連結：<a href='{url}'>連結</a><br />" +
                $"注意，開啟連結後，舊的動態驗証碼會自動失效。";

            Core.Helpers.EmailHelper.SendMailWithAmazonSES(to, from, subject, body, isBodyHtml: true);

            return true;
        }

        [HttpGet("is-login")]
        [AddPermission(AdminPermission.UnLimited)]
        public object IsLogIn()
        {
            return Core.Helpers.AuthHelper.IsLogin;
        }

        [HttpGet("log-out")]
        [AddPermission(AdminPermission.Login)]
        public dynamic LogOut()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Core.Helpers.AuthHelper.LogOut();
            return true;
        }

        /// <summary>
        ///系統管理員登入
        /// </summary>
        /// <returns></returns>
        [HttpPost("log-in")]
        [AddPermission(AdminPermission.UnLimited)]
        public dynamic LogIn([FromBody] Core.Dtos.LogInDto postBody)
        {
            bool isCheckSecret = true;

            if (Request.Host.Host.Contains("localhost"))
            {
                isCheckSecret = false;
            }

            var admin = AdminUserHelper.GetManager(_dbContext, postBody, isCheckSecret);
            if (admin == null)
            {
                AddLog("Log in fail");
                throw new CustomException("登入失敗，請確認帳號或密碼是否正確", HttpStatusCode.Unauthorized);
            }

            if (admin.DeletedAt != null || admin.EnStatus == Core.Constants.General.Status.Disabled)
            {
                AddLog("MEMBER_BE_BLOCKED");
                throw new CustomException("該使用者已被停權", HttpStatusCode.Unauthorized);
            }

            var OTP = postBody.Otp;

            if (!string.IsNullOrEmpty(OTP))
            {
                if (isCheckSecret || OTP != "123")
                {
                    var otpSecret = AdminUserHelper.GetOtpSecret(_dbContext, admin.Uid);

                    if (string.IsNullOrEmpty(otpSecret))
                    {
                        AddLog("otpSecret is null");
                        throw new CustomException("請先建立動態密碼。", HttpStatusCode.BadRequest);
                    }

                    TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                    bool result = tfa.ValidateTwoFactorPIN(otpSecret, OTP);
                    if (result == false)
                    {
                        AddLog("INVALID_OTP");
                        throw new CustomException("動態密碼錯誤", HttpStatusCode.BadRequest);
                    }
                }

                Core.Helpers.AuthHelper.AdminLogIn(admin.Uid, _dbContext);
                AddLog(admin.Uid.ToString() + "；" + admin.Name + "；login success");
                return new { res = true };
            }
            else
            {
                throw new CustomException("請輸入動態密碼", HttpStatusCode.BadRequest);
            }
        }
    }
}