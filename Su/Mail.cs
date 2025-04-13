using System.Net.Mail;
using System.Collections;

namespace Su
{
    public class Mail
    {
        //public enum EmailType
        //{
        //    註冊完成通知信 = 6,
        //    忘記密碼通知信 = 7,
        //    VIP即將失效通知信 = 8,
        //    VVIP即將失效通知信 = 9,
        //    即將升級為VIP通知信 = 10,
        //    即將升級為VVIP通知信 = 11,
        //    購物車未結帳通知信 = 12,
        //    成為VIP通知信 = 13,
        //    成為VVIP通知信 = 14,
        //    瀏覽商品通知信 = 15,
        //    點數即將失效通知信 = 16,
        //    系統通知 = 17,
        //    廠商合作 = 18,
        //    嘗試入侵通知信 = 19,
        //    無法發送註冊通知信 = 20,
        //    訂單完成通知信 = 21,
        //    密碼修改通知信 = 22
        //}

        ///// <summary>
        ///// 三個欄位，HOST;SMTP_USERNAME;SMTP_PASSWORD
        ///// </summary>
        //public static string AmazonSesSettings;
        ///// <summary>
        ///// 預設寄件者
        ///// </summary>
        //public static string DefaultMailFrom;
        ///// <summary>
        ///// 發信記錄的資料庫
        ///// </summary>
        //public static Su.Sql.DbId StatisticId = null;

        //public static bool SendMailWithAmazonSES(string toMails, string fromMail, string subject, string body, string cc = "", string bcc = "",
        //    bool isBodyHtml = false, System.Text.Encoding bodyEncoding = null, System.Net.Mail.MailPriority priority = System.Net.Mail.MailPriority.Normal,
        //    string pathFileName = "", ArrayList alPathFileName = null, string returnReceipt = "", Hashtable newAttachmentFilenames = null,
        //    int emailTypeId = 1, bool isThrowException = false, bool isSkipLog = false)
        //{
        //    string[] SESParams = AmazonSesSettings.Trim().Split(';'); //DB.SysConfig.GetSysConfig("AmazonSES").Trim().Split(';');
        //    // Amazon SES SMTP host name. This example uses the US West (Oregon) region.
        //    //const String HOST = "email-smtp.us-west-1.amazonaws.com";
        //    String HOST = SESParams[0];

        //    // Supply your SMTP credentials below. Note that your SMTP credentials are different from your AWS credentials.
        //    //const String SMTP_USERNAME = "AKIAJTQ35NEV2UJLVE3A";  // Replace with your SMTP username. 
        //    //const String SMTP_PASSWORD = "AuT2yoddKIrARlVwbh9h5d7j0jw/R2r2Hw9oVz07gbEM";  // Replace with your SMTP password.
        //    String SMTP_USERNAME = SESParams[1];
        //    String SMTP_PASSWORD = SESParams[2];

        //    // The port you will connect to on the Amazon SES SMTP endpoint. We are choosing port 587 because we will use
        //    // STARTTLS to encrypt the connection.
        //    const int PORT = 587;

        //    bool res = true;

        //    // Create an SMTP client with the specified host name and port.
        //    using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(HOST, PORT))
        //    {
        //        // Create a network credential with your SMTP user name and password.
        //        client.Credentials = new System.Net.NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);

        //        // Use SSL when accessing Amazon SES. The SMTP session will begin on an unencrypted connection, and then 
        //        // the client will issue a STARTTLS command to upgrade to an encrypted connection using SSL.
        //        client.EnableSsl = true;

        //        if (!isSkipLog && StatisticId != null)
        //        {
        //            //ToMail may be mtiple
        //            string[] arrToMails = toMails.Split(';');
        //            var insertSQL = "";
        //            foreach (string tomail in arrToMails)
        //            {
        //                if (tomail.Length > 0 && tomail.IndexOf("@") != -1)
        //                {
        //                    //InsertSQL += " Insert into S3_Statistic..SESLog (EmailTypeId, Title, Receiver, Sender) values(" + EmailTypeId + ", '" + Subject.SqlStr() + "', '" + tomail.SqlStr() + "','" + FromMail.SqlStr() + "') ; ";
        //                    insertSQL += " Insert into S3_Statistic..SESLog (EmailTypeId, Title, Receiver, Sender) values({EmailTypeId}, {Title}, {Receiver},{Sender}) ; "
        //                        .ToMsSql(new { emailTypeId, Title = subject, Receiver = tomail, Sender = fromMail });
        //                }
        //            }

        //            Su.MsSql.ExecuteSql(insertSQL, StatisticId);
        //        }

        //        Su.Mail.SendMail(toMails, fromMail, subject, body, cc, isBodyHtml, bodyEncoding, client, bcc, priority, pathFileName, alPathFileName, returnReceipt, newAttachmentFilenames);
        //    }

        //    return res;
        //}



        /// <summary>
        /// 使用 Gmail 時, 請設定為 "smtp.gmail.com"
        /// </summary>
        public static string SMTP_SERVER_NAME = ""; // This Should Be Set in Global.asax

        public static bool IsSendWithGmail = false;
        public static bool IsSSL = false;
        public static string Username = "";
        public static string Password = "";

        public enum ENCODING
        {
            UTF8 = 100,
            BIG5 = 200
        }

        public static Mail.ENCODING MAIL_ENCODING = ENCODING.UTF8; // This Should Be Set in Global.asax

        private static System.Net.Mail.SmtpClient _SmtpClient = null;

        /// <summary>
        ///     ''' 不要再使用這個物件了, 在 .NET 4.0 容易造成錯誤 !! 請改指定 SMTP_SERVER_NAME
        ///     ''' </summary>
        ///     ''' <value></value>
        ///     ''' <returns></returns>
        ///     ''' <remarks></remarks>
        public static System.Net.Mail.SmtpClient SmtpClient
        {
            get
            {
                if (_SmtpClient == null)
                {
                    if (IsSendWithGmail)
                    {
                        _SmtpClient = new System.Net.Mail.SmtpClient();
                        // 登入帳號認證  
                        _SmtpClient.Credentials = new System.Net.NetworkCredential(Username, Password);
                        // 使用587 Port 
                        _SmtpClient.Port = 587;
                        _SmtpClient.Host = "smtp.gmail.com";
                        // 啟動SSL 
                        _SmtpClient.EnableSsl = true;
                    }
                    else if (IsSSL)
                    {
                        SmtpClient = new System.Net.Mail.SmtpClient();
                        // 登入帳號認證  
                        _SmtpClient.Credentials = new System.Net.NetworkCredential(Username, Password);
                        // 使用587 Port 
                        _SmtpClient.Port = 587;
                        _SmtpClient.Host = SMTP_SERVER_NAME;
                        // 啟動SSL 
                        _SmtpClient.EnableSsl = true;
                    }
                    else
                        _SmtpClient = new System.Net.Mail.SmtpClient(SMTP_SERVER_NAME);
                }
                return _SmtpClient;
            }
            set
            {
                _SmtpClient = value;
            }
        }// This Should Be Set in Global.asax.vb

        public static System.Net.Mail.SmtpClient NewSmtpClient()
        {
            if (IsSendWithGmail)
            {
                System.Net.Mail.SmtpClient NSC = new System.Net.Mail.SmtpClient();

                NSC = new System.Net.Mail.SmtpClient();
                // 登入帳號認證  
                NSC.Credentials = new System.Net.NetworkCredential(Username, Password);
                // 使用587 Port 
                NSC.Port = 587;
                NSC.Host = "smtp.gmail.com";
                // 啟動SSL 
                NSC.EnableSsl = true;

                return NSC;
            }
            else if (SMTP_SERVER_NAME != null && SMTP_SERVER_NAME.Length > 0)
                // 不要用同一個物件, 在 .NET 4.0 會有 Connection Pool, 若是前一個封信還沒寄完, 會有 Exception.
                return new System.Net.Mail.SmtpClient(SMTP_SERVER_NAME);
            else
                // 和舊版相容
                return SmtpClient;
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
                M.BodyEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
            else
                M.BodyEncoding = BodyEncoding;

            // If Smtp Is Nothing Then
            // Dim NSC As System.Net.Mail.SmtpClient = NewSmtpClient()
            // NSC.Send(M)
            // Else
            // Smtp.Send(M)
            // End If

            if (Smtp == null)
            {
                System.Net.Mail.SmtpClient smtpClient = null;
                if (IsSendWithGmail)
                {
                    smtpClient = new System.Net.Mail.SmtpClient();
                    // 登入帳號認證  
                    smtpClient.Credentials = new System.Net.NetworkCredential(Username, Password);
                    // 使用587 Port 
                    smtpClient.Port = 587;
                    smtpClient.Host = "smtp.gmail.com";
                    // 啟動SSL 
                    smtpClient.EnableSsl = true;
                }
                else
                {
                    smtpClient = new System.Net.Mail.SmtpClient(SMTP_SERVER_NAME);
                }

                smtpClient.Send(M);
                M.Dispose();
                smtpClient.Dispose();
            }
            else
            {
                Smtp.Send(M);
                M.Dispose();
            }
        }

        public static void SendMailWithAutoEncodingOld(string ToMails, string FromMail,
            string Subject, string Body, string CC = "", bool IsHTML = false,
            System.Net.Mail.SmtpClient Smtp = null, string BCC = "", string SMTP_DNS = "",
            string AttachFileName = "", System.Text.Encoding BodyEncoding = null)
        {

            // 這裡的寫法有點怪怪的，傳入的 smtp 好像一定會是 nothing :P
            // 因為 System.Web.Mail.SmtpMail 沒有 New 的 function .

            // Body 應該是 utf-8

            // Dim M As New System.Web.Mail.MailMessage
            System.Net.Mail.MailMessage M = new System.Net.Mail.MailMessage();
            M.From = new System.Net.Mail.MailAddress(FromMail);

            AddMailAddress(M, ToMails, CC, BCC);

            // M.To = ToMails
            // M.Cc = CC
            // M.Bcc = BCC

            M.Subject = Subject;

            M.IsBodyHtml = IsHTML;
            // M.BodyFormat = BodyFormat

            // 依 To 來決定 Encoding 的方式
            // OrElse ToMails.ToLower.IndexOf("hotmail") > 0


            if (BodyEncoding == null)
            {
                switch (Mail.MAIL_ENCODING)
                {
                    case ENCODING.BIG5:
                        {
                            M.BodyEncoding = System.Text.Encoding.GetEncoding("big5");
                            M.Body = Body.Replace("肽", "&#32957;"); // 特別處理
                            break;
                        }

                    case ENCODING.UTF8:
                        {
                            M.BodyEncoding = System.Text.Encoding.UTF8;
                            M.Body = Body;
                            break;
                        }

                    default:
                        {
                            if (ToMails.ToLower().IndexOf("gmail") > 0)
                            {
                                M.BodyEncoding = System.Text.Encoding.UTF8;
                                M.Body = Body;
                            }
                            else
                            {
                                M.BodyEncoding = System.Text.Encoding.GetEncoding("big5");
                                M.Body = Body.Replace("肽", "&#32957;"); // 特別處理
                            }

                            break;
                        }
                }

                // 改成用預設值控制, 沒設時用big5  2009/2/10 Karen
                // 'gmail 為 utf8 編碼
                // If ToMails.ToLower.IndexOf("gmail") > 0 Then
                // M.BodyEncoding = System.Text.Encoding.UTF8
                // M.Body = Body
                // Else
                // M.BodyEncoding = System.Text.Encoding.GetEncoding("big5")
                // M.Body = Body.Replace("肽", "&#32957;") '特別處理
                // End If

                if (AttachFileName.Length > 0)
                    M.Attachments.Add(new System.Net.Mail.Attachment(AttachFileName));
            }
            else
            {
                M.BodyEncoding = BodyEncoding;
                M.Body = Body;
            }

            // M.BodyEncoding = System.Text.Encoding.UTF8
            // M.Body = Body

            SmtpClient SC = Smtp;

            if (SMTP_DNS.Length > 0)
                // System.Web.Mail.SmtpMail.SmtpServer = SMTP_DNS
                SC = new SmtpClient(SMTP_DNS);
            else if (SC == null)
                SC = new SmtpClient(SMTP_SERVER_NAME);

            // Smtp.SmtpServer = SMTP_DNS

            // UW.WU.DebugWriteLine("SendMailWithAutoEncoding: " & M.Body)

            try
            {
                // System.Web.Mail.SmtpMail.Send(M)
                SC.Send(M);
                M.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + ", SMTP: " + SC.Host);
            }
        }

        public static void SendMailWithAutoEncoding(string ToMails, string FromMail, string Subject, string Body, string CC = "", bool IsBodyHtml = false, System.Net.Mail.SmtpClient Smtp = null, string BCC = "")
        {

            // Body 應該是 utf-8

            System.Net.Mail.MailMessage M = new System.Net.Mail.MailMessage();
            M.From = new MailAddress(FromMail);

            AddMailAddress(M, ToMails, CC, BCC);

            M.Subject = Subject;
            M.IsBodyHtml = IsBodyHtml;
            M.Body = Body;

            switch (Mail.MAIL_ENCODING)
            {
                case ENCODING.BIG5:
                    {
                        M.BodyEncoding = System.Text.Encoding.GetEncoding("big5");
                        break;
                    }
                default:
                    {
                        M.BodyEncoding = System.Text.Encoding.UTF8;
                        break;
                    }
            }


            if (Smtp == null)
                SmtpClient.Send(M);
            else
                Smtp.Send(M);
            M.Dispose();
        }

        /// <summary>
        /// 同 SendMailWithAutoEncoding
        /// </summary>
        /// <param name="ToMails"></param>
        /// <param name="FromMail"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="CC"></param>
        /// <param name="SMTP"></param>
        /// <param name="BCC"></param>
        public static void SendMailWithAutoEncodingWithCDOMSGForHTML(string ToMails, string FromMail, string Subject, string Body, string CC = "", string SMTP = null, string BCC = "")
        {

            // CDOMSG 因為 Exchange 2003 有問題，先改回 SendMailWithAutoEncoding
            SendMailWithAutoEncoding(ToMails, FromMail, Subject, Body, CC, true, null/* Conversion error: Set to default value for this argument */, BCC);

            return;
        }



        public static void SendMail_TLS(string Subject, string Body, string FromMail, string ToMail, string SendMail_Username = null, string SendMail_Password = null, string SendMail_Port = "587", string CCMails = "", string BccMails = "", System.Net.Mail.MailPriority Priority = MailPriority.Normal, string PathFileName = "", ArrayList alPathFileName = null/* TODO Change to default(_) if this is not a reference type */, Hashtable htNewAttachmentFilename = null/* TODO Change to default(_) if this is not a reference type */, System.Text.Encoding BodyEncoding = null)
        {
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            try
            {
                msg.Subject = Subject.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
                if (BodyEncoding == null)
                    msg.SubjectEncoding = System.Text.Encoding.UTF8; // 預設用 UTF8
                else
                    msg.SubjectEncoding = BodyEncoding;
                msg.Body = Body;
                msg.IsBodyHtml = true;

                if (PathFileName.Length > 0)
                    msg.Attachments.Add(new Attachment(PathFileName));

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
                        msg.Attachments.Add(oA);
                    }
                }

                msg.From = new System.Net.Mail.MailAddress(FromMail);
                foreach (string mail in ToMail.Split(';'))
                    msg.To.Add(new MailAddress(mail));
                msg.Priority = Priority;

                if (CCMails.Length > 0)
                {
                    foreach (string mail in CCMails.Split(';'))
                        msg.CC.Add(new MailAddress(mail));
                }

                if (BccMails.Length > 0)
                {
                    foreach (string mail in BccMails.Split(';'))
                        msg.Bcc.Add(new MailAddress(mail));
                }


                client.Host = SMTP_SERVER_NAME;
                //if (SMTP_SERVER_NAME.Length > 0 && SMTP_SERVER_NAME.Contains("gmail.com"))
                //    client.Host = SMTP_SERVER_NAME;
                //else
                //    client.Host = "smtp.gmail.com";

                client.Port = Int32.Parse(SendMail_Port);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                SendMail_Username ??= Username;
                SendMail_Password ??= Password;
                System.Net.NetworkCredential basicauthenticationinfo = new System.Net.NetworkCredential(SendMail_Username, SendMail_Password);
                client.Credentials = basicauthenticationinfo;

                client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                client.Send(msg);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
