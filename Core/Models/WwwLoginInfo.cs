
namespace Core.Models
{
    /// <summary>
    /// 一般的登入資訊
    /// </summary>
    public class GeneralLoginInfo
    {
        /// <summary>
        /// 登入時間
        /// </summary>
        public DateTime LoginAt { get; set; }

        /// <summary>
        /// 登入者的 Uid
        /// </summary>
        public string LoginUid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginAt"></param>
        /// <param name="loginUid"></param>
        public GeneralLoginInfo(DateTime loginAt,string loginUid)
        {
            LoginAt = loginAt;
            LoginUid = loginUid;
        }
    }
}
