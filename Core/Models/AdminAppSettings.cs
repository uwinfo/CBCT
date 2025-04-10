using static Core.Models.AdminAppSettings.CommonClass;

namespace Core.Models
{
    public class AdminAppSettings
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
        }

        public class ConnectionStrings
        {
            public string? DefaultConnectionString { get; set; }
        }

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