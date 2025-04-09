using Microsoft.EntityFrameworkCore;
using System.Runtime.Caching;

namespace Core.Helpers
{
    public partial class MenuHelper
    {
        private const string GET_MENU_CACHE_KEY = "Core.DataService.GetMenuWithCache";

        public static void RemoveMenuCache()
        {
            MemoryCache.Default.Remove(GET_MENU_CACHE_KEY);
        }

        /// <summary>
        /// Cache 2 分鐘
        /// </summary>
        /// <returns></returns>
        public static List<Dtos.MenuForClient> GetMenuWithCache(Ef.CBCTContext dbContext)
        {
            // this.AddBaseLog("GetCurrentMenu Start.");
            string cacheKey = GET_MENU_CACHE_KEY;
            ObjectCache cache = MemoryCache.Default;
            var menuList = cache[cacheKey] as List<Dtos.MenuForClient>;
            if (menuList == null)
            {
                lock (Su.LockerProvider.GetLocker($"CacheLocker.{cacheKey}"))
                {
                    menuList = cache[cacheKey] as List<Dtos.MenuForClient>;
                    if (menuList == null)
                    {
                        var menuPermissions = dbContext.AdminPermissions.Join(
                            dbContext.AdminMenuPermissions,
                            permission => permission.Uid,
                            adminMenuPermission => adminMenuPermission.PermissionUid,
                            (permission, adminMenuPermission) => new
                            {
                                permission.Code,
                                adminMenuPermission.AdminMenuUid,
                                PdeletedAt = permission.DeletedAt,
                                MPdeletedAt = adminMenuPermission.DeletedAt
                            }).Where(x => x.PdeletedAt == null && x.MPdeletedAt == null);

                        menuList = dbContext.AdminMenus.AsNoTracking()
                            .Where(x => x.DeletedAt == null)
                            .OrderBy(x => x.Sort)
                            .Select(menu => new Dtos.MenuForClient
                            {
                                Id = menu.Uid,
                                Text = menu.Name,
                                Link = menu.Link,
                                Parent = menu.ParentUid,
                                Sort = menu.Sort,
                                Permissions = menuPermissions.Where(mp => mp.AdminMenuUid == menu.Uid).Select(mp => mp.Code).ToList()
                            }).ToList();

                        var policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTime.UtcNow.AddMinutes(2);
                        MemoryCache.Default.Set(cacheKey, menuList, policy);
                    }
                }
                // this.AddBaseLog("GetCurrentMenu From DB.");
            }
            else
            {

                // this.AddBaseLog("GetCurrentMenu From Cache.");
            }

            return menuList;
        }
    }
}
