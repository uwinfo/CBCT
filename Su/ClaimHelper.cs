using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Su
{
    /// <summary>
    /// 前台使用
    /// </summary>
    public class ClaimHelper
    {
        private readonly ClaimsPrincipal _principal;
        public ClaimHelper(ClaimsPrincipal principal)
        {
            _principal = principal;
        }

        /// <summary>
        /// ClaimTypes.Sid 的內容
        /// </summary>
        public string? Uid
        {
            get
            {
                var claim = _principal.FindFirst(ClaimTypes.Sid);
                return claim?.GetValue("Value").ToString();
            }
        }

        public string? LineId
        {
            get
            {
                var claim = _principal.FindFirst("LineId");
                return claim?.GetValue("Value").ToString();
            }
        }

        public string? Name
        {
            get
            {
                return _principal.FindFirst(ClaimTypes.Name)?.GetValue("Value")?.ToString();
            }
        }

        public string? Email
        {
            get
            {
                return _principal.FindFirst(ClaimTypes.Email)?.GetValue("Value")?.ToString();
            }
        }

        public string? IsActivate
        {
            get
            {
                var claim = _principal.FindFirst("IsActivate");
                return claim?.GetValue("Value").ToString();
            }
        }

        public string? Phone
        {
            get
            {
                return _principal.FindFirst(ClaimTypes.MobilePhone)?.GetValue("Value")?.ToString();
            }
        }
    }


    ///// <summary>
    ///// company admin 用
    ///// </summary>
    //public class ClaimBaseHelper
    //{
    //    private readonly ClaimsPrincipal _principal;
    //    public ClaimBaseHelper(ClaimsPrincipal principal)
    //    {
    //        _principal = principal;
    //    }

    //    /// <summary>
    //    /// ClaimTypes.Sid 的內容
    //    /// </summary>
    //    public string? Uid
    //    {
    //        get
    //        {
    //            var claim = _principal.FindFirst(ClaimTypes.Sid);
    //            return claim?.GetValue("Value").ToString();
    //        }
    //    }
    //}
}
