using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos
{
    public class LogInDto
    {
        public string Email { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }

    public class AdminUserDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Uid { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [Su.ValidationAttributes.EmailValidation]
        public string? Email { get; set; }

        /// <summary>
        /// Secret
        /// </summary>
        [Su.ValidationAttributes.SecretValidation(isUpperAndLower: true)]
        public string? Secret { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Su.ValidationAttributes.RequireValidation]
        public string? Name { get; set; }

        public string? Mobile { get; set; }

        public string? BackMemo { get; set; }

        /// <summary>
        /// 管理員狀態: 正常=100, 鎖定=-100
        /// </summary>
        public int? EnStatus { get; set; }

        /// <summary>
        /// 所屬群組
        /// </summary>
        public List<string> AdminRoleUids { get; set; }
    }

    public class ForgotSecretDto
    {
        [Su.ValidationAttributes.RequireValidation]
        public string Email { get; set; } = null!;
    }

    public class ResetSecretDto
    {
        public string Uid { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Secret { get; set; } = null!;
        public string CheckSecret { get; set; } = null!;
        public string ResetToken { get; set; } = null!;
    }
}
