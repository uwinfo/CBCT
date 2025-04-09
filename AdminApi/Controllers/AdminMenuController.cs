using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SystemAdmin.AdminApiController;
using Core.Constants;
using Core.Helpers;
using System.Net;
using System.Data;
using Su;

namespace AdminApi.Controllers
{
    /// <summary>
    /// 後台目錄管理
    /// </summary>
    /// <param name="appSettings"></param>
    /// <param name="env"></param>
    /// <param name="CBCTContext"></param>
    [Route("admin-menu")]
    [ApiController]
    public class AdminMenuController(IOptions<Core.Models.AdminAppSettings> appSettings, IWebHostEnvironment env, Core.Ef.CBCTContext CBCTContext) : AdminApiControllerBase(appSettings, env, CBCTContext)
    {
        /// <summary>
        /// 取得 Menu 清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [AddPermission(AdminPermission.MenuManagement)]
        public IEnumerable<Core.Ef.AdminMenu> List()
        {
            return _dbContext.AdminMenus.AsNoTracking().Where(x => x.DeletedAt == null).OrderBy(x => x.ParentUid).ThenBy(x => x.Sort);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("login-user-menu")]
        [AddPermission(AdminPermission.Login)]
        public List<Core.Dtos.MenuForClient> GetAllMenusWithPermission()
        {
            // 這是for編輯  所以先清除cache
            Core.Helpers.MenuHelper.RemoveMenuCache();
            return Core.Helpers.MenuHelper.GetMenuWithCache(_dbContext);
        }

        /// <summary>
        /// 取得 Menu
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet()]
        [AddPermission(AdminPermission.MenuManagement)]
        public Core.Dtos.AdminMenuDto? GetOne([FromQuery]string uid)
        {
            var menu = _dbContext.AdminMenus.AsNoTracking().Where(x => x.Uid == uid).FirstOrDefault();

            var ret = menu.CopyTo<Core.Dtos.AdminMenuDto>();

            var permissions = _dbContext.AdminPermissions.Join(
                            _dbContext.AdminMenuPermissions,
                            permission => permission.Uid,
                            adminMenuPermission => adminMenuPermission.PermissionUid,
                            (permission, adminMenuPermission) => new
                            {
                                permission.Code,
                                adminMenuPermission.AdminMenuUid,
                                PdeletedAt = permission.DeletedAt,
                                MPdeletedAt = adminMenuPermission.DeletedAt
                            }).Where(x => x.PdeletedAt == null && x.MPdeletedAt == null && x.AdminMenuUid == uid);

            ret.Permissions = permissions.Select(x => x.Code).ToOneString(",");

            return ret;
        }

        /// <summary>
        /// Upsert Menu
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [AddPermission(AdminPermission.MenuManagement)]
        public Core.Dtos.AdminMenuDto? Upsert(Core.Dtos.AdminMenuDto dto)
        {
            var ex = dto.GetCustomException();
            ex.TryThrowValidationException();

            dto.Name = dto.Name!.Trim();
            // 檢查 parent
            if (dto.ParentUid != "#")
            {
                if (!_dbContext.AdminMenus.Where(x => x.Uid == dto.ParentUid && x.DeletedAt == null).Any())
                {
                    ex.AddValidationError("parent", $"查無上一層Menu({dto.ParentUid})");
                }
            }
            ex.TryThrowValidationException();

            string adminUid = Core.Helpers.AuthHelper.AdminUserUid!;

            Core.Ef.AdminMenu? entity;
            if (string.IsNullOrEmpty(dto.Uid))
            {
                //新增
                entity = new Core.Ef.AdminMenu
                {
                    Uid = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    CreatorUid = adminUid
                };
            }
            else
            {
                //更新
                entity = _dbContext.AdminMenus.Where(x => x.Uid == dto.Uid).FirstOrDefault();
                if (entity == null)
                {
                    throw new CustomException(HttpStatusCode.NotFound, "查無此 Menu");
                }
            }
            dto.CopyTo(entity, skips: "Uid");
            entity.ModifiedAt = DateTime.Now;
            entity.ModifierUid = adminUid;

            if (string.IsNullOrEmpty(dto.Uid))
            {
                _dbContext.AdminMenus.Add(entity);
            }
            _dbContext.SaveChanges();

            var permissionList = _dbContext.AdminPermissions.AsNoTracking().ToList();
            using (var scope = new System.Transactions.TransactionScope())
            {
                if (dto.Permissions != null)
                {
                    // 此系統的 MenuPermission.DeletedAt 不需要 直接珊
                    // 所屬群組權限全部刪除再新增
                    var permissions = dto.Permissions.Split(',');
                    _dbContext.AdminMenuPermissions.Where(x => x.AdminMenuUid == entity.Uid).ExecuteDelete();
                    foreach (string permission in permissions)
                    {
                        if (!permissionList.Where(x => x.Code == permission).Any()) continue;
                        Core.Ef.AdminMenuPermission menuPermission = new()
                        {
                            AdminMenuUid = entity.Uid,
                            PermissionUid = permissionList.Where(x => x.Code == permission).First().Uid,
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                            CreatorUid = AuthHelper.AdminUserUid!,
                            ModifierUid = AuthHelper.AdminUserUid!,
                        };
                        _dbContext.Add(menuPermission);
                    }
                }

                _dbContext.SaveChanges();
                scope.Complete();
            }

            MenuHelper.RemoveMenuCache();

            return GetOne(entity.Uid);
        }

        /// <summary>
        /// 刪除 Menu
        /// </summary>
        /// <param name="uid"></param>
        /// <exception cref="Su.CustomException"></exception>
        [HttpDelete()]
        [AddPermission(AdminPermission.MenuManagement)]
        public void DeleteMenu([FromQuery] string uid)
        {
            var entity = _dbContext.AdminMenus.Where(x => x.Uid == uid).FirstOrDefault() 
                ?? throw new Su.CustomException(HttpStatusCode.NotFound, "查無此 Menu");

            entity.DeletedAt = DateTime.Now;
            _dbContext.SaveChanges();

            MenuHelper.RemoveMenuCache();
        }
    }
}
