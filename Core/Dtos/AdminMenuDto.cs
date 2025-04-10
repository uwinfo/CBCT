namespace Core.Dtos
{
    public class AdminMenuDto
    {
        /// <summary>
        /// 新增時不用填
        /// </summary>
        public string? Uid { get; set; }

        [Su.ValidationAttributes.RequireValidation]
        public string? Name { get; set; }

        public string? Link { get; set; }

        [Su.ValidationAttributes.RequireValidation]
        public long Sort { get; set; }

        /// <summary>
        /// 第一層目錄，Parent應為 # 
        /// </summary>
        [Su.ValidationAttributes.RequireValidation]
        public string? ParentUid { get; set; }

        /// <summary>
        /// 授權
        /// </summary>
        public string? PermissionUid { get; set; }
        public string? PermissionDesc { get; set; }
    }
}
