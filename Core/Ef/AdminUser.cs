using System;
using System.Collections.Generic;

namespace Core.Ef;

public partial class AdminUser
{
    public string Uid { get; set; } = null!;

    public long Id { get; set; }

    /// <summary>
    /// 管理員狀態: 正常=100, 鎖定=-100
    /// </summary>
    public int EnStatus { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Mobile { get; set; }

    public string SecretHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string? OtpSecret { get; set; }

    public string? OtpConfirm { get; set; }

    public string? BackMemo { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiration { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public DateTime ModifiedAt { get; set; }

    public string ModifierUid { get; set; } = null!;

    public DateTime? DeletedAt { get; set; }
}
