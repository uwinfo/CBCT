using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Su;

namespace Su
{
    /// <summary>
    /// Validator 的摘要描述
    /// </summary>
    public class Validator
    {
        public string error = null;
        public string seperator = "\r\n";

        public enum Type
        {
            _int = 10,
            _double = 11,
            _decimal = 12,
            _string = 20,
            /// <summary>
            /// 台灣手機號 09xxxxx
            /// </summary>
            _twMobile = 21,
            _datetime = 30,

            /// <summary>
            /// 日期(不可包含時間)
            /// </summary>
            _date = 31,

            /// <summary>
            /// yyyyMMdd
            /// </summary>
            _dateYMD = 32
        }

        public Validator()
        {
            //
            // TODO: 在這裡新增建構函式邏輯
            //
        }

        /// <summary>
        /// 回傳目前的錯誤.
        /// </summary>
        /// <param name="msg">傳入 null 時不處理</param>
        /// <returns></returns>
        public string AddError(string msg)
        {
            if(msg == null)
            {
                return null;
            }

            if(error == null)
            {
                error = msg;
            }
            else
            {
                error += seperator + msg;
            }

            return msg;
        }

        public bool IsEmailBadDomain(string email)
        {
            email = email.ToLower();
            if (email.EndsWith("@gmail.com") || email.EndsWith("@yahoo.com.tw") || email.EndsWith("@hotmail.com") || email.EndsWith("@icloud.com") || email.EndsWith("@yahoo.com"))
            {
                return false;
            }

            string[] strBadDomains = @"gmail.com.tw
gmail.co
gmai.com
gmial.com
gamil.com
gmail.tw
gnail.com
gamil.com.tw
gmaii.com
.cpm
.vom
.vom.tw
.con.tw
.cpm.tw
.cm.tw
.con
.te
.rw
yshoo.com
yhaoo.com
yshoo.com.tw
yhaoo.com.tw
hitmail.com
hotmil.com
homail.com
hotmai.com
..com.tw
..com
outlook. com
com.tw/
yahoo com
@.com
..tw".Replace("\r", "").Split('\n');
            
            foreach (string item in strBadDomains)
            {
                if (email.EndsWith(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void EmptyCheck(string inputKey, string displayName, Type type = Type._string, bool isEmptyOK = false)
        {
            Check(inputKey, displayName, type, true);
        }

        public string CheckFile(IFormFileCollection files, string displayName, string exts = "jpg,gif,png,jpeg,xls,xlsx,svg", int maxSize = 0, bool isEmptyOk = true)
        {
            foreach(var f in files)
            {
                var msg = CheckFile(f, displayName, exts, maxSize, isEmptyOk);
                if (! string.IsNullOrEmpty(msg))
                {
                    return msg;
                }
            }

            return null;
        }

        /// <summary>
        /// 檢查檔案的類別, 大小, 可指定是否為必需上傳
        /// </summary>
        /// <param name="file"></param>
        /// <param name="displayName"></param>
        /// <param name="exts"></param>
        /// <param name="maxSize"></param>
        /// <param name="isEmptyOk"></param>
        /// <returns></returns>
        public static string GetErrorMessageFromFile(IFormFile file, string displayName, string exts = "jpg,gif,png,jpeg,xls,xlsx,svg", int maxSize = 0, bool isEmptyOk = true)
        {
            if (file != null && file.Length > 0)
            {
                if (!file.IsValidExt(exts))
                {
                    return displayName + "限定上傳 " + exts + " 格式的檔案.";
                }

                if (maxSize > 0 && file.Length > maxSize)
                {
                    if (maxSize > 1024)
                    {
                        return displayName + "限定上傳 " + maxSize / 1024 + "K 以內的檔案.";
                    }
                    else
                    {
                        return displayName + "限定上傳 " + maxSize + "Bytes 以內的檔案.";
                    }
                }
            }
            else if (!isEmptyOk)
            {
                return displayName + "為必需上傳檔案.";
            }

            return null;
        }


        /// <summary>
        /// 檢查檔案附檔名及大小
        /// </summary>
        /// <param name="file"></param>
        /// <param name="displayName"></param>
        /// <param name="exts"></param>
        /// <param name="maxSize"></param>
        /// <param name="isEmptyOk"></param>
        /// <returns></returns>
        public string CheckFile(IFormFile file, string displayName, string exts = "jpg,gif,png,jpeg,xls,xlsx,svg", int maxSize = 0, bool isEmptyOk = true)
        {
            var msg = GetErrorMessageFromFile(file, displayName, exts, maxSize, isEmptyOk);
            if(msg != null)
            {
                this.AddError(msg);
            }

            return msg;
        }

        //public string CheckImageWH(IFormFileCollection files, string displayName, int w, int h, int checkType)
        //{
        //    foreach (var f in files)
        //    {
        //        var msg = CheckImageWH(f, displayName, w, h, checkType);
        //        if (!string.IsNullOrEmpty(msg))
        //        {
        //            return msg;
        //        }
        //    }

        //    return null;
        //}


        ///// <summary>
        ///// 檢查上傳圖檔的長寬, 大小(bytes), 類別, 並可指定檢查方式
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="displayName"></param>
        ///// <param name="w"></param>
        ///// <param name="h"></param>
        ///// <param name="checkType">0: 長寬必需吻合; 1: 長寬必需大於 W, H; 2: 長寬必需小於 W, H</param>
        ///// <param name="isEmptyOK"></param>
        ///// <param name="maxSize"></param>
        ///// <param name="exts"></param>
        ///// <returns></returns>
        //public static string GetErrorMessageFromImage(IFormFile file, string displayName, int w, int h, int checkType, bool isEmptyOK = true, 
        //    int maxSize = 0, string exts = "jpg,gif,png,jpeg,xls,xlsx,svg")
        //{
        //    if (file == null || file.Length <= 0)
        //    {
        //        if (isEmptyOK)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return "請上傳檔案 " + displayName;
        //        }
        //    }

        //    if (!file.IsValidExt(exts))
        //    {
        //        return displayName + "限定上傳 " + exts + " 格式的檔案.";
        //    }

        //    if (maxSize > 0 && file.Length > maxSize)
        //    {
        //        if (maxSize > 1024)
        //        {
        //            return displayName + "限定上傳 " + maxSize / 1024 + "K 以內的檔案.";
        //        }
        //        else
        //        {
        //            return displayName + "限定上傳 " + maxSize + "Bytes 以內的檔案.";
        //        }
        //    }

        //    var WH = Su.FileUtility.GetWH(file);

        //    switch (checkType)
        //    {
        //        case 0:
        //            if (w > 0 && w != WH.Width || h > 0 && h != WH.Height)
        //            {
        //                return displayName + "的長寬必需合於 " + w + "x" + h;
        //            }
        //            break;
        //        case 1:
        //            if (w > 0 && w < WH.Width || h > 0 && h < WH.Height)
        //            {
        //                return displayName + "的長寬必需大於或等於 " + w + "x" + h;
        //            }
        //            break;
        //        case 2:
        //            if (w > 0 && w > WH.Width || h > 0 && h > WH.Height)
        //            {
        //                return displayName + "的長寬必需小於或等於 " + w + "x" + h;
        //            }
        //            break;
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// 檢查上傳圖檔的長寬
        ///// </summary>
        ///// <param name="file"></param>
        ///// <param name="displayName"></param>
        ///// <param name="w"></param>
        ///// <param name="h"></param>
        ///// <param name="checkType">0: 長寬必需吻合; 1: 長寬必需大於 W, H; 2: 長寬必需小於 W, H</param>
        ///// <returns></returns>
        //public string CheckImageWH(IFormFile file, string displayName, int w, int h, int checkType)
        //{
        //    return AddError(GetErrorMessageFromImage(file, displayName, w, h, checkType));
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="displayName"></param>
        /// <param name="type"></param>
        /// <param name="isEmptyOK"></param>
        /// <param name="maxValue"></param>
        /// <param name="minValue"></param>
        /// <param name="action"></param>
        /// <returns>若是有錯誤, 回傳錯誤訊息.</returns>
        public string CheckContent(string Content, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null, string action = "輸入")
        {
            if (string.IsNullOrEmpty(Content))
            {
                if (isEmptyOK)
                {
                    return null;
                }
                else
                {
                    return AddError("請" + action + displayName);
                }
            }

            switch (type)
            {
                case Type._datetime:
                    if (! Content.IsDate())
                    {
                        return AddError(displayName + "格式錯誤, 應為日期(可包含時間)。");
                    }
                    return null;
                case Type._dateYMD:
                    if(Content.Length != 8)
                    {
                        return AddError(displayName + "格式錯誤, 應為8碼數字的日期(yyyyMMdd)。");
                    }
                    string D = Content.Substring(0, 4) + "-" + Content.Substring(4, 2) + "-" + Content.Substring(6);
                    if (!D.IsDate(true))
                    {
                        return AddError(displayName + "格式錯誤, 應為8碼數字的日期(yyyyMMdd)。");
                    }
                    return null;
                case Type._date:
                    if (!Content.IsDate(true))
                    {
                        return AddError(displayName + "格式錯誤, 應為日期(不包含時間)。");
                    }
                    return null;
                case Type._int:
                    if (!Content.IsInt())
                    {
                        return AddError(displayName + "格式錯誤, 應為整數。");
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if(double.Parse(Content) > maxValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可大於 " + maxValue);
                            }                            
                        }
                        if(minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可小於 " + minValue);
                            }
                        }
                    }
                    return null;
                case Type._double:
                    if (!Content.IsNumeric())
                    {
                        return AddError(displayName + "格式錯誤, 應為數字。");
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if (double.Parse(Content) > maxValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可大於 " + maxValue);
                            }
                        }
                        if (minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可小於 " + minValue);
                            }
                        }
                    }
                    return null;
                case Type._decimal:
                    if (!Content.IsNumeric())
                    {
                        return AddError(displayName + "格式錯誤, 應為數字。");
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if (double.Parse(Content) > maxValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可大於 " + maxValue);
                            }
                        }
                        if (minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return AddError(displayName + "資料錯誤, 不可小於 " + minValue);
                            }
                        }
                    }
                    return null;
                case Type._twMobile:
                    Regex regex = new(@"^09[0-9]{8}$");
                    if (!regex.IsMatch(Content))
                    {
                        return AddError("手機號碼格式錯誤，請輸入09開頭共10位數字");
                    }
                    return null;
                case Type._string:
                    //字串只要判斷是否為空白即可.
                    return null;

