using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using Su;
using Core.Helpers;

namespace AdminApi
{
    /// <summary>
    /// 權限管理
    /// </summary>
    [Route("admin-permission")]
    [AddPermission(Core.Constants.AdminPermission.Admin)]
    public class AdminPermissionController : BaseApiController
    {
        public AdminPermissionController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env,
            Core.Ef.CBCTContext CBCTContext, AuthHelper authHelper) : base(commonClass, env, CBCTContext, authHelper)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public ActionResult<dynamic> List([FromQuery] int? currentPage, [FromQuery] int? pageSize)
        {
            var query = _dbContext.AdminPermissions.AsNoTracking().Where(x => x.DeletedAt == null).OrderBy(x => x.Sort).ThenBy(x => x.Id);
            return new Su.PageList<Core.Ef.AdminPermission>((IOrderedQueryable<Core.Ef.AdminPermission>)query, currentPage, pageSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public Core.Ef.AdminPermission? GetOne([FromQuery] string uid)
        {
            return _dbContext.AdminPermissions.AsNoTracking()
                .Where(x => x.Uid == uid && x.DeletedAt == null)
                .FirstOrDefault();
        }

        /// <summary>
        /// 取得單一權限關連到的角色清單
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("get-roles")]
        public IEnumerable<Core.Ef.AdminRole> GetPermissionRoles([FromQuery] string uid)
        {
            var roleList = _dbContext.AdminRoleAdminPermissions.AsNoTracking().Where(x => x.PermissionUid == uid).Select(x => x.AdminRoleUid).ToList();
            return _dbContext.AdminRoles.AsNoTracking().Where(x => roleList.Contains(x.Uid));
        }

        /// <summary>
        /// 建立 Permission
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public Core.Dtos.PermissionDto Upsert(Core.Dtos.PermissionDto dto)
        {
            var ex = dto.GetCustomException();
            ex.TryThrowValidationException();

            dto.Code = dto.Code.Trim();
            // 檢查有無重複 name
            if (_dbContext.AdminPermissions.Where(x => x.Code == dto.Code
                && (string.IsNullOrEmpty(x.Uid) || x.Uid != x.Uid)).Any())
            {
                ex.AddValidationError("Code", "權限代碼重複");
            }
            ex.TryThrowValidationException();

            string adminUid = _authHelper.LoginUid!;
            Core.Ef.AdminPermission? entity;
            if (string.IsNullOrEmpty(dto.Uid))
            {
                //新增
                entity = new Core.Ef.AdminPermission
                {
                    Uid = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    CreatorUid = adminUid
                };
            }
            else
            {
                //更新
                entity = _dbContext.AdminPermissions.Where(x => x.Uid == dto.Uid).FirstOrDefault();
                if (entity == null)
                {
                    throw new CustomException(HttpStatusCode.NotFound, "查無此權限資料");
                }

                if (entity.Code != dto.Code)
                {
                    ex.AddValidationError("Name", "權限代碼禁止修改");
                }
            }
            dto.CopyTo(entity, skips: "Uid");
            entity.ModifiedAt = DateTime.Now;
            entity.ModifierUid = adminUid;

            if (string.IsNullOrEmpty(dto.Uid))
            {
                _dbContext.AdminPermissions.Add(entity);
            }
            _dbContext.SaveChanges();

            dto.Uid = entity.Uid;
            return dto;
        }
    }
}