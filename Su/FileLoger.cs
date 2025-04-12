using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    public class FileLogger: IDisposable
    {
        // Pointer to an external unmanaged resource.
        private IntPtr handle;
                
        // Track whether Dispose has been called.
        private bool disposed = false;
        // The class constructor.
        public FileLogger(IntPtr handle)
        {
            this.handle = handle;
        }
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    oFile.Close();
                    oFile.Dispose();
                }
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;
                // Note disposing has been done.
                disposed = true;
            }
        }
        // Use interop to call the method necessary
        // to clean up the unmanaged resource.
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);
        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~FileLogger()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }


        StreamWriter oFile;


        /// <summary>
        /// 一率採用 Append 的方式新增 log
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="encoding">預設 UTF8 </param>
        public FileLogger(string Filename, System.Text.Encoding encoding = null)
        {
            if (encoding == null)
            {
                oFile = new StreamWriter(Filename, true, System.Text.Encoding.UTF8);
            }
            else
            {
                oFile = new StreamWriter(Filename, true, encoding );
            }
        }

        public void Writer(string Data)
        {
            oFile.Write(Data);
        }

        public void Close()
        {
            oFile.Close();
        }

        public static void WriteText1(string FileName, List<string> ltData)
        {
            foreach(string data in ltData)
            {
                System.IO.File.AppendAllText(FileName, data);
            }
        }

        public static void WriteText2(string FileName, List<string> ltData)
        {
            var Res = "";
            foreach (string data in ltData)
            {
                Res += data;
            }

            System.IO.File.WriteAllText(FileName, Res);
        }

        public static void AppendLog(string filename, string message, bool isAddDateTime = true, bool isCrLf = true, bool isThrowException = false)
        {
            try
            {
                StreamWriter w = File.AppendText(filename);

                if (isAddDateTime)
                {
                    if (isCrLf)
                    {
                        w.WriteLine("\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "\r\n" + message);
                    }
                    else
                    {
                        w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "\r\n" + message);
                    }
                }
                else if (isCrLf)
                {
                    w.WriteLine("\r\n" + message);
                }
                else
                {
                    w.WriteLine(message);
                }

                w.Flush();
                w.Close();
            }
            catch (Exception)
            {
                if (isThrowException)
                {
                    throw;
                }   
            }
        }


        ///// <summary>
        ///// 會建立 folder, 每天建立一個檔案 ymd.log
        ///// </summary>
        ///// <param name="Directory"></param>
        ///// <param name="Message"></param>
        ///// <param name="RemoveFilesBeforeDays">有補了哦.</param>
        ///// <param name="Prefix"></param>
        ///// <param name="isThrowExctption"></param>
        //static void AppendDailyLog(string Directory, string Message, int RemoveFilesBeforeDays = 0, string Prefix = "", bool isThrowExctption = true)
        //{
        //    try
        //    {
        //        if (!Directory.StartsWith(OneDayLogDirectory))
        //        {
        //            Directory = OneDayLogDirectory.AddPath(Directory);
        //        }

        //        Su.FileUtility.CreateDirectory(Directory);

        //        if (!string.IsNullOrEmpty(Prefix))
        //        {
        //            Wu.AppendLog(System.IO.Path.Combine(Directory, Prefix + "_" + DateTime.Now.Date.Ymd() + ".log"), Message);
        //        }
        //        else
        //        {
        //            Wu.AppendLog(System.IO.Path.Combine(Directory, DateTime.Now.Date.Ymd() + ".log"), Message);
        //        }

        //        if (RemoveFilesBeforeDays > 0)
        //        {
        //            Su.FileUtility.RemoveOldFile(Directory, DateTime.Now.AddDays(-RemoveFilesBeforeDays), IsOncePerDay: true, isInThread: true);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (isThrowExctption)
        //        {
        //            throw;
        //        }
        //    }
        //}

        //public static bool IsAddOneDayLogDirectoryCreated = false;

        ///// <summary>
        ///// 使用共用的參數 OneDayLogDirectory
        ///// 預設會保留三天.
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="message"></param>
        ///// <param name="removeFilesBeforeDays"></param>
        ///// <param name="isThrowExctption"></param>
        //public static void AddOneDayLog(string type, string message, int removeFilesBeforeDays = 3, bool isThrowExctption = false, string directory = null)
        //{
        //    Su.FileLogger.AddOneDayLog(type, message, removeFilesBeforeDays, isThrowExctption, directory);
        //}

        public static string? _OneDayLogDirectory = null;
        public static string OneDayLogDirectory
        {
            get
            {
                return _OneDayLogDirectory ?? throw new Exception("未設定 OneDayLogDirectory");
            }

            set
            {
                if (_OneDayLogDirectory != null)
                {
                    throw new Exception("_OneDayLogDirectory 不可重覆設定");
                }

                _OneDayLogDirectory = value;
            }
        }

        /// <summary>
        /// 會放在 OneDayLog 的 exception 子目錄，
        /// 保留 30 天
        /// </summary>
        /// <param name="ex"></param>
        public static void AddExceptionDailyLog(Exception ex)
        {
            AddDailyLog("exception", ex.FullInfo(), removeFilesBeforeDays: 30);
        }

        /// <summary>
        /// 預設會保留三天的 Log，避免 Log 成長太大。
        /// 同一目錄下的檔案會保留相同天數, 所以不提供檔名的 Prefix 的選項
        /// </summary>
        /// <param name="subDirectory"></param>
        /// <param name="message"></param>
        /// <param name="removeFilesBeforeDays"> 0 表示不要刪除</param>
        /// <param name="isThrowException"></param>
        /// <param name="logBeore"></param>
        /// <param name="fileNameMode">可傳入 day, hour, 10mins</param>
        /// <exception cref="Exception"></exception>
        public static void AddDailyLog(string subDirectoryx, string message, int removeFilesBeforeDays = 3, bool isThrowException = false, int logBeore = 30001231,
            string fileNameMode = "day", string? filenamePrefix = null)
        {
            //AppendDailyLog(subDirectory, message, removeFilesBeforeDays, filenamePrefix, isThrowException);

            if (OneDayLogDirectory == null)
            {
                if (isThrowException)
                {
                    throw new Exception("LogRoot is not specified.");
                }
                else
                {
                    return;
                }
            }

            var directory = subDirectoryx;
            if (!directory.StartsWith(OneDayLogDirectory))
            {
                directory = OneDayLogDirectory.AddPath(directory);
            }
            Su.FileUtility.CreateDirectory(directory);

            if (DateTime.Now.ToString("yyyyMMdd").ToInt32() > logBeore)
            {
                return;
            }

            lock (Su.LockerProvider.GetLocker($"AddDailyLog_{subDirectoryx}"))
            {
                try
                {
                    string filename = DateTime.Now.Date.Ymd() + ".log";
                    if (fileNameMode == "hour")
                    {
                        filename = DateTime.Now.ToString("yyyyMMddHH") + ".log";
                    }
                    else if (fileNameMode == "10mins")
                    {
                        long t = DateTime.Now.ToString("yyyyMMddHHmm").ToInt64();
                        filename = $"{t - t % 10}.log";
                    }
                    else if (fileNameMode == "day")
                    {
                        filename = DateTime.Now.Date.Ymd() + ".log";
                    }
                    else
                    {
                        throw new Exception("unknow file mode");
                    }

                    if (!string.IsNullOrEmpty(filenamePrefix))
                    {
                        filename = filenamePrefix + filename;
                    }

                    
                    AppendLog(directory.AddPath(filename), message, isThrowException: isThrowException);

                    if (removeFilesBeforeDays > 0)
                    {
                        Su.FileUtility.RemoveOldFile(directory, DateTime.Now.AddDays(-removeFilesBeforeDays), IsOncePerDay: true, isInThread: true);
                    }
                }
                catch (Exception)
                {
                    if (isThrowException)
                    {
                        throw;
                    }
                }
            }
        }

        ///// <summary>
        ///// 使用共用的參數 OneDayLogDirectory
        ///// 預設會保留三天.
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="message"></param>
        ///// <param name="removeFilesBeforeDays"></param>
        ///// <param name="isThrowExctption"></param>
        ///// <param name="directory"></param>
        //public static void AddOneDayLog(string prefix, string message, int removeFilesBeforeDays = 10, bool isThrowExctption = false, int LogBeore = 30001231, string? subDirectory = null, bool isTryCreateDirectory = false)
        //{
        //    lock (Su.LockerProvider.GetLocker("AddOneDayLog"))
        //    {
        //        if (DateTime.Now.ToString("yyyyMMdd").ToInt32() > LogBeore)
        //        {
        //            return;
        //        }

        //        if (OneDayLogDirectory == null)
        //        {
        //            throw new Exception("請先設定 OneDayLogDirectory");
        //        }

        //        var directory = OneDayLogDirectory.AddPath(subDirectory ?? "");

        //        if (isTryCreateDirectory)
        //        {
        //            System.IO.Directory.CreateDirectory(directory);
        //        }

        //        AppendDailyLog(directory, message, removeFilesBeforeDays, prefix, isThrowExctption);
        //    }
        //}
    }
}
