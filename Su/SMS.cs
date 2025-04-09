using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    public class SMS
    {
        /// <summary>
        /// 使用三竹發送簡訊 (SendSMS)
        /// </summary>
        /// <param name="Msg"></param>
        public static void SendByMitake(string mobile, string msg)
        {
            string url = $"http://smexpress.mitake.com.tw:9600/SmSendGet.asp?username=28511849&password=4546ebay&encoding=UTF8&dstaddr={mobile}&&smbody={msg.UrlEncode().Replace("+", "%20")}";

            Su.Wu.GetRemotePage(url);
        }
    }
}
