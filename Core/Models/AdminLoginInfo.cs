
namespace Core.Models
{
    /// <summary>
    /// 登入資訊
    /// </summary>
    public class AdminLoginInfo
    {
        public DateTime LoginAt { get; set; }
        public string AdminUserUid { get; set; }
        public string AdminPermissionCodes { get; set; }
    }
}