                default:
                   return AddError("無法識別的類別: " + type.ToString());
            }
        }

        public string Check(string inputKey, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null)
        {
            string action = "輸入";
            if (inputKey.StartsWith("ddl"))
            {
                action = "選擇";
            }

            return CheckContent(Su.Wu.GetValue(inputKey), displayName, type, isEmptyOK, maxValue, minValue, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputKey"></param>
        /// <param name="displayName"></param>
        /// <param name="type"></param>
        /// <param name="isEmptyOK"></param>
        /// <param name="maxValue"></param>
        /// <param name="minValue"></param>
        /// <param name="action">不傳入時, 預設是 "輸入", 若 inputKey.StartsWith("ddl") 則為 "選擇"</param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string GetErrorMessageFromInput(string inputKey, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null, string action = null, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(action))
            {
                action = "輸入";
                if (inputKey.StartsWith("ddl"))
                {
                    action = "選擇";
                }
            }

            return GetErrorMessage(Su.Wu.GetValue(inputKey), displayName, type, isEmptyOK, maxValue, minValue, action, maxLength);
        }

        public static string GetErrorMessageFromObject(object obj, string fieldName, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null, string action = "輸入", int maxLength = 0)
        {
            string content = null;
            object contentObj = obj.GetValue(fieldName);
            if(contentObj != null)
            {
                content = contentObj.ToString();
            }

            return GetErrorMessage(content, displayName, type, isEmptyOK, maxValue, minValue, action, maxLength);
        }

        public static string GetErrorMessageFromObject(ExpandoObject obj, string fieldName, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null, string action = "輸入", int maxLength = 0)
        {
            var dic = (IDictionary<string, object>)obj;

            string content = null;
            if (dic.ContainsKey(fieldName) && dic[fieldName] != null)
            {
                content = dic[fieldName].ToString();
            }

            return GetErrorMessage(content, displayName, type, isEmptyOK, maxValue, minValue, action, maxLength);
        }

        public static string GetErrorMessage(string Content, string displayName, Type type = Type._string, bool isEmptyOK = false,
            double? maxValue = null, double? minValue = null, string action = "輸入", int maxLength = 0)
        {

            if (string.IsNullOrEmpty(action))
            {
                action = "輸入";
            }

            if (string.IsNullOrEmpty(Content))
            {
                if (isEmptyOK)
                {
                    return null;
                }
                else
                {
                    return "請" + action + displayName;
                }
            }

            switch (type)
            {
                case Type._datetime:
                    if (!Content.IsDate())
                    {
                        return displayName + "格式錯誤, 應為日期(可包含時間)。";
                    }
                    return null;
                case Type._dateYMD:
                    if (Content.Length != 8)
                    {
                        return displayName + "格式錯誤, 應為8碼數字的日期(yyyyMMdd)。";
                    }
                    string D = Content.Substring(0, 4) + "-" + Content.Substring(4, 2) + "-" + Content.Substring(6);
                    if (!D.IsDate(true))
                    {
                        return displayName + "格式錯誤, 應為8碼數字的日期(yyyyMMdd)。";
                    }
                    return null;
                case Type._date:
                    if (!Content.IsDate(true))
                    {
                        return displayName + "格式錯誤, 應為日期(不包含時間)。";
                    }
                    return null;
                case Type._int:
                    if (!Content.IsInt())
                    {
                        return displayName + "格式錯誤, 應為整數。";
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if (double.Parse(Content) > maxValue)
                            {
                                return displayName + "資料錯誤, 不可大於 " + maxValue;
                            }
                        }
                        if (minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return displayName + "資料錯誤, 不可小於 " + minValue;
                            }
                        }
                    }
                    return null;
                case Type._double:
                    if (!Content.IsNumeric())
                    {
                        return displayName + "格式錯誤, 應為數字。";
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if (double.Parse(Content) > maxValue)
                            {
                                return displayName + "資料錯誤, 不可大於 " + maxValue;
                            }
                        }
                        if (minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return displayName + "資料錯誤, 不可小於 " + minValue;
                            }
                        }
                    }
                    return null;
                case Type._decimal:
                    if (!Content.IsNumeric())
                    {
                        return displayName + "格式錯誤, 應為數字。";
                    }
                    else
                    {
                        if (maxValue != null)
                        {
                            if (double.Parse(Content) > maxValue)
                            {
                                return displayName + "資料錯誤, 不可大於 " + maxValue;
                            }
                        }
                        if (minValue != null)
                        {
                            if (double.Parse(Content) < minValue)
                            {
                                return displayName + "資料錯誤, 不可小於 " + minValue;
                            }
                        }
                    }
                    return null;
                case Type._twMobile:
                    Regex regex = new(@"^09[0-9]{8}$");
                    if (!regex.IsMatch(Content))
                    {
                        return "手機號碼格式錯誤，請輸入09開頭共10位數字";
                    }
                    return null;
                case Type._string:
                    //字串只要判斷是否為空白即可.

                    if (maxLength > 0)
                    {
                        if(Content.Length > maxLength)
                        {
                            return displayName + "資料錯誤, 字串長度不可大於 " + maxLength;
                        }
                    }
                    return null;

                default:
                    return "無法識別的類別: " + type.ToString();
            }
        }
    }
}
