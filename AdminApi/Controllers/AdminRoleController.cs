using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using Core.Helpers;
using Su;

namespace AdminApi
{
    /// <summary>
    /// 角色管理
    /// </summary>
    [Route("admin-role")]
    [AddPermission(Core.Constants.AdminPermission.Admin)]
    public class AdminRoleController : BaseApiController
    {
        public AdminRoleController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env, Core.Ef.CBCTContext CBCTContext)
            : base(commonClass, env, CBCTContext)
        {
        }

        /// <summary>
        /// 取得 Role 清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        public IEnumerable<Core.Ef.AdminRole> List()
        {
            return _dbContext.AdminRoles.AsNoTracking().Where(x => x.DeletedAt == null).OrderBy(x => x.Name);
        }

        /// <summary>
        /// 取得 Role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet()]
        public Core.Dtos.AdminRoleDto? GetOne([FromQuery] string uid)
        {
            var menu = _dbContext.AdminRoles.AsNoTracking().Where(x => x.Uid == uid).FirstOrDefault();

            var ret = menu.CopyTo<Core.Dtos.AdminRoleDto>();

            var permissions = _dbContext.AdminPermissions.Join(
                            _dbContext.AdminRoleAdminPermissions,
                            permission => permission.Uid,
                            adminRolePermission => adminRolePermission.PermissionUid,
                            (permission, adminRolePermission) => new
                            {
                                permission.Code,
                                adminRolePermission.AdminRoleUid,
                                PdeletedAt = permission.DeletedAt,
                                MPdeletedAt = adminRolePermission.DeletedAt
                            }).Where(x => x.PdeletedAt == null && x.MPdeletedAt == null && x.AdminRoleUid == uid);

            ret.Permissions = permissions.Select(x => x.Code).ToOneString(",");

            return ret;
        }

        /// <summary>
        /// Upsert Role
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public Core.Dtos.AdminRoleDto? Upsert(Core.Dtos.AdminRoleDto dto)
        {
            var ex = dto.GetCustomException();
            ex.TryThrowValidationException();

            dto.Name = dto.Name!.Trim();
            string adminUid = Core.Helpers.AuthHelper.LoginUid!;
            Core.Ef.AdminRole? entity;
            if (string.IsNullOrEmpty(dto.Uid))
            {
                //新增
                entity = new Core.Ef.AdminRole
                {
                    Uid = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.Now,
                    CreatorUid = adminUid
                };
                _dbContext.AdminRoles.Add(entity);
            }
            else
            {
                //更新
                entity = _dbContext.AdminRoles.Where(x => x.Uid == dto.Uid).FirstOrDefault();
                if (entity == null)
                {
                    throw new CustomException(HttpStatusCode.NotFound, "查無此 Role");
                }
            }
            dto.CopyTo(entity, skips: "Uid");
            entity.ModifiedAt = DateTime.Now;
            entity.ModifierUid = adminUid;

            //if (string.IsNullOrEmpty(dto.Uid))
            //{
            //    _dbContext.AdminRoles.Add(entity);
            //}
            _dbContext.SaveChanges();

            var permissionList = _dbContext.AdminPermissions.AsNoTracking().ToList();
            using (var scope = new System.Transactions.TransactionScope())
            {
                if (dto.Permissions != null)
                {
                    // 所屬群組權限全部刪除再新增
                    var permissions = dto.Permissions.Split(',');
                    _dbContext.AdminRoleAdminPermissions.Where(x => x.AdminRoleUid == entity.Uid).ExecuteDelete();
                    foreach (string permission in permissions)
                    {
                        if (!permissionList.Where(x => x.Code == permission).Any()) continue;
                        Core.Ef.AdminRoleAdminPermission menuPermission = new()
                        {
                            AdminRoleUid = entity.Uid,
                            PermissionUid = permissionList.Where(x => x.Code == permission).First().Uid,
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                            CreatorUid = AuthHelper.LoginUid!,
                            ModifierUid = AuthHelper.LoginUid!,
                        };
                        _dbContext.Add(menuPermission);
                    }
                }

                _dbContext.SaveChanges();
                scope.Complete();
            }

            //Core.DataService.RoleDataService.RemoveRoleCache();

            return GetOne(entity.Uid);
        }

        /// <summary>
        /// 刪除 Role
        /// </summary>
        /// <param name="uid"></param>
        /// <exception cref="Su.CustomException"></exception>
        [HttpDelete()]
        public void DeleteRole([FromQuery] string uid)
        {
            var entity = _dbContext.AdminRoles.Where(x => x.Uid == uid).FirstOrDefault()
                ?? throw new Su.CustomException(HttpStatusCode.NotFound, "查無此 Role");

            entity.DeletedAt = DateTime.Now;
            _dbContext.SaveChanges();
        }
    }
}
