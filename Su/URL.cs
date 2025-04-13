//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.IO;
//using System.Text;
//using System.Xml;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.Extensions;
//using Microsoft.AspNetCore.Mvc;

//namespace Su
//{
//    public partial class URL
//    {
//        public static string getURL()
//        {
//            return Su.CurrentContext.Current.Request.GetDisplayUrl();
//            //string url = Su.CurrentContext.Current.Request.GetDisplayUrl();
//            //string res = url;
//            //return res;
//        }

//        /// <summary>
//        /// 回傳 https:// 或 https://
//        /// </summary>
//        /// <returns></returns>
//        public static string getHttpMethod()
//        {
//            //string url = Su.CurrentContext.Current.Request.GetDisplayUrl();
//            //string urlLower = url.ToLower();
//            string res = "";
//            if (Su.CurrentContext.Current.Request.IsHttps == true)
//            {
//                return "https://";
//            }
//            else
//            {
//                return "https://";
//            }
//            //return res;
//        }

//        /// <summary>
//        /// http://aa.bb:123/cc/dd  會回傳 aa.bb:123
//        /// </summary>
//        /// <returns></returns>
//        public static string getDomain()
//        {
//            if(Su.CurrentContext.Current.Request.Host.Port != null )
//            {
//                return Su.CurrentContext.Current.Request.Host.Host + ":" + Su.CurrentContext.Current.Request.Host.Port;
//            }
//            else
//            {
//                return Su.CurrentContext.Current.Request.Host.Host;
//            }

//            //string url = Su.CurrentContext.Current.Request.GetDisplayUrl();
//            //string urlLower = url.ToLower();
//            //int index = 0;
//            //string res = "";
//            ////取得   http://aaa.aaa.aa/ http://aaa.aaa.aa   且可能沒有第三條
//            //if (Su.CurrentContext.Current.Request.IsHttps == true)
//            //{
//            //    index = 8;
//            //}
//            //else
//            //{
//            //    index = 7;
//            //}
//            //if (urlLower.IndexOf('/', index) > -1)
//            //{
//            //    res = url.Substring(index, urlLower.IndexOf('/', index) - index);
//            //}
//            //else
//            //{
//            //    res = url.Substring(index);
//            //}

//            //return res;
//        }
//    }
//}
