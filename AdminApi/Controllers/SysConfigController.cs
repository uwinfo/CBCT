using Core.Helpers;
using System.Data;
using System.Transactions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi
{
    /// <summary>
    /// 系統參數
    /// </summary>
    [Route("sys_config")]
    [AddPermission(Core.Constants.AdminPermission.Admin)]
    public class SysConfigController : BaseApiController
    {
        public SysConfigController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env,
            Core.Ef.CBCTContext CBCTContext, AuthHelper authHelper) : base(commonClass, env, CBCTContext, authHelper)
        {
        }

        /// <summary>
        /// 取得系統參數清單
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="keyword">關鍵字. 篩選系統參數名稱/系統參數負責人有符合的資料</param>
        /// <returns></returns>
        [HttpGet("list")]
        public ActionResult<dynamic> GetSysConfigs([FromQuery] int? currentPage, [FromQuery] int? pageSize, string? keyword = "")
        {
            var query = SysConfigHelper.GetQuery(_dbContext, keyword);
            var newQuery = query.Select(x => new Core.Dtos.SysConfigList
            {
                Id = x.Id,
                Uid = x.Uid,
                Name = x.Name,
                Content = x.Content,
                Description = x.Description,
                ModifiedAt = x.ModifiedAt,
                ModifierUid = x.ModifierUid,
                ModifierName = x.ModifierName,
            });
            return new Su.PageList<Core.Dtos.SysConfigList>((IOrderedQueryable<Core.Dtos.SysConfigList>)newQuery, currentPage, pageSize);
        }

        /// <summary>
        /// 取得單一系統參數資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public ActionResult<dynamic> GetSysConfig([FromQuery] string uid)
        {
            return SysConfigHelper.GetOne(_dbContext, uid);
        }

        /// <summary>
        /// 新增/修改系統參數資料
        /// </summary>
        /// <returns></returns>
        [HttpPatch("")]
        public ActionResult<dynamic>? UpsertSysConfig([FromBody] Core.Dtos.UpsertSysConfig dto)
        {
            using (var scope = new System.Transactions.TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (string.IsNullOrEmpty(dto.Uid))
                {
                    var res = SysConfigHelper.AddOneData(_dbContext, dto, _authHelper.LoginAdmin);
                    if (res != null)
                    {
                        scope.Complete();
                    }
                    return res;
                }
                else
                {
                    var SysConfigBefore = _dbContext.SysConfigs.AsNoTracking().Where(r => r.DeletedAt == null && r.Uid == dto.Uid).FirstOrDefault();

                    var res = SysConfigHelper.UpdateOneData(_dbContext, dto, _authHelper.LoginAdmin);

                    if (SysConfigBefore != null)
                    {
                        var SysConfigAfter = _dbContext.SysConfigs.AsNoTracking().Where(r => r.DeletedAt == null && r.Uid == dto.Uid).FirstOrDefault();
                        LogHelper.InsertSysLog("SysConfig", SysConfigBefore.Uid, SysConfigBefore, SysConfigAfter, null, null, _authHelper.LoginAdmin);
                    }

                    if (res != null)
                    {
                        scope.Complete();
                    }
                    return res;
                }
            }
        }
    }
}