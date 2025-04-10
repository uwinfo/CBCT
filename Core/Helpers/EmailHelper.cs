using System.Collections;

namespace Core.Helpers
{
    public class EmailHelper
    {
        public static Core.Models.EmailSender.SenderInfo _senderInfo;
        public static Core.Models.EmailSender.ServerInfo _serverInfo;

        public static bool SendMailWithAmazonSES(string toMails, string subject, string body, 
            bool isBodyHtml = false, System.Text.Encoding? bodyEncoding = null, System.Net.Mail.MailPriority priority = System.Net.Mail.MailPriority.Normal,
            string pathFileName = "", ArrayList? alPathFileName = null, string returnReceipt = "", Hashtable? newAttachmentFilenames = null,
            int emailTypeId = 1, bool isThrowException = false, bool isSkipLog = false,
            bool CCSender = false, bool BCCSender = false)
        {
            bool res = true;

            using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(_serverInfo.Host, _serverInfo.Port))
            {
                client.Credentials = new System.Net.NetworkCredential(_serverInfo.Username, _serverInfo.Password);

                client.EnableSsl = true;

                SendMail(toMails, subject, body, client, isBodyHtml, bodyEncoding, priority, pathFileName, alPathFileName, returnReceipt, newAttachmentFilenames, CCSender, BCCSender);
            }

            return res;
        }

        public static void SendMail(string ToMails, string Subject, string Body, System.Net.Mail.SmtpClient Smtp,
            bool IsBodyHtml = false, System.Text.Encoding? BodyEncoding = null,
            System.Net.Mail.MailPriority Priority = System.Net.Mail.MailPriority.Normal,
            string PathFileName = "", ArrayList? alPathFileName = null, string ReturnReceipt = "",
            Hashtable? htNewAttachmentFilename = null, bool CCSender = false, bool BCCSender = false)
        {
            ToMails = ToMails.Replace("\r", "").Replace("\n", "");

            System.Net.Mail.MailMessage M = new System.Net.Mail.MailMessage();

            M.IsBodyHtml = IsBodyHtml;

            M.From = new System.Net.Mail.MailAddress(_senderInfo.Email);

            AddMailAddress(M, ToMails, CCSender, BCCSender);

            M.Subject = Subject.Replace("\r\n", ""); // 主旨有換行的動作，會有error, Reiko, 2011/07/07

            if (BodyEncoding == null)
                M.SubjectEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
            else
                M.SubjectEncoding = BodyEncoding;

            M.Body = Body;

            M.Priority = Priority;

            if (ReturnReceipt.Length > 0) M.Headers.Add("Disposition-Notification-To", ReturnReceipt);

            if (PathFileName.Length > 0) M.Attachments.Add(new System.Net.Mail.Attachment(PathFileName));

            if (alPathFileName != null)
            {
                foreach (string File in alPathFileName)
                {
                    System.Net.Mail.Attachment oA = new System.Net.Mail.Attachment(File);
                    if (htNewAttachmentFilename != null && htNewAttachmentFilename[File] != null)
                    {
                        oA.Name = (string)htNewAttachmentFilename[File];

                        if (BodyEncoding == null)
                        {
                            oA.NameEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
                        }
                        else
                        {
                            oA.NameEncoding = BodyEncoding;
                        }
                    }
                    M.Attachments.Add(oA);
                }
            }

            if (BodyEncoding == null)
            {
                M.BodyEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
            }
            else
            {
                M.BodyEncoding = BodyEncoding;
            }

            Smtp.Send(M);
            M.Dispose();
        }

        public static void AddMailAddress(System.Net.Mail.MailMessage M, string ToMails, bool CCSender = false, bool BCCSender = false)
        {
            string[] arrTo = ToMails.Replace(",", ";").Split(';');
            foreach (string addrTo in arrTo)
            {
                if (addrTo.Trim().Length > 0)
                {
                    try
                    {
                        M.To.Add(new System.Net.Mail.MailAddress(addrTo.Trim()));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("toMails AddMailAddress Error For '" + addrTo + "', EX: " + ex.ToString());
                    }
                }
            }

            if (CCSender && _senderInfo.CCEmails.Trim() != string.Empty)
            {
                foreach (string mail in _senderInfo.CCEmails.Split(';'))
                {
                    if (mail.Trim().Length > 0)
                    {
                        try
                        {
                            M.CC.Add(new System.Net.Mail.MailAddress(mail.Trim()));

                        }
                        catch (Exception ex)
                        {
                            throw new Exception("CC AddMailAddress Error For '" + mail + "', EX: " + ex.ToString());
                        }
                    }
                }
            }

            if (BCCSender && _senderInfo.BCCEmails.Trim() != string.Empty)
            {
                foreach (string mail in _senderInfo.BCCEmails.Split(';'))
                {
                    if (mail.Trim().Length > 0)
                    {
                        try
                        {
                            M.Bcc.Add(new System.Net.Mail.MailAddress(mail.Trim()));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("BCC AddMailAddress Error For '" + mail + "', EX: " + ex.ToString());
                        }
                    }
                }
            }
        }
    }
}
