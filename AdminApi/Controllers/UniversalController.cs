using Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdminApi
{
    /// <summary>
    /// 取得通用資料(登入即可存取，需權限控管的資料請勿放在這裡)
    /// </summary>
    [Route("universal")]
    [AddPermission(Core.Constants.AdminPermission.Login)]
    public class UniversalController : BaseApiController
    {
        public UniversalController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonClass, IWebHostEnvironment env, 
            Core.Ef.CBCTContext CBCTContext, AuthHelper authHelper) : base(commonClass, env, CBCTContext, authHelper)
        {
        }


        ///// <summary>
        ///// 商品選項 (autocomplete使用)
        ///// </summary>
        ///// <param name="keyword">關鍵字. 檢查國際條碼/料號/商品名稱有符合的資料</param>
        ///// <param name="isVirtual">是否為虛擬商品(空值: 不篩選; Y: 虛擬商品; N:單品)</param>
        ///// <param name="top">抓前X幾筆資料. 預設為30</param>
        ///// <param name="minSalePrice"></param>
        ///// <returns></returns>
        //[HttpGet("products")]
        //public ActionResult<dynamic> GetProducts([FromQuery] string? keyword = "", string? isVirtual = "", int top = 30, int? minSalePrice = null)
        //{
        //    var list = ProductHelper.GetProducts(_dbContext, keyword, isVirtual, top, minSalePrice);
        //    return list;
        //}

        /// <summary>
        /// 取得所有權限清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("permissions")]
        public object GetPermissions()
        {
            return _dbContext.AdminPermissions.AsNoTracking().Where(x => x.DeletedAt == null)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Code)
                .ThenBy(x => x.Id);
        }

        /// <summary>
        /// 取得所有角色清單
        /// </summary>
        /// <returns></returns>
        [HttpGet("roles")]
        public object GetRoles()
        {
            return _dbContext.AdminRoles.AsNoTracking().Where(x => x.DeletedAt == null)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.Id);
        }

        ///// <summary>
        ///// 配送方式選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("delivery-method")]
        //public dynamic GetDeliveryMethod()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.DeliveryMethod);
        //}

        ///// <summary>
        ///// 配送狀態選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("delivery-status")]
        //public dynamic GetDeliveryStatus()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.DeliveryStatus);
        //}

        ///// <summary>
        ///// 付款方式選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("payment-status")]
        //public dynamic GetPaymentStatus()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.PaymentStatus);
        //}

        ///// <summary>
        ///// 付款類型選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("payment-type")]
        //public dynamic GetPaymentType()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.PaymentType);
        //}

        ///// <summary>
        ///// 訂單狀態選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("order-status")]
        //public dynamic GetOrderStatus()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.OrderStatus);
        //}

        ///// <summary>
        ///// 訂單來源選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("order-source")]
        //public dynamic GetOrderSource()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.OrderSource);
        //}

        ///// <summary>
        ///// 訂單明細類型
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("order-detail-type")]
        //public dynamic GetOrderDetailType()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.OrderDetailType);
        //}

        ///// <summary>
        ///// 經銷商付款金額類別
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("dealer-detail-check-out-type")]
        //public dynamic GetDealerChekOutType()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.DealerChekOutType);
        //}

        ///// <summary>
        ///// 發票類別選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("invoice-type")]
        //public dynamic GetInvoiceType()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.InvoiceType);
        //}

        ///// <summary>
        ///// 課稅別選項
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("tax-type")]
        //public dynamic GetTaxType()
        //{
        //    return ParameterHelper.OptionList(_dbContext, Core.Constants.Parameter.TaxType);
        //}

        ///// <summary>
        ///// 公司選項
        ///// </summary>
        ///// <param name="keyword">公司名稱關鍵字/統編</param>
        ///// <param name="top">抓前X幾筆資料. 預設為30</param>
        ///// <returns></returns>
        //[HttpGet("company")]
        //public dynamic GetCompany(string keyword = "", int top = 30)
        //{
        //    var option = _dbContext.Companies.AsNoTracking().Where(r => r.DeletedAt == null && (r.Name.Contains(keyword) || r.TaxId.Contains(keyword)))
        //        .OrderByDescending(x => x.Id)
        //        .Select(r => new { Value = r.Uid, Text = r.TaxId + " " + r.Name })
        //        .Take(top);
        //    return option;
        //}



        [Route("send-mail")]
        [HttpPost]
        public dynamic? SendMailAsync(string toMail, string subject, string mailBody)
        {
            Core.Helpers.EmailHelper.SendMailWithAmazonSES(toMail, subject, mailBody);
            return Ok();
        }
    }
}