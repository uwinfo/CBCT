namespace Core.Models
{
    public class WwwSettings
    {
        public CommonClass Common { get; set; }

        public class CommonClass
        {
            /// <summary>
            /// 掛載 upload 用
            /// </summary>
            public string UploadDirectory { get; set; }
            public string DataDirectory { get; set; }

            /// <summary>
            /// Log 檔案存放位置
            /// </summary>
            public string? LogDirectory { get; set; }

            //public string FrontEndApiUrl { get; set; }
            //public string FrontEndWebUrl { get; set; }
            public string SystemName { get; set; }
            public string Host { get; set; }
        }
    }
}
