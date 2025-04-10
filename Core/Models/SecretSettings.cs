
namespace Core.Models
{
    public class SecretSettings
    {
        public SecretsConfig Secrets { get; set; }
        public EmailSender.SenderInfo SenderInfo { get; set; }
        public EmailSender.ServerInfo AWS_SES { get; set; }

        public class SecretsConfig
        {
            public ConnectionStrings? ConnectionStrings { get; set; }
            public string CookieKey { get; set; }
            public string CookieIv { get; set; }
        }

        public class ConnectionStrings
        {
            public string? DefaultConnectionString { get; set; }
        }
    }
}