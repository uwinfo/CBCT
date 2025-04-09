using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    public class Ftp
    {
        public static string[] FileList(string sFTPServerIP, string sUserName, string sPassWord, string sDirName, int timeout = 60000)
        {
            sDirName = sDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
            FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                                                                                                   //設定連線模式及相關參數
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
            myFTP.Timeout = timeout; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
            myFTP.Method = System.Net.WebRequestMethods.Ftp.ListDirectory; //取得檔案清單

            StreamReader myReadStream = new StreamReader(myFTP.GetResponse().GetResponseStream(), Encoding.UTF8); //取得FTP請求回應

            //檔案清單
            string sFTPFile; StringBuilder sbResult = new StringBuilder(); //,string[] sDownloadFiles;
            while (!(myReadStream.EndOfStream))
            {
                sFTPFile = myReadStream.ReadLine();
                sbResult.Append(sFTPFile + "\n");
                //Console.WriteLine("{0}", FTPFile);
            }
            myReadStream.Close();
            myReadStream.Dispose();
            sFTPFile = null;

            if (sbResult.Length > 0)
            {
                sbResult.Remove(sbResult.ToString().LastIndexOf("\n"), 1); //檔案清單查詢結果
                                                                           //Console.WriteLine("Result:" + "\n" + "{0}", sResult);
            }

            return sbResult.ToString().Split('\n'); //回傳至字串陣列
        }

        public static DateTime GetFileDate(string sFTPServerIP, string sUserName, string sPassWord, string sDirName, string sFileName)
        {
            sDirName = sDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sDirName + "/" + sFileName;

            FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                                                                                                   //設定連線模式及相關參數
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
            myFTP.Timeout = 2000; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
            myFTP.Method = WebRequestMethods.Ftp.GetDateTimestamp; //取得資料修改日期

            System.Net.FtpWebResponse myFTPFileDate = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP請求回應
            return myFTPFileDate.LastModified;
        }

        public static Boolean CreateDir(string sFTPServerIP, string sUserName, string sPassWord, string sDirName)
        {
            sDirName = sDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sDirName;
            System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                                                                                                              //設定連線模式及相關參數
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                                                                                        //myFTP.KeepAlive = false; //關閉/保持 連線
                                                                                        //myFTP.Timeout = 2000; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
            myFTP.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory; //建立目錄模式

            System.Net.FtpWebResponse myFtpResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //創建目錄
            myFtpResponse.Close();
            return true;
        }

        public static void UploadFile(string sFTPServerIP, string sUserName, string sPassWord, System.IO.Stream sourceStream, string destFolder, string destFilename)
        {
            if (destFolder.Contains("..") || destFilename.Contains(".."))
            {
                throw new Exception("不可使用 .. 來指定檔案或目錄");
            }

            try
            {
                CreateDir(sFTPServerIP, sUserName, sPassWord, destFolder);
            }
            catch (Exception)
            {

            }

            destFolder = destFolder.Replace("\\", "/");
            string sURI = $"FTP://" + sFTPServerIP + "/" + destFolder + "/" + destFilename;


            System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                                                                                                                //myFTP.KeepAlive = false; //關閉/保持 連線
                                                                                                                //myFTP.Timeout = 2000; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                                    //myFTP.UsePassive = true; //通訊埠接聽並等待連接
            myFTP.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            System.IO.Stream myWriteStream = myFTP.GetRequestStream(); //資料串流設為上傳至FTP

            //上傳檔案
            byte[] bBuffer = new byte[2047]; int iRead = 0; //傳輸位元初始化
            do
            {
                iRead = sourceStream.Read(bBuffer, 0, bBuffer.Length); //讀取上傳檔案
                myWriteStream.Write(bBuffer, 0, iRead); //傳送資料串流
                                                        //Console.WriteLine("Buffer: {0} Byte", iRead);
            } while (!(iRead == 0));

            myWriteStream.Flush();
            myWriteStream.Close();
            myWriteStream.Dispose();
        }

        public static void UploadFile(string sFTPServerIP, string sUserName, string sPassWord, string sFromFileName,
            string sToDirName, string sToFileName)
        {
            //上傳檔案
            using FileStream myReadStream = new FileStream(sFromFileName, FileMode.Open, FileAccess.Read); //檔案設為讀取模式
            UploadFile(sFTPServerIP, sUserName, sPassWord, myReadStream, sToDirName, sToFileName);            
        }


        public static void DownloadFile(string sFTPServerIP, string sUserName, string sPassWord,
            string sFromDirName, string sFromFileName, string sToFileName)
        {
            sFromDirName = sFromDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sFromDirName + "/" + sFromFileName;
            System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                                                                                                              //設定連線模式及相關參數
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                                                                                        //myFTP.Timeout = 2000; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                                    //myFTP.UsePassive = false; //通訊埠接聽並等待連接
            myFTP.Method = System.Net.WebRequestMethods.Ftp.DownloadFile; //下傳檔案

            System.Net.FtpWebResponse myFTPResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP回應
                                                                                                      //下載檔案
            System.IO.FileStream myWriteStream = new System.IO.FileStream(sToFileName, FileMode.CreateNew, FileAccess.Write); //檔案設為寫入模式
            System.IO.Stream myReadStream = myFTPResponse.GetResponseStream(); //資料串流設為接收FTP回應下載
            byte[] bBuffer = new byte[2047]; int iRead = 0; //傳輸位元初始化
            do
            {
                iRead = myReadStream.Read(bBuffer, 0, bBuffer.Length); //接收資料串流
                myWriteStream.Write(bBuffer, 0, iRead); //寫入下載檔案
                                                        //Console.WriteLine("bBuffer: {0} Byte", iRead);
            } while (!(iRead == 0));

            myReadStream.Flush();
            myReadStream.Close();
            myReadStream.Dispose();
            myWriteStream.Flush();
            myWriteStream.Close();
            myWriteStream.Dispose();
            myFTPResponse.Close();
        }
        public static void DownloadFile(string sFTPServerIP, string sUserName, string sPassWord,
            string sFromDirName, string sFromFileName, string sToFileName, FileMode fileMode = FileMode.Create)
        {
            sFromDirName = sFromDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sFromDirName + "/" + sFromFileName;
            System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(sURI); //建立FTP連線
                                                                                                              //設定連線模式及相關參數
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                                                                                        //myFTP.Timeout = 2000; //等待時間
            myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                                    //myFTP.UsePassive = false; //通訊埠接聽並等待連接
            myFTP.Method = System.Net.WebRequestMethods.Ftp.DownloadFile; //下傳檔案

            System.Net.FtpWebResponse myFTPResponse = (System.Net.FtpWebResponse)myFTP.GetResponse(); //取得FTP回應
                                                                                                      //下載檔案
            System.IO.FileStream myWriteStream = new System.IO.FileStream(sToFileName, fileMode, FileAccess.Write); //檔案設為寫入模式
            System.IO.Stream myReadStream = myFTPResponse.GetResponseStream(); //資料串流設為接收FTP回應下載
            byte[] bBuffer = new byte[2047]; int iRead = 0; //傳輸位元初始化
            do
            {
                iRead = myReadStream.Read(bBuffer, 0, bBuffer.Length); //接收資料串流
                myWriteStream.Write(bBuffer, 0, iRead); //寫入下載檔案
                                                        //Console.WriteLine("bBuffer: {0} Byte", iRead);
            } while (!(iRead == 0));

            myReadStream.Flush();
            myReadStream.Close();
            myReadStream.Dispose();
            myWriteStream.Flush();
            myWriteStream.Close();
            myWriteStream.Dispose();
            myFTPResponse.Close();
        }
        /// <summary>
        /// 檢查FTP上的是否存在sToFileName的檔案
        /// </summary>
        /// <param name="sFTPServerIP"></param>
        /// <param name="sUserName"></param>
        /// <param name="sPassWord"></param>
        /// <param name="sToDirName">於FTP存放的路徑</param>
        /// <param name="sToFileName">於FTP的檔案名稱</param>
        /// <returns></returns>
        public static bool CheckFileExistsOnServer(string sFTPServerIP, string sUserName, string sPassWord,
            string sToDirName, string sToFileName)
        {
            sToDirName = sToDirName.Replace("\\", "/");
            string sURI = "FTP://" + sFTPServerIP + "/" + sToDirName + "/" + sToFileName;
            System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(sURI); //建立FTP連線
            myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
            myFTP.Method = WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse myWriteStream = (FtpWebResponse)myFTP.GetResponse();
                return true;

            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFTPServerIP"></param>
        /// <param name="sUserName"></param>
        /// <param name="sPassWord"></param>
        /// <param name="sToDirName"></param>
        /// <param name="sToFileName"></param>
        /// <param name="BackupFullFileName">這裡不會建立目錄, 所以呼叫前要確認目錄已建立</param>
        /// <returns></returns>
        public static string DeleteFile(string sFTPServerIP, string sUserName, string sPassWord,
            string sToDirName, string sToFileName, string backupFullFileName = null)
        {
            //Tool.AppendOneDayLog("DeleteFile", "sURI = " + sURI);

            //先檢查檔案是否存在
            if (CheckFileExistsOnServer(sFTPServerIP, sUserName, sPassWord, sToDirName, sToFileName))
            {
                if (!string.IsNullOrEmpty(backupFullFileName))
                {
                    DownloadFile(sFTPServerIP, sUserName, sPassWord, sToDirName, sToFileName, backupFullFileName);
                }

                sToDirName = sToDirName.Replace("\\", "/");
                string sURI = "FTP://" + sFTPServerIP + "/" + sToDirName + "/" + sToFileName;
                System.Net.FtpWebRequest myFTP = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(sURI); //建立FTP連線
                myFTP.Credentials = new System.Net.NetworkCredential(sUserName, sPassWord); //帳密驗證
                                                                                            //myFTP.KeepAlive = false; //關閉/保持 連線
                                                                                            //myFTP.Timeout = 2000; //等待時間
                                                                                            //myFTP.UseBinary = true; //傳輸資料型別 二進位/文字
                                                                                            //myFTP.UsePassive = true; //通訊埠接聽並等待連接
                myFTP.Method = System.Net.WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse myWriteStream = (FtpWebResponse)myFTP.GetResponse(); //資料串流設為上傳至FTP

                myWriteStream.Close();
                myWriteStream.Dispose();

                return "OK";
            }
            else
            {
                return "File Not Found.";
            }
        }

        public class PhotoServer
        {
            //static string ServerIP = "61.216.154.172";
            //static string ServerIP = "photoftp2.mtsc.shop";
            static string ServerIP = "innerphoto.s3.com.tw";
            static string Username = "photo-s3";
            //static string Password = "S31q2w#E$R";
            static string Password = "@iWx61a9SxP3$";

            //FTPExtensions.sFTPServerIP = "photo.s3.com.tw"; //59.124.112.241
            //FTPExtensions.sUserName = "S3Photo";
            //FTPExtensions.sPassWord = "S3Photo@wj6qu04";
            //FTPExtensions.sDirName = @"banner/block/";

            //        名稱:    photo.s3.com.tw
            //Addresses:  118.163.102.86
            //          118.163.67.54
            //          211.20.19.83
            //          118.163.131.233
            //          61.216.177.14
            //          59.124.112.241
            //          211.75.14.143
            //          220.128.218.162
            //          61.219.165.217
            //          61.216.66.11
            //          59.120.153.205


            ///// <summary>
            ///// 檔案暫存在 DB.SysConfig.Path.FTPTempFolder, 會由 ClearFiles.aspx 定期刪除
            ///// </summary>
            ///// <param name="postedFile"></param>
            ///// <param name="sToDirName"></param>
            ///// <param name="sToFileName"></param>
            //public static void UploadFile(System.Web.HttpPostedFile postedFile, string sToDirName, string sToFileName)
            //{
            //    string sFromFileName = DB.SysConfig.Path.FTPTempFolder + Guid.NewGuid().ToString() + "_" + postedFile.FileName;

            //    postedFile.SaveAs(sFromFileName);

            //    Ftp.UploadFile(ServerIP, Username, Password, sFromFileName, sToDirName, sToFileName);
            //}

            public static string[] GetFileList(string sDirName)
            {
                return Ftp.FileList(ServerIP, Username, Password, sDirName);
            }

            public string[] FileList(string sDirName)
            {
                return GetFileList(sDirName);
                //return FTP.FileList(ServerIP, Username, Password, sDirName);
            }

            public static DateTime GetFileDate(string sDirName, string sFileName)
            {
                return Ftp.GetFileDate(ServerIP, Username, Password, sDirName, sFileName);
            }

            public static void UploadFile(string sFromFileName, string sToDirName, string sToFileName)
            {
                Ftp.UploadFile(ServerIP, Username, Password, sFromFileName, sToDirName, sToFileName);
            }

            public static string DownloadFile(string sFromDirName, string sFromFileName, string sToFileName)
            {
                Ftp.DownloadFile(ServerIP, Username, Password, sFromDirName, sFromFileName, sToFileName);
                return sToFileName;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="sToDirName"></param>
            /// <param name="sToFileName"></param>
            /// <param name="BackupFullFileName">這裡不會建立目錄, 所以呼叫前要確認目錄已建立</param>
            /// <returns></returns>
            public static string DeleteFile(string sToDirName, string sToFileName, string BackupFullFileName = null)
            {
                return Ftp.DeleteFile(ServerIP, Username, Password, sToDirName, sToFileName, BackupFullFileName);
            }

            /// <summary>
            /// 用 YA00(ProdGroupId) 自動帶出目錄
            /// </summary>
            /// <param name="sFromFileName"></param>
            /// <param name="YA00"></param>
            /// <param name="sToFileName"></param>
            public static void UploadBerryProductFile(string sFromFileName, string YA00, string sToFileName)
            {
                Ftp.UploadFile(ServerIP, Username, Password, sFromFileName, @"GMImg/" + YA00 + "/", sToFileName);
            }
            /// <summary>
            /// 用 PD00 自動帶出目錄
            /// </summary>
            /// <param name="PD00"></param>
            /// <param name="sFileName"></param>
            /// <returns></returns>
            public static DateTime GetProductFileDate(string PD00, string sFileName)
            {
                return GetFileDate(@"pdImg/" + PD00 + "/", sFileName);
            }

            /// <summary>
            /// 用 PD00 自動帶出目錄
            /// </summary>
            /// <param name="sFromFileName"></param>
            /// <param name="PD00"></param>
            /// <param name="sToFileName"></param>
            public static void UploadProductFile(string sFromFileName, string PD00, string sToFileName)
            {
                Ftp.UploadFile(ServerIP, Username, Password, sFromFileName, @"pdImg/" + PD00 + "/", sToFileName);
            }

            /// <summary>
            /// 用 YA00 自動帶出目錄 yaImg/ya00/ (產品圖用的, 不是產品說明圖用的)
            /// </summary>
            /// <param name="sFromFileName"></param>
            /// <param name="YA00"></param>
            /// <param name="sToFileName"></param>
            public static void UploadYAFile(string fromFileName, string ya00, string tofileName)
            {
                Ftp.UploadFile(ServerIP, Username, Password, fromFileName, @"yaImg/" + ya00 + "/", tofileName);
            }

            /// <summary>
            /// 用 marketId 自動帶出目錄 /kol-market/marketId (產品圖用的, 不是產品說明圖用的)
            /// </summary>
            /// <param name="sFromFileName"></param>
            /// <param name="YA00"></param>
            /// <param name="sToFileName"></param>
            public static void UploadKolMarketFile(string fromFileName, long marketId, string tofileName)
            {
                Ftp.UploadFile(ServerIP, Username, Password, fromFileName, @"kol-market/" + marketId + "/", tofileName);
            }

            /// <summary>
            /// 用 PD00 自動帶出目錄
            /// </summary>
            /// <param name="PD00"></param>
            /// <param name="sFromFileName"></param>
            /// <param name="sToFileName"></param>
            public static void DownloadProductFile(string PD00, string sFromFileName, string sToFileName)
            {
                Ftp.DownloadFile(ServerIP, Username, Password, @"pdImg/" + PD00 + "/", sFromFileName, sToFileName);
            }

            public static void DownloadYaFile(string ya00, string fromFileName, string toFileName)
            {
                Ftp.DownloadFile(ServerIP, Username, Password, @"yaImg/" + ya00 + "/", fromFileName, toFileName);
            }

            public static void DownloadKolMarketFile(long marketId, string fromFileName, string toFileName)
            {
                Ftp.DownloadFile(ServerIP, Username, Password, @"kol-market/" + marketId + "/", fromFileName, toFileName);
            }
        }
    }
}
