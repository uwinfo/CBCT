namespace Core.Dtos
{
    public class AdminRoleDto
    {
        public string? Uid { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        public string Name { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// 授權清單,逗號分隔
        /// </summary>
        public string? Permissions { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        
        public long Sort { get; set; }  
    }
}
