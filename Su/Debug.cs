using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    public class Debug
    {
        public static bool IsDebug { get; set; } = false;

        /// <summary>
        /// System.Diagnostics.Debug.WriteLine(msg);
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLine(string msg)
        {
            if (IsDebug)
            {
                System.Diagnostics.Debug.WriteLine(msg);
            }
        }

        public static void WriteObject(object obj, string title)
        {
            if (IsDebug)
            {
                System.Diagnostics.Debug.WriteLine(title);
                System.Diagnostics.Debug.WriteLine(obj.Json(isIndented: true));
            }
        }


        public static void AppendLog(string msg, bool isAddDateTime = true, string? filename = null)
        {
            if (filename != null && filename.Length > 0)
            {
                var root = Su.Wu.LogRoot;
                if (root != null)
                {
                    System.IO.Directory.CreateDirectory(root);
                    filename = root + filename;
                }
            }
            else
            {
                filename = LogFilename;
            }

            if (isAddDateTime) msg = DateTime.Now.ToString("HH:mm:ss:fff") + ", " + msg;
            Su.Wu.AppendLog(filename, "--" + msg, false, false, false);
            WriteLine(msg);
        }

        public static string LogFilename
        {
            get
            {
                var root = Su.Wu.LogRoot;
                if (root != null)
                {
                    System.IO.Directory.CreateDirectory(root);
                    return root + System.DateTime.Now.ToString("yyyyMMdd") + ".log";
                }
                return "";
            }
        }
    }
}
