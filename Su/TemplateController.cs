using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Caching;

namespace Su
{    
    public class TemplateController : Controller
    {
        ClaimHelper? __member;

        protected ClaimHelper? _member
        {
            get
            {
                if (__islogin == null) //確保只會執行一次設定的區段
                {
                    if (HttpContext?.User != null)
                    {
                        __member = new ClaimHelper(HttpContext.User);
                    }

                    __islogin = __member != null;
                }

                return __member;
            }
        }

        bool? __islogin = null;

        protected bool _islogin
        {
            get
            {
                if (__islogin == null) //確保只會執行一次設定的區段
                {
                    __islogin = (_member != null && !string.IsNullOrEmpty(_member.Uid));
                }
                return (bool)__islogin;
            }
        }

        public Template MasterTemplate;
        public Template ContentWrapTemplate;

        public Su.Template HeaderTemplate = null;
        public Su.Template ContentTemplate = null;
        public Su.Template FinalScriptTemplate = null;

        public string Title = "";
        public string Description = "";
        public static string PhotoHost = null;

        IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 傳入 httpContextAccessor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public TemplateController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TemplateController()
        {

        }

        void SetScriptWithTime(Template template)
        {
            //if (!template.result.Contains("<!--ScriptWithTime S-->"))
            //{
            //    return;
            //}

            //Su.TemplateController.AssetsFileRoot

            ////Su.TemplateController.FrontEndAssetsFileRoot

        }

        [NonAction]
        public void Init(string ContentFileName, string MasterFileName, string lang = "")
        {
            this.MasterTemplate = new Template(MasterFileName, lang);
            SetScriptWithTime(MasterTemplate);

            if (! string.IsNullOrEmpty(ContentFileName))
            {
                this.ContentWrapTemplate = new Template(ContentFileName, lang);
                this.HeaderTemplate = ContentWrapTemplate.Child("Header");
                this.ContentTemplate = ContentWrapTemplate.Child("Content");
                this.FinalScriptTemplate = ContentWrapTemplate.Child("FinalScript");
                if (this.ContentWrapTemplate.result.IndexOf("<!--Title S") > 0)
                {
                    this.Title = this.ContentWrapTemplate.SubTemplate("Title").result.RemoveHtml().Trim();
                }
                if (this.ContentWrapTemplate.result.IndexOf("<!--Description S") > 0)
                {
                    var decTemp = this.ContentWrapTemplate.SubTemplate("Description");
                    int startIndex = decTemp.result.IndexOf("content=\"");
                    if (startIndex > 0)
                    {
                        startIndex += 9;
                        int endIndex = decTemp.result.IndexOf("\"", startIndex);
                        if (endIndex > startIndex + 1)
                        {
                            this.Description = decTemp.result.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                }
            }
        }

        ///// <summary>
        ///// 設為 true 時, 不會輸出 Template 的內容.
        ///// </summary>
        //public bool IsError = false;
        //[NonAction]
        //public void WriteError(string Message)
        //{
        //    IsError = true;
        //    Su.Wu.WriteText(Message);
        //}

        //private IActionResult _ReturnResult = null;
        //[NonAction]
        //public void ReturnResult(IActionResult result)
        //{
        //    IsError = true;
        //    this._ReturnResult = result;
        //}

        //public bool IsWriteMaster = true;

        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    var s = DateTime.Now;

        //    try
        //    {
        //        if (filterContext.Result != null && filterContext.Result.GetType() == typeof(RedirectResult))
        //        {
        //            return;
        //        }

        //        if (IsError)
        //        {
        //            filterContext.Result = _ReturnResult;
        //            return;
        //        }

        //        base.OnActionExecuted(filterContext);

        //        if(this.MasterTemplate != null)
        //        {
        //            AutoCssAndScript();

        //            if (HeaderTemplate != null)
        //            {
        //                this.MasterTemplate.ReplaceString("<!--Header-->", this.HeaderTemplate.ResultAfterBuildAll_TRIM);
        //            }
        //            if (ContentTemplate != null)
        //            {
        //                this.MasterTemplate.ReplaceString("<!--Content-->", this.ContentTemplate.ResultAfterBuildAll_TRIM);
        //            }
        //            if (FinalScriptTemplate != null)
        //            {
        //                this.MasterTemplate.ReplaceString("<!--FinalScript-->", this.FinalScriptTemplate.ResultAfterBuildAll_TRIM);
        //            }

        //            this.MasterTemplate
        //                .ReplaceString("<!--Title-->", this.Title)
        //                .ReplaceString("<!--Description-->", this.Description)
        //                .ReplaceString("{PhotoHost}", PhotoHost)
        //                ;

        //            var diff = (DateTime.Now - s).TotalMilliseconds;
        //            Su.Debug.WriteLine("total Time: " + diff);

        //            //Su.FileLogger.AddOneDayLog("performance", "TemplateController OnActionExecuted run time: " + diff + " ms");

        //            filterContext.Result = Content(this.MasterTemplate.result, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        filterContext.Result = Content(ex.ToString() + ", " + ex.StackTrace, new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/html"));
        //    }

        //    base.OnActionExecuted(filterContext);
        //}

        //public static string WwwFileRoot { get; set; } = "";

        //[NonAction]
        //public void AutoCssAndScript()
        //{
        //    if(MasterTemplate == null)
        //    {
        //        return;
        //    }

        //    string assetsFileRoot = WwwFileRoot.AddPath("assets");

        //    string mergedCss = GetMergedFile(assetsFileRoot.AddPath("css", "auto"), assetsFileRoot.AddPath("css", "merged_css"), ".css");
        //    MasterTemplate.ReplaceString("<!--AutoCss-->", $"<link href='/assets/css/merged_css/{mergedCss}' rel='stylesheet'>");

        //    string mergedJs = GetMergedFile(assetsFileRoot.AddPath("js", "auto"), assetsFileRoot.AddPath("js", "merged_js"), ".js");
        //    MasterTemplate.ReplaceString("<!--AutoJs-->", $"<script src='/assets/js/merged_js/{mergedJs}'></script>");
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourceFolder">應包含目錄的結尾符號</param>
        /// <param name="DestinationFolder">應包含目錄的結尾符號</param>
        /// <param name="ext">包含 .</param>
        /// <returns>所有檔案的清單, 最後一個是 merge 後的檔名</returns>
        static List<string> MergeAllFile(string SourceFolder, string DestinationFolder, string ext)
        {
            string mergedFile = DateTime.Now.Ymdhmsf() + ext;
            System.IO.Directory.CreateDirectory(DestinationFolder);

            List<string> filePaths = new();
            filePaths.Add(SourceFolder);

            var dirInfo = new System.IO.DirectoryInfo(SourceFolder);
            var files = dirInfo.GetFiles().OrderBy(f => f.Name);
            var destinationFile = DestinationFolder.AddPath(mergedFile);
            if(files.Count() == 0)
            {
                System.IO.File.AppendAllText(destinationFile, "", System.Text.Encoding.UTF8);
            }
            else
            {
                foreach (var file in files)
                {
                    filePaths.Add(file.FullName);
                    System.IO.File.AppendAllText(destinationFile, "/******" + file.Name + "******/\r\n", System.Text.Encoding.UTF8);

                    System.IO.File.AppendAllText(destinationFile, System.IO.File.ReadAllText(file.FullName) + "\r\n\r\n", System.Text.Encoding.UTF8);
                }
            }            

            //Remove Old Files
            List<string> oldFiels = System.IO.Directory.GetFiles(DestinationFolder).OrderByDescending(x => x).ToList();
            if (oldFiels.Count > 10)
            {
                for (int i = 5; i < oldFiels.Count; i++)
                {
                    try
                    {
                        System.IO.File.Delete(oldFiels[i]);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            filePaths.Add(mergedFile);

            return filePaths;
        }

        /// <summary>
        /// sourceFolder 的指定附檔名的檔案會被合併到 destinationFolder，依檔案名稱排序
        /// </summary>
        /// <returns></returns>
        static string GetMergedFile(string sourceFolder, string destinationFolder, string ext)
        {
            
            string CacheKey = sourceFolder;
            ObjectCache cache = MemoryCache.Default;
            string? obj = (string?)cache[CacheKey];
            if (obj == null)
            {
                lock (LockerProvider.GetLocker(CacheKey))
                {
                    obj = (string?)cache[CacheKey];
                    if (obj == null)
                    {
                        List<string> filePaths = MergeAllFile(sourceFolder, destinationFolder, ext);
                        obj = filePaths[^1];
                        filePaths.RemoveAt(filePaths.Count - 1);

                        CacheItemPolicy policy = new();
                        policy.ChangeMonitors.Add(new
                            HostFileChangeMonitor(filePaths));
                        cache.Set(CacheKey, obj, policy);
                    }
                }
            }

            return obj ?? "";
        }

        public ClaimHelper? LoginUser
        {
            get
            {
                if (HttpContext?.User != null && HttpContext.User.Identity.IsAuthenticated)
                {
                    return new ClaimHelper(HttpContext.User);
                }

                return null;
            }
        }

        public bool IsLogin
        {
            get
            {
                if (HttpContext?.User != null 
                    && HttpContext.User.Identity.IsAuthenticated
                    && !string.IsNullOrEmpty(LoginUser.LineId))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
