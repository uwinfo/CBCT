using Su;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    /// <summary>
    /// 分頁的資料表
    /// </summary>
    public class PagedDt
    {
        public DataTable Dt { get; set; }
        public int TotalPage { get; set; }
        public int TotalRecord { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        /// <summary>
        /// 避免 SQL 傳到 Client 端, 改用 function 回傳 SQL;
        /// </summary>
        private string _Sql;
        public void SetSQL(string sql)
        {
            _Sql = sql;
        }

        public string SQL()
        {
            return _Sql;
        }

        public string Pager(string Template, string Css_txtPage = "txtPage", string Css_pagelink = "pagelink", string Css_ddlPager = "ddlPager", string Css_CurrentPage = "CurrentPage", bool IsShowFirstAndLast = true)
        {
            if (this.TotalRecord == 0)
                return "";

            string URL = Su.Wu.URL;

            // Dim After As String = ""

            if (URL.IndexOf("#") > -1)
                // After = URL.Substring(URL.IndexOf("#"))
                URL = URL.Substring(0, URL.IndexOf("#"));
            URL = Su.TextFns.FilterQueryString(URL, "Page");

            Template = Template.Replace("#Css_pagelink#", Css_pagelink);
            Template = Template.Replace("#Css_ddlPager#", Css_ddlPager);
            Template = Template.Replace("#Css_txtPage#", Css_txtPage);

            string PageList = "";
            string SP = "?";
            if (URL.IndexOf("?") > -1)
                SP = "&";
            if (CurrentPage > 1)
            {
                if (IsShowFirstAndLast)
                    PageList += "<a href='" + URL + SP + "Page=1' class='" + Css_pagelink + "'>第一頁</a>&nbsp;";
                PageList += "<a href='" + URL + SP + "Page=" + (CurrentPage - 1) + "' class='" + Css_pagelink + "'>上一頁</a>&nbsp;";
            }

            Int32 StartPage;
            if ((CurrentPage % 10) == 0)
                StartPage = CurrentPage - 9;
            else
                StartPage = CurrentPage - (CurrentPage % 10) + 1;

            if (StartPage > 10)
                PageList += "<a title='前 10 頁' href='" + URL + SP + "Page=" + (StartPage - 1) + "' class='" + Css_pagelink + "'>…</a>&nbsp;";

            Int32 EndPage = StartPage + 9;

            if (EndPage > TotalPage)
                EndPage = TotalPage;

            for (Int32 I = StartPage; I <= EndPage; I++)
            {
                if (I == CurrentPage)
                    PageList += "<span class='" + Css_CurrentPage + "'>" + I + "</span>&nbsp;";
                else
                    PageList += "<a href='" + URL + SP + "Page=" + I + "' class='" + Css_pagelink + "'>" + I + "</a>&nbsp;";
            }

            if (TotalPage > EndPage)
                PageList += "<a title='下 10 頁' href='" + URL + SP + "Page=" + EndPage + 1 + "' class='" + Css_pagelink + "'>…</a>&nbsp;";

            if (TotalPage > CurrentPage)
            {
                PageList += "<a href='" + URL + SP + "Page=" + CurrentPage + 1 + "' class='" + Css_pagelink + "'>下一頁</a>&nbsp;";
                if (IsShowFirstAndLast)
                    PageList += "<a href='" + URL + SP + "Page=" + TotalPage + "' class='" + Css_pagelink + "'>最後一頁</a>&nbsp;";
            }
            Template = Template.Replace("#PageList#", PageList);

            string PagerOptions = "";
            for (Int32 I = 1; I <= TotalPage; I++)
                PagerOptions += " <option value=\"" + URL + SP + "Page=" + I + "\" " + (I == CurrentPage ? "selected" : "") + ">" + I + "</option>";
            Template = Template.Replace("#PagerOptions#", PagerOptions);

            Template = Template.Replace("#TotalPage#", TotalPage.ToString());
            Template = Template.Replace("#TotalRecord#", TotalRecord.ToString());

            return Template;
        }


        public PagerInfo pager
        {
            get
            {
                var TotalPage = 0;
                var StartItemIndex = 0;
                var EndItemIndex = TotalRecord;
                var StartPage = 0;
                var EndPage = 0;

                if (TotalRecord > 0)
                {
                    if ((TotalRecord % PageSize) == 0)
                    {
                        TotalPage = TotalRecord / PageSize;
                    }
                    else
                    {
                        TotalPage = (TotalRecord / PageSize) + 1;
                    }

                    if (CurrentPage > TotalPage) CurrentPage = TotalPage;

                    if (CurrentPage == 1)
                    {
                        StartItemIndex = 1;
                    }
                    else
                    {
                        if (TotalPage > 0)
                        {
                            StartItemIndex = ((PageSize * (CurrentPage - 1)) + 1);
                        }
                    }

                    if (CurrentPage < TotalPage)
                    {
                        EndItemIndex = PageSize * CurrentPage;
                    }

                    var PageItemCount = 10;
                    if (TotalRecord > 0)
                    {
                        if (CurrentPage > PageItemCount)
                        {
                            //無條件進位
                            StartPage = (Convert.ToInt32(Math.Ceiling(CurrentPage.ToFloat() / PageItemCount.ToFloat())) - 1) * PageItemCount + 1;
                        }
                        else
                        {
                            StartPage = 1;
                        }
                    }

                    EndPage = StartPage + (PageItemCount - 1);
                    if (EndPage > TotalPage) EndPage = TotalPage;
                }

                return new PagerInfo
                {
                    CurrentPage = CurrentPage,
                    TotalPage = TotalPage,
                    TotalRecord = TotalRecord,
                    StartItemIndex = StartItemIndex,
                    EndItemIndex = EndItemIndex,
                    StartPage = StartPage,
                    EndPage = EndPage
                };
            }
        }
    }

    public class PagerInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }
        public int TotalRecord { get; set; }
        public int StartItemIndex { get; set; }
        public int EndItemIndex { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }
}
