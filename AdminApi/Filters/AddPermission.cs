using Core.Constants;

namespace AdminApi
{
    /// <summary>
    /// 設定所需的權限(只要有任一權限即可使用)
    /// 權限類別: CompanyFeAuthCode
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AddPermission : Core.Filters.AddPermissionWithTypeFilter<AdminPermission>
    {
        /// <summary>
        /// 設定多個權限
        /// </summary>
        /// <param name="authCodes"></param>
        public AddPermission(Core.Constants.AdminPermission[] authCodes) : base(authCodes)
        {
        }

        /// <summary>
        /// 設定單一權限
        /// </summary>
        /// <param name="authCode"></param>
        public AddPermission(Core.Constants.AdminPermission authCode) : base(authCode)
        {
        }
    }
}