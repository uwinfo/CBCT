using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public class EmailHelper
    {
        public static string AmazonSesSettings = "email-smtp.us-west-2.amazonaws.com;AKIAWWF7Z7VSVEXBAQ6P;BAegtt9rmKomwca5T6nFW5kQcwQ1o5bbYGNZ5kQQiU28";  // 緯中的 SES，測試專用，不可用來發送電子報

        public static bool SendMailWithAmazonSES(string toMails, string fromMail, string subject, string body, string cc = "", string bcc = "",
            bool isBodyHtml = false, System.Text.Encoding bodyEncoding = null, System.Net.Mail.MailPriority priority = System.Net.Mail.MailPriority.Normal,
            string pathFileName = "", ArrayList alPathFileName = null, string returnReceipt = "", Hashtable newAttachmentFilenames = null,
            int emailTypeId = 1, bool isThrowException = false, bool isSkipLog = false)
        {
            string[] SESParams = AmazonSesSettings.Trim().Split(';'); //DB.SysConfig.GetSysConfig("AmazonSES").Trim().Split(';');
            // Amazon SES SMTP host name. This example uses the US West (Oregon) region.
            //const String HOST = "email-smtp.us-west-1.amazonaws.com";
            String HOST = SESParams[0];

            // Supply your SMTP credentials below. Note that your SMTP credentials are different from your AWS credentials.
            //const String SMTP_USERNAME = "AKIAJTQ35NEV2UJLVE3A";  // Replace with your SMTP username. 
            //const String SMTP_PASSWORD = "AuT2yoddKIrARlVwbh9h5d7j0jw/R2r2Hw9oVz07gbEM";  // Replace with your SMTP password.
            String SMTP_USERNAME = SESParams[1];
            String SMTP_PASSWORD = SESParams[2];

            // The port you will connect to on the Amazon SES SMTP endpoint. We are choosing port 587 because we will use
            // STARTTLS to encrypt the connection.
            const int PORT = 587;

            bool res = true;

            // Create an SMTP client with the specified host name and port.
            using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(HOST, PORT))
            {
                // Create a network credential with your SMTP user name and password.
                client.Credentials = new System.Net.NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

                // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
                // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
                client.EnableSsl = true;

                SendMail(toMails, fromMail, subject, body, cc, isBodyHtml, bodyEncoding, client, bcc, priority, pathFileName, alPathFileName, returnReceipt, newAttachmentFilenames);
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ToMails"></param>
        /// <param name="FromMail"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="CC"></param>
        /// <param name="IsBodyHtml"></param>
        /// <param name="BodyEncoding">預設是 utf8</param>
        /// <param name="Smtp"></param>
        /// <param name="BCC"></param>
        /// <param name="Priority"></param>
        /// <param name="PathFileName"></param>
        /// <param name="alPathFileName"></param>
        /// <param name="ReturnReceipt"></param>
        /// <param name="htNewAttachmentFilename"></param>
        /// <remarks></remarks>
        public static void SendMail(string ToMails, string FromMail, string Subject, string Body,
            string CC = "", bool IsBodyHtml = false, System.Text.Encoding BodyEncoding = null,
            System.Net.Mail.SmtpClient Smtp = null, string BCC = "",
            System.Net.Mail.MailPriority Priority = System.Net.Mail.MailPriority.Normal,
            string PathFileName = "", ArrayList alPathFileName = null, string ReturnReceipt = "",
            Hashtable htNewAttachmentFilename = null)
        {
            ToMails = ToMails.Replace("\r", "").Replace("\n", "");

            System.Net.Mail.MailMessage M = new System.Net.Mail.MailMessage();

            M.IsBodyHtml = IsBodyHtml;

            M.From = new MailAddress(FromMail);

            AddMailAddress(M, ToMails, CC, BCC);

            M.Subject = Subject.Replace("\r\n", ""); // 主旨有換行的動作，會有error, Reiko, 2011/07/07

            if (BodyEncoding == null)
                M.SubjectEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
            else
                M.SubjectEncoding = BodyEncoding;

            M.Body = Body;

            M.Priority = Priority;

            if (ReturnReceipt.Length > 0)
                M.Headers.Add("Disposition-Notification-To", ReturnReceipt);


            if (PathFileName.Length > 0)
                M.Attachments.Add(new Attachment(PathFileName));

            if (alPathFileName != null)
            {
                foreach (string File in alPathFileName)
                {
                    Attachment oA = new Attachment(File);
                    if (htNewAttachmentFilename != null && htNewAttachmentFilename[File] != null)
                    {
                        oA.Name = (string)htNewAttachmentFilename[File];

                        if (BodyEncoding == null)
                            oA.NameEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
                        else
                            oA.NameEncoding = BodyEncoding;
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

        public static void AddMailAddress(System.Net.Mail.MailMessage M, string ToMails, string CC, string BCC)
        {
            string[] arrTo = ToMails.Replace(",", ";").Split(';');
            foreach (string addrTo in arrTo)
            {
                if (addrTo.Trim().Length > 0)
                {
                    try
                    {
                        M.To.Add(new MailAddress(addrTo.Trim()));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("AddMailAddress Error For '" + addrTo + "', EX: " + ex.ToString());
                    }
                }
            }

            if (CC.Length > 0)
            {
                string[] arrMails = CC.Replace(",", ";").Split(';');
                foreach (string addrTo in arrMails)
                {
                    if (addrTo.Trim().Length > 0)
                    {
                        try
                        {
                            M.CC.Add(new MailAddress(addrTo.Trim()));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("AddMailAddress Error For '" + addrTo + "', EX: " + ex.ToString());
                        }
                    }
                }
            }

            if (BCC.Length > 0)
            {
                string[] arrMails = BCC.Replace(",", ";").Split(';');
                foreach (string addrTo in arrMails)
                {
                    if (addrTo.Trim().Length > 0)
                    {
                        try
                        {
                            M.Bcc.Add(new MailAddress(addrTo.Trim()));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("AddMailAddress Error For '" + addrTo + "', EX: " + ex.ToString());
                        }
                    }
                }
            }
        }
    }
}
