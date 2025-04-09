namespace Core.Dtos
{
    public class MenuForClient
    {
        public string Id { get; set; }

        public string? Text { get; set; }

        public string? Link { get; set; }

        public long Sort { get; set; }

        public string? Parent { get; set; }

        /// <summary>
        /// Menu關聯的權限
        /// </summary>
        public List<string>? Permissions { get; set; }
    }
}
