using Core.Ef;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using Su;

namespace Core.Helpers
{
    public class SysConfigHelper
    {
        /// <summary>
        /// 取得全部參數的字典
        /// </summary>
        /// <returns></returns>
        static internal Dictionary<string, string> GetDictionaryFromDb()
        {
            var ct = Core.Ef.CBCTContext.NewDbContext;

            var list = ct.SysConfigs.Select(x => new {x.Name, x.Content}).ToList();

            return list.ToList().ToDictionary(x => x.Name, x => x.Content);
        }

        /// <summary>
        /// 取得清單
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="keyword">關鍵字. 檢查名稱/描述有符合的資料</param>
        /// <returns></returns>
        public static IOrderedQueryable<SysConfig> GetQuery(CBCTContext ct, string? keyword = "")
        {
            return ct.SysConfigs.Where(x => (string.IsNullOrEmpty(keyword) || x.Name.Contains(keyword) || x.Description.Contains(keyword)))
                .OrderBy(x => x.Name)
                .ThenByDescending(x => x.Id);
        }

        /// <summary>
        /// 取得單一資料
        /// </summary>
        /// <returns></returns>
        public static  Core.Ef.SysConfig GetOne(CBCTContext _dbContext, string uid)
        {
            var data = _dbContext.SysConfigs.FirstOrDefault(x => x.Uid == uid);
            if (data == null)
            {
                throw new CustomException(HttpStatusCode.NotFound, "查無此資料");
            }

            return data;
        }

        /// <summary>
        /// 公司新增/修改時檢查是否有重複
        /// </summary>
        /// <param name="_dbContext"></param>
        /// <param name="dto"></param>
        /// <param name="ex"></param>
        private static void CheckForDuplicate(CBCTContext _dbContext, Dtos.UpsertSysConfig dto, CustomException ex)
        {
            var duplicateSysConfig = _dbContext.SysConfigs.AsNoTracking()
                .Where(r => r.Name == dto.Name && r.Uid != dto.Uid)
                .FirstOrDefault();

            if (duplicateSysConfig != null)
            {
                if (duplicateSysConfig.Name == dto.Name)
                {
                    ex.AddValidationError("Name", "此名稱已存在");
                }
            }

            ex.TryThrowValidationException();
        }

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <param name="_dbContext"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static dynamic AddOneData(CBCTContext _dbContext, Dtos.UpsertSysConfig dto, Dtos.AdminUserDto? adminUser)
        {
            var ex = dto.GetCustomException();

            if (!string.IsNullOrEmpty(dto.Uid))
            {
                ex.AddValidationError("Uid", "此為新增功能，無須Uid");
            }

            ex.TryThrowValidationException();

            CheckForDuplicate(_dbContext, dto, ex);

            var newData = dto.CopyTo<SysConfig>();
            newData.Uid = Guid.NewGuid().ToString();
            newData.CreatedAt = DateTime.Now;
            newData.CreatorUid = adminUser.Uid!;
            newData.CreatorName = adminUser.Name;
            newData.ModifiedAt = DateTime.Now;
            newData.ModifierUid = adminUser.Uid!;
            newData.ModifierName = adminUser.Name;
            _dbContext.SysConfigs.Add(newData);
            _dbContext.SaveChanges();

            return newData;
        }

        /// <summary>
        /// 修改資料
        /// </summary>
        /// <param name="_dbContext"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static dynamic UpdateOneData(CBCTContext _dbContext, Dtos.UpsertSysConfig dto, Dtos.AdminUserDto? adminUser)
        {
            var ex = dto.GetCustomException();

            if (string.IsNullOrEmpty(dto.Uid))
            {
                ex.AddValidationError("Uid", "此為修改功能，須有Uid");
            }

            ex.TryThrowValidationException();

            CheckForDuplicate(_dbContext, dto, ex);

            var data = _dbContext.SysConfigs.FirstOrDefault(x => x.Uid == dto.Uid);
            if (data == null)
            {
                throw new CustomException(HttpStatusCode.NotFound, "查無此資料");
            }

            dto.CopyTo(data, skips: "Uid");
            data.ModifiedAt = DateTime.Now;
            data.ModifierUid = adminUser.Uid!;
            data.ModifierName = adminUser.Name;

            _dbContext.SaveChanges();

            return data;
        }
    }
}
