
namespace Core.Models
{
    public class LoginInfo
    {
        /// <summary>
        /// 登入時間
        /// </summary>
        public DateTime? LoginAt { get; set; }
        /// <summary>
        /// 登入者的 Uid
        /// </summary>
        public string? LoginUid { get; set; } 
        /// <summary>
        /// 後台管理者權限
        /// </summary>
        public string? PermissionCodes { get; set; }
    }
}