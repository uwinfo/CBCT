using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Constants
{
    /// <summary>
    /// 權限定義
    /// </summary>
    public enum AdminPermission
    {
        /// <summary>
        /// 不限，含未入者
        /// </summary>
        [Description("不限，含未入者")]
        UnLimited,

        /// <summary>
        /// 登入者
        /// </summary>
        [Description("登入者")]
        Login,

        /// <summary>
        /// 系統管理(最高權限)
        /// </summary>
        [Description("系統管理(最高權限)")]
        Admin,

        /// <summary>
        /// 後台目錄管理
        /// </summary>
        [Description("後台目錄管理")]
        MenuManagement,

        /// <summary>
        /// 後台管理員管理
        /// </summary>
        [Description("後台管理員管理")]
        UserManagement,

        ///// <summary>
        ///// 會員管理
        ///// </summary>
        //[Description("會員管理")]
        //Member,

        ///// <summary>
        ///// 會員帳號查詢
        ///// </summary>
        //[Description("會員帳號查詢")]
        //MemberQuery,

        ///// <summary>
        ///// 會員優惠券管理
        ///// </summary>
        //[Description("會員優惠券管理")]
        //MemberCoupon,

        ///// <summary>
        ///// 訂單管理
        ///// </summary>
        //[Description("訂單管理")]
        //Order,

        ///// <summary>
        ///// 商品管理
        ///// </summary>
        //[Description("商品管理")]
        //Product,
    }
}
