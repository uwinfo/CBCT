namespace Core.Dtos
{
    /// <summary>
    /// 權限
    /// </summary>
    public class PermissionDto
    {
        public string? Uid { get; set; }

        /// <summary>
        /// 英文，權限
        /// </summary>
        [Su.ValidationAttributes.RequireValidation]
        public string? Code { get; set; }

        /// <summary>
        /// 中文，權限的描述
        /// </summary>
        [Su.ValidationAttributes.RequireValidation]
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Su.ValidationAttributes.RequireValidation]
        [Su.ValidationAttributes.RangeNumberValidation(lowerBound: 0)]
        public long? Sort { get; set; }
    }
}
