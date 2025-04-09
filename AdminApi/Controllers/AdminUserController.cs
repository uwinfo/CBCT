using AdminApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Core.Dtos;
using Core.Helpers;

namespace SystemAdmin.AdminApiController
{
    /// <summary>
    /// 管理後台使用者 
    /// </summary>
    [Route("admin-user")]
    [AddPermission(Core.Constants.AdminPermission.UserManagement)]
    public class AdminUserController(IOptions<Core.Models.AdminAppSettings> appSettings, IWebHostEnvironment env, Core.Ef.CBCTContext CBCTContext) : AdminApiControllerBase(appSettings, env, CBCTContext)
    {
        /// <summary>
        /// 在 AdminUser 之中新增記錄
        /// </summary>
        /// <param name="message"></param>
        void AddLog(string message)
        {
            Su.FileLogger.AddDailyLog("AdminUser", message);
        }

        /// <summary>
        ///取得系統管理員清單
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<dynamic> GetList([FromQuery] string? keyword, [FromQuery] int? currentPage, [FromQuery] int? pageSize)
        {
            return new Su.PageList<Core.Dtos.AdminUserDto>(AdminUserHelper.GetQuery(_dbContext, keyword), currentPage, pageSize);
        }

        /// <summary>
        ///取得單一系統管理員
        /// </summary>
        /// <returns></returns>
        [Route("get")]
        [HttpGet]
        public AdminUserDto? GetOne([FromQuery] string uid)
        {
            return AdminUserHelper.GetOne(_dbContext, uid);
        }

        /// <summary>
        /// 新增/修系統管理員
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<dynamic> Upsert([FromBody] Core.Dtos.AdminUserDto dto)
        {
            return AdminUserHelper.Upsert(_dbContext, dto);
        }

        /// <summary>
        ///刪除系統管理員
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpDelete]
        public void Delete([FromQuery] string uid)
        {
            AdminUserHelper.Delete(_dbContext, uid);
        }

        [Route("get-auth")]
        [HttpGet]
        public dynamic? GetAuth()
        {
            return Core.Helpers.AuthHelper.AdminPermissions;
        }
    }
}