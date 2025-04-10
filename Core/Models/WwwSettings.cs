namespace Core.Models
{
    public class WwwSettings
    {
        public SecretsClass Secrets { get; set; }
        public EmailSender.SenderInfo SenderInfo { get; set; }
        public EmailSender.ServerInfo AWS_SES { get; set; }
        public CommonClass Common { get; set; }

        public class SecretsClass
        {
            public ConnectionStrings? ConnectionStrings { get; set; }

            /// <summary>
            /// 加密 Cookie 用的 Key, 暫時先放在 appsettings 裡，未來再移到安全的地方
            /// </summary>
            public string CookieKey { get; set; } = null!;

            /// <summary>
            /// 加密 Cookie 用的 Iv, 暫時先放在 appsettings 裡，未來再移到安全的地方
            /// </summary>
            public string CookieIv { get; set; } = null!;

            //public string CookieName { get; set; }
        }

        public class ConnectionStrings
        {
            public string? DefaultConnectionString { get; set; }
        }

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
