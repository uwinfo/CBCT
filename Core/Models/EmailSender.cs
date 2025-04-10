
namespace Core.Models
{
    public class EmailSender
    {
        public class SenderInfo
        {
            public string Email { get; set; } = null!;
            public string CCEmails { get; set; } = null!;
            public string BCCEmails { get; set; } = null!;
            public string Name { get; set; } = null!;
        }
        public class ServerInfo
        {
            public string Host { get; set; } = null!;
            public int Port { get; set; } 
            public string Password { get; set; } = null!;
            public string Username { get; set; } = null!;
        }
    }
}