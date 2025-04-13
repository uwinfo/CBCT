using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Core.Constants;
using Core.Helpers;
using System.Net;
using System.Data;
using Su;
using Microsoft.EntityFrameworkCore;

namespace AdminApi
{
    /// <summary>
    /// 後台目錄管理
    /// </summary>
    [Route("admin-menu")]
    public class AdminMenuController : BaseApiController
    {
        public AdminMenuController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env,
            Core.Ef.CBCTContext CBCTContext, AuthHelper authHelper) : base(commonClass, env, CBCTContext, authHelper)
        {
        }

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
        public List<Core.Dtos.MenuForClient> GetUserMenusWithPermission()
        {
            // 這是for編輯  所以先清除cache
            //Core.Helpers.MenuHelper.RemoveMenuCache();
            //return Core.Helpers.MenuHelper.GetMenuWithCache(_dbContext);
            var menuList = new List<Core.Dtos.MenuForClient>();

            var userPermissions = _authHelper.AdminPermissions;
            if (userPermissions != null)
            {
                var permissions = _dbContext.AdminPermissions.AsNoTracking()
                    .Where(x => x.DeletedAt == null && userPermissions.Select(x => x.ToString()).Contains(x.Code))
                    .Select(x => new { x.Uid, x.Code })
                    .ToList();

                var menus = _dbContext.AdminMenus.AsNoTracking()
                    .Where(x => x.DeletedAt == null &&
                                    (x.PermissionUid == null ||
                                        userPermissions.Contains(AdminPermission.Admin) ||
                                        (permissions == null || permissions.Select(p => p.Uid).Contains(x.PermissionUid)))
                    ).OrderBy(x => x.Sort)
                    .ToList();

                if (menus != null && menus.Count > 0)
                {
                    menuList = menus
                        .Select(menu => new Core.Dtos.MenuForClient
                        {
                            Id = menu.Uid,
                            Text = menu.Name,
                            Link = menu.Link,
                            Parent = menu.ParentUid,
                            Sort = menu.Sort,
                        }).ToList();
                }
            }

            return menuList;
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

            if (menu.PermissionUid != null && menu.PermissionUid.Length > 0)
            {
                var permission = _dbContext.AdminPermissions.FirstOrDefault(x => x.DeletedAt == null && x.Uid == menu.PermissionUid);
                if (permission != null)
                {
                    ret.PermissionDesc = permission.Description + " " + permission.Code;
                }
            }

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

            string adminUid = _authHelper.LoginUid!;

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

            if (dto.PermissionUid != null && dto.PermissionUid.Length > 0)
            {
                var permission = _dbContext.AdminPermissions.FirstOrDefault(x => x.DeletedAt == null && x.Uid == dto.PermissionUid);
                if (permission != null)
                {
                    entity.PermissionUid = permission.Uid;
                }
            }

            if (string.IsNullOrEmpty(dto.Uid))
            {
                _dbContext.AdminMenus.Add(entity);
            }
            _dbContext.SaveChanges();

            //MenuHelper.RemoveMenuCache();

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

            //MenuHelper.RemoveMenuCache();
        }
    }
}
