using static Core.Models.AdminAppSettings.CommonClass;

namespace Core.Models
{
    public class AdminAppSettings
    {
        public CommonClass Common { get; set; }

        public class CommonClass
        {
            public string UploadDirectory { get; set; }
            public string DataDirectory { get; set; }
            public string? LogDirectory { get; set; }
            //public string SystemCode { get; set; }
            public string SystemName { get; set; }
            public string Host { get; set; }
        }
    }
}