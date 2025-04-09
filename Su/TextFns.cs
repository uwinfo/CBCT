using NPOI.POIFS.FileSystem;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Su
{
    public partial class TextFns
    {

        /// <summary>
        /// 必需為 "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\-_$.@:/ "，且不包括 ..
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetValidFilename(string value, string validFilenameCharacters = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\-_$.@:/ ")
        {
            if (value.Contains(".."))
            {
                throw new Exception("檔名中不可包含 .. ");
            }

            string newUrl = "";
            for (int i = 0; i < value.Length; i++)
            {
                var c = value.Substring(i, 1);
                int k = validFilenameCharacters.IndexOf(c);
                if (k < 0)
                {
                    throw new Exception("檔名中有非法的字元 '" + c + "'。");
                }
                newUrl += validFilenameCharacters.Substring(k, 1);
            }

            return newUrl;
        }

        public static string FilterQueryString(string url, string ParamName, bool IsQueryStringOnly = false)
        {
            string[] SAR = url.Split("?".ToCharArray());
            if (SAR.Length > 1 || IsQueryStringOnly)
            {
                string[] arrStr;
                if (IsQueryStringOnly)
                    arrStr = url.Split(Convert.ToChar("&"));
                else
                    arrStr = SAR[1].Split(Convert.ToChar("&"));


                string tmpStr = "";
                foreach (string ele in arrStr)
                {
                    if (ele.IndexOf("=") > -1)
                    {
                        if (ele.Substring(0, ele.IndexOf("=")).ToLower() != ParamName.ToLower())
                            tmpStr += ele + "&";
                    }
                }

                if (!IsQueryStringOnly)
                {
                    if (tmpStr != "")
                        return SAR[0] + "?" + tmpStr.Substring(0, tmpStr.Length - 1);
                    else
                        return SAR[0];
                }
                else if (tmpStr != "")
                    return tmpStr.Substring(0, tmpStr.Length - 1);
                else
                    return "";
            }

            // no QueryString
            return url;
        }

        /// <summary>
        /// 全英文時不做 Mask
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetMaskedName(string? name)
        {
            if (name == null || name.Contains('*'))
            {
                return name;
            }

            if (name.Length > 1 && !Regex.IsMatch(name, "^[a-zA-Z]."))
            {
                return "*" + name.Substring(1, 1) + new string('*', name.Length - 2);
            }

            return name;
        }

        public static string GetMaskedAddress(string? address)
        {
            if (address == null || address.Contains("**"))
            {
                return address;
            }

            var breakers = "巷段路街村鄉鎮區市";

            for(var i = 0; i < breakers.Length; i++)
            {
                var breaker = breakers.Substring(i, 1);

                if (address.Contains(breaker))
                {
                    return $"**{address[address.IndexOf(breaker)..]}";
                }
            }

            return address;
        }

        /// <summary>
        /// 同 GetMaskedPhone
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static string GetMaskedMobile(string mobile)
        {
            return GetMaskedPhone(mobile);
        }

        /// <summary>
        /// 保留前四碼和後兩碼
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMaskedPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return phone;
            }

            int length = phone.Length;

            if (length <= 4)
            {
                return phone; // 長度小於等於 4，直接回傳
            }
            else if (length <= 6)
            {
                // 長度 5~6，保留前 2 碼 + * + 最後 2 碼
                return phone.Substring(0, 2) + new string('*', length - 4) + phone.Substring(length - 2);
            }
            else
            {
                // 正常情況，保留前 4 碼 + **** + 後 2 碼
                return phone.Substring(0, 4) + new string('*', length - 6) + phone.Substring(length - 2);
            }
        }

        /// <summary>
        /// 保留第一個字母和 @ 之後的網域
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetMaskedEmail(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2)
            {
                return value;
            }

            if (value.IndexOf("@") == -1)
            {
                return value;
            }

            return value[..1] + "**" + value[value.IndexOf("@")..];
        }

        public static string GetMaskedBirthday(string birthday)
        {
            if (birthday == null || birthday.Contains("*"))
            {
                return birthday;
            }

            if (birthday.Length == 8)
            {
                return "****" + birthday.Substring(4, 2) + "**";
            }

            return birthday;
        }

        public static bool IsEmail(string Email)
        {
            if (Email == null || Email.Length == 0)
            {
                return false;
            }

            //偷改一下, 可以用 3 結尾
            string strPattern = "^[\\w\\.-]{1,}@[a-zA-Z0-9][\\w\\.-]*\\.[a-zA-Z][a-zA-Z\\.]*[a-zA-Z3]$";
            System.Text.RegularExpressions.Regex objReg = new System.Text.RegularExpressions.Regex(strPattern);
            return objReg.IsMatch(Email);
        }

        /// <summary>
        /// AAA,BB,,C,,,D'xx  傳回 'AAA','BB','C','D''xx', 會做 ' 的取代, 略過空白項目
        /// </summary>
        /// <param name="OriginalStr"></param>
        /// <returns></returns>
        public static string GetSqlStingList(string OriginalStr)
        {
            if (OriginalStr == null || OriginalStr.Length == 0)
            {
                return "";
            }

            string[] STRs = OriginalStr.Split(',');
            string Res = "";
            foreach (string str in STRs)
            {
                if (str.Length > 0)
                {
                    Res = Res + ",'" + str.Replace("'", "''") + "'";
                }
            }

            if (Res.Length > 0)
            {
                return Res.Substring(1);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 把多個字串用 concater 合併在一起
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string Concat(string concater, params string[] strings )
        {
            string res = "";
            foreach(string str in strings)
            {
                res = res.Attach(str, concater);
            }

            return res;
        }

        /// <summary>
        /// 非整數和空白自動移除. ex: 傳入: ,A,B,1,2,3,4,5,,, 傳出: 1,2,3,4,5
        /// </summary>
        /// <param name="OriginalStr"></param>
        /// <returns></returns>
        public static string GetIntList(string OriginalStr)
        {
            if (OriginalStr == null || OriginalStr.Length == 0)
            {
                return "";
            }

            string[] Ints = OriginalStr.Split(',');
            string Res = "";
            foreach (string strInt in Ints)
            {
                if (strInt.Length > 0 && IsNumeric(strInt))
                {
                    Res = Res + "," + Int64.Parse(strInt);
                }
            }

            if (Res.Length > 0)
            {
                return Res.Substring(1);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 這裡會叫用 Double.TryParse, 若之後要轉換形別, 直接用 Double.TryParse 比較快.
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(object Expression)
        {
            return Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out double retNum);
        }

        /// <summary>
        /// 這裡會叫用 int.TryParse, 若之後要轉換形別, 直接用 int.TryParse 比較快.
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsInt(object Expression)
        {
            return int.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out int retNum);
        }

        /// <summary>
        /// 這裡會叫用 int.TryParse, 若之後要轉換形別, 直接用 int.TryParse 比較快.
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsInt64(object Expression)
        {
            return Int64.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out Int64 retNum);
        }

        /// <summary>
        /// 這裡叫用 DateTime.TryParse, 若之後要轉換形別, 直接用 DateTime.TryParse 比較快.
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsDate(string Expression)
        {
            return DateTime.TryParse(Expression, out DateTime retDate);
        }

        /// <summary>
        /// 檢查是否符合 hh:mm 的格式
        /// </summary>
        /// <param name="hhmm"></param>
        /// <returns></returns>
        public static bool IsValidHhmm(string hhmm)
        {
            if (string.IsNullOrEmpty(hhmm))
            {
                return false;
            }

            var hmArray = hhmm.Split(":");
            if (hmArray.Length != 2
                || !hmArray[0].IsInt()
                || !hmArray[1].IsInt())
            {
                return false;
            }

            var hour = hmArray[0].ToInt32();
            var minutes = hmArray[1].ToInt32();

            if(hour < 0 || hour >= 24)
            {
                return false;
            }

            if (minutes < 0 || minutes >= 60)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 是否有 XSS 的危險性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsXssDanger(object source)
        {
            var sourceProps = source.GetType().GetProperties().Where(x => x.CanRead).ToList();
            foreach (var srcItem in sourceProps)
            {
                var property = srcItem.GetValue(source);

                if(property != null && property is string && IsDangerScript(property.ToString()))
                {
                    return true;
                }
            }

            var sourceFields = source.GetType().GetFields().ToList();
            foreach (var srcItem in sourceFields)
            {
                var field = srcItem.GetValue(source);

                if (field != null && field is string && IsDangerScript(field.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否包括危險的 javascript
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dangerWords"></param>
        /// <returns></returns>
        public static bool IsDangerScript(string s, string dangerWords = "<script,document.,javascript:,<iframe,<object,<applet,<embed")
        {
            var l = s.ToLower();

            foreach(string w in dangerWords.Split(','))
            {
                if (l.Contains(w))
                {
                    return true;
                }
            }

            return false;
        }

        //static Random _oRandomGenerator = new Random();
        static char[] _SourceCharacters = "23456789abcdefghijkmnpqrstwxyzABCDEFGHJKLMNPQRSTWXYZ".ToCharArray();
        /// <summary>
        /// 數字和字母組成的隨機碼, 沒有 0, 1, O, o, l, U, V, u, v 這幾個字元
        /// </summary>
        /// <param name="CodeLength"></param>
        /// <returns></returns>
        public static String GetRandomString(Int32 CodeLength, string? sourceCharacters = null)
        {
            var SourceCharacters = sourceCharacters == null ? _SourceCharacters : sourceCharacters.ToCharArray();

            //生成驗證碼字串
            String sCode = "";
            for (int i = 0; i < CodeLength; i++)
            {
                sCode += SourceCharacters[MathUtil.GetRandomInt(SourceCharacters.Length)];
            }
            return sCode;
        }

        /// <summary>
        /// 傳回值不包括 "."
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string FileExt(string FileName)
        {
            if (FileName.LastIndexOf(".") > -1 && FileName.LastIndexOf(".") < FileName.Length - 2)
            {
                return Mid(FileName, FileName.LastIndexOf(".") + 1);
            }
            return "";
        }

        public static string Mid(string param, int startIndex)
        {
            return param.Substring(startIndex);
        }

        public static string Mid(string param, int startIndex, int length)
        {
            return param.Substring(startIndex, length);
        }

        /// <summary>
        /// 找出用 HTML 註解 XX S, XX E 區隔的區塊, 回傳字串不包括註解.
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Key"></param>
        /// <param name="IsReturnEmptyString">若為 false 時, 找不到會丟出 Excption. 若為 true 時, 找不到會回傳 null.</param>
        /// <returns></returns>
        public static string GetBlock(string Source, string Key, bool IsReturnEmptyString = true)
        {
            string StartKey = "<!--" + Key + " S-->"; // 再精簡一點
            string EndKey = "<!--" + Key + " E-->";

            int StartP = Source.IndexOf(StartKey);
            if (StartP == -1)
            {
                if (IsReturnEmptyString)
                {
                    return null;
                }
                else
                {
                    throw new Exception("Can not find start position for block !! " + StartKey + " | " + Source);
                }
            }

            // 跳到註解的下一個字元
            StartP = StartP + StartKey.Length;

            int EndP = Source.IndexOf(EndKey, StartP);
            if (EndP == -1)
            {
                if (IsReturnEmptyString)
                {
                    return null;
                }
                else
                {
                    throw new Exception("Can not find start position for block !! " + StartKey + " | " + Source);
                }
            }

            return Source.Substring(StartP, EndP - StartP);

            //if (IsFastBuild || IsRemoveTemplate)
            //    Source = Source.Substring(0, StartP) + Source.Substring(EndP);// 保留註解, 做為把 Template 放回來的位置

            //return Template;
        }


        /// <summary>
        /// 字串-半型轉全型
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ToWide(string txt)
        {
            char[] c = txt.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                //全形空格為12288，半形空格為32
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] < 127)
                {
                    c[i] = (char)(c[i] + 65248);
                }
            }
            return new string(c);
        }

        static readonly char[] _SourceCharacters28Arr = "ABCDEFGHJKLMNPQRTUVWXY346789".ToCharArray();
        static readonly string _SourceCharacters28 = "ABCDEFGHJKLMNPQRTUVWXY346789";
        static readonly string _SourceCharacters53 = "ABCDEFGHJKLMNPQRTUVWXY346789abcdefghijkmnopqrstuvwxyz";
        static readonly char[] _SourceCharacters53Arr = "ABCDEFGHJKLMNPQRTUVWXY346789abcdefghijkmnopqrstuvwxyz".ToCharArray();

        public static string GetShortUrlEncode(long OriNumber)
        {
            if (OriNumber < 28 * 28)
            {
                return Base28Encode(OriNumber, 1);
            }
            else
            {
                string res = "";
                int baseNumber = _SourceCharacters53.Length;
                while (OriNumber > 0)
                {
                    Int32 R = Convert.ToInt32(OriNumber % baseNumber);
                    res = _SourceCharacters53Arr[R] + res;
                    OriNumber = (OriNumber - R) / baseNumber;
                }

                // 加上 - 作為 28*28 ~ (53*53-1) 時期過渡用
                if (res.Length <= 2)
                {
                    res = res + "Z";
                }
                return res;
            }
        }

        public static long GetShortUrlDecode(string encodedStr)
        {
            if (encodedStr.Length <= 2)
            {
                return Base28Decode(encodedStr);
            }
            if (encodedStr.Length == 3 && encodedStr.EndsWith("Z"))
            {
                encodedStr = encodedStr[..2];
            }
            int baseNumber = _SourceCharacters53.Length;
            var arr = encodedStr.ToCharArray();
            long res = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                int k = _SourceCharacters53.IndexOf(arr[i]);
                if (k == -1)
                {
                    throw new Exception($"illegal character: {arr[i]}, encodedStr: {encodedStr}");
                }
                res = res * baseNumber + k;
            }

            return res;
        }

        /// <summary>
        /// 用 346789ABCDEFGHJKLMNPQRTUVWXY 來編碼數字, (去除 0, 1, I, O, 2, Z, 5, S)
        /// </summary>
        /// <param name="OriNumber"></param>
        /// <param name="TotalLength"></param>
        /// <returns></returns>
        public static string Base28Encode(long OriNumber, int TotalLength)
        {
            string res = "";
            int baseNumber = _SourceCharacters28.Length;

            while (OriNumber > 0)
            {
                Int32 R = Convert.ToInt32(OriNumber % baseNumber);
                res = _SourceCharacters28Arr[R] + res;

                OriNumber = (OriNumber - R) / baseNumber;
            }

            if(res.Length < TotalLength)
            {
                return res.PadLeft(TotalLength, _SourceCharacters28Arr[0]);
            }

            return res;
        }

        public static long Base28Decode(string encodedStr)
        {
            int baseNumber = _SourceCharacters28.Length;

            var arr = encodedStr.ToCharArray();
            long res = 0;
            for(int i = 0; i < arr.Length; i++)
            {
                int k = _SourceCharacters28.IndexOf(arr[i]);
                if(k == -1)
                {
                    throw new Exception("illegal character");
                }
                res = res * baseNumber + k;
            }

            return res;
        }


        /// <summary>
        /// 字串-全型轉半型
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ToNarrow(string txt)
        {
            char[] c = txt.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                //全形空格為12288，半形空格為32
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                //其他字元半形(33-126)與全形(65281-65374)的對應關係是：均相差65248
                if (c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }
                
        /// <summary>
        /// 移除 HTML 註解 "XXX S" "XXX E"之間的文字，不支援巢狀，會出錯。
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="BlockKey"></param>
        /// <param name="IsThrowExceptionIfNotFound"></param>
        /// <returns></returns>
        public static string RemoveBlock(string Content, string BlockKey, bool IsThrowExceptionIfNotFound = false)
        {
            string StartKey = "<!--" + BlockKey + " S-->";
            string EndTag = " E-->";
            Int32 SP = Content.IndexOf(StartKey, System.StringComparison.OrdinalIgnoreCase);
            if (SP == -1)
            {
                StartKey = "<!--" + BlockKey + "_Start-->";
                EndTag = "_End-->";
                SP = Content.IndexOf(StartKey, System.StringComparison.OrdinalIgnoreCase);
            }
            if (SP == -1)
            {
                StartKey = "<!--" + BlockKey + " Start-->";
                EndTag = " End-->";
                SP = Content.IndexOf(StartKey, System.StringComparison.OrdinalIgnoreCase);
            }

            if (SP == -1)
            {
                if (IsThrowExceptionIfNotFound)
                    throw new Exception("RemoveBlock2: " + BlockKey + " not found !!");
                else
                    return Content;
            }

            string EndKey = "<!--" + BlockKey + EndTag;
            Int32 EP = Content.IndexOf(EndKey, SP + 9, System.StringComparison.OrdinalIgnoreCase);

            if (IsThrowExceptionIfNotFound && EP == -1)
                throw new Exception("RemoveBlock2: " + BlockKey + " end position not found !!");

            // UW.WU.DebugWriteLine("StartKey, EndKey  " & Current.Server.HtmlEncode(StartKey) & ", " & Current.Server.HtmlEncode(EndKey))
            // UW.WU.DebugWriteLine("Found SP, EP  " & SP & ", " & EP)
            Int32 KeyLength = (EndKey).Length;
            while (SP >= 0 && EP > 0)
            {
                // UW.WU.DebugWriteLine("SP, EP  " & SP & ", " & EP)
                if (SP > EP)
                    throw new Exception("RemoveBlock2 Exception, " + BlockKey + " 起始位置大於結束位置 !!");

                if ((EP + KeyLength) < Content.Length - 1)
                    Content = Content.Substring(0, SP) + Content.Substring(EP + KeyLength);
                else
                    Content = Content.Substring(0, SP);

                SP = Content.IndexOf(StartKey, System.StringComparison.OrdinalIgnoreCase);
                EP = Content.IndexOf(EndKey, System.StringComparison.OrdinalIgnoreCase);
            }

            return Content;
        }
    }
}
