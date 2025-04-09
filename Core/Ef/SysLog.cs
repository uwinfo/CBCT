using System;
using System.Collections.Generic;

namespace Core.Ef;

/// <summary>
/// 更動歷程
/// </summary>
public partial class SysLog
{
    public string Uid { get; set; } = null!;

    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatorUid { get; set; } = null!;

    public string? CreatorName { get; set; }

    /// <summary>
    /// 資料表名稱
    /// </summary>
    public string TableName { get; set; } = null!;

    /// <summary>
    /// 被更動的資料Uid
    /// </summary>
    public string RecordUid { get; set; } = null!;

    /// <summary>
    /// 變更前內容
    /// </summary>
    public string? BeforeJson { get; set; }

    /// <summary>
    /// 變更後內容
    /// </summary>
    public string AfterJson { get; set; } = null!;

    /// <summary>
    /// 額外資料更動前
    /// </summary>
    public string? BeforeAdditionalJson { get; set; }

    /// <summary>
    /// 額外資料更動後
    /// </summary>
    public string? AfterAdditionalJson { get; set; }
}
