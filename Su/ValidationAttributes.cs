using Su.Cst;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Su
{
    public interface IValidationChecker
    {
        public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null);
    }

    namespace ValidationAttributes
    {
        /// <summary>
        /// 多個日期區間
        /// 允許 null 或空字串
        /// 必需為 yyyy-MM-dd~yy-MM-dd, yyyy-MM-dd~yy-MM-dd 的格式
        /// </summary>
        public class DayPeriodsValidation() : Attribute, IValidationChecker
        {
            /// <summary>
            /// 驗証
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            /// <param name="prefix"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value == null)
                {
                    return;
                }

                if (value is string stringValue && (string.IsNullOrEmpty(stringValue) ||
                    ValidationHelper.CheckMutipleDayPeriod(stringValue)))
                {
                    return;
                }

                ex.AddValidationError(propertyInfo.Name, "日期段落格式錯誤，請使用以下範例的格式: 2024-01-05~2024-01-03,2024-01-06 ", index, prefix);
            }
        }


        /// <summary>
        /// 多個時間區間
        /// 允許 null 或空字串
        /// 必需為 HH:mm~HHmm,HH:mm~HHmm 的格式
        /// </summary>
        public class TimePeriodsValidation() : Attribute, IValidationChecker
        {
            /// <summary>
            /// 驗証
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value == null)
                {
                    return;
                }

                if (value is string stringValue && (string.IsNullOrEmpty(stringValue) || ValidationHelper.CheckMutipleTimePeriod(stringValue)))
                {
                    return;
                }

                ex.AddValidationError(propertyInfo.Name, "時間段落格式錯誤，請使用以下範例的格式: 09:15~12:30,14:00~18:45", index, prefix);
            }
        }

        /// <summary>
        /// 比對相同字串
        /// </summary>
        /// <param name="secretPropertyName"></param>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class ConfirmSecretValidation(string secretPropertyName, bool isRequire = false) : Attribute, IValidationChecker
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly string _secretPropertyName = secretPropertyName;

            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 最少必需包含英數字
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                var secret = (string?)dto.GetValue(_secretPropertyName);
                if (string.IsNullOrEmpty(secret))
                {
                    return;
                }

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (secret == stringValue)
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, ErrorMessage.ConfrimSecretNotTheSame, index, prefix);
                }
                else
                {
                    //若 secret 有填寫，則 confirm secret 也必填
                    ex.AddValidationError(propertyInfo.Name, ErrorMessage.ConfrimSecretNotTheSame, index, prefix);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minLength">最小長度</param>
        /// <param name="maxLength">最大長度</param>
        /// <param name="isUpperAndLower">包含大小寫</param>
        /// <param name="isSymbol">包含符號</param>
        /// <param name="isRequire">必填</param>
        [AttributeUsage(AttributeTargets.Property)]
        public class SecretValidation(int minLength = 8, int maxLength = 32, bool isUpperAndLower = false, bool isSymbol = false, bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;
            readonly int _minLength = minLength;
            readonly int _maxLength = maxLength;
            readonly bool _isUpperAndLower = isUpperAndLower;
            readonly bool _isSymbol = isSymbol;


            /// <summary>
            /// 最少必需包含英數字
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.Length < _minLength)
                    {
                        ex.AddValidationError(propertyInfo.Name, $"密碼長度最少 {_minLength} 個字元", index, prefix);
                    }

                    if (stringValue.Length > _maxLength)
                    {
                        ex.AddValidationError(propertyInfo.Name, $"密碼長度限 {_maxLength} 個字元", index, prefix);
                    }

                    // 包含英文字母
                    if (!Regex.Match(stringValue.ToLower(), "[a-z]", RegexOptions.ECMAScript).Success)
                    {
                        ex.AddValidationError(propertyInfo.Name, "密碼應包含英文字母", index, prefix);
                    }

                    // 包含數字
                    if (!Regex.Match(stringValue, @"[0-9]", RegexOptions.ECMAScript).Success)
                    {
                        ex.AddValidationError(propertyInfo.Name, "密碼應包含數字", index, prefix);
                    }

                    // 同時包含大小寫字母
                    if (_isUpperAndLower && !(Regex.Match(stringValue, "[a-z]", RegexOptions.ECMAScript).Success && Regex.Match(stringValue, "[A-Z]", RegexOptions.ECMAScript).Success))
                    {
                        ex.AddValidationError(propertyInfo.Name, "密碼應同時包含大小寫字母", index, prefix);
                    }

                    //包含符號
                    if (_isSymbol && !Regex.Match(stringValue, @"[~!@#$%^&*()\-+=\[{\]};:<>|./?,_`'""]", RegexOptions.ECMAScript).Success)
                    {
                        ex.AddValidationError(propertyInfo.Name, "密碼應同時包含符號", index, prefix);
                    }

                    return;
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix: prefix);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class EmailValidation(bool isRequire = false, bool isCheckDomain = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;
            readonly bool _isCheckDomain = isCheckDomain;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsEmail(_isCheckDomain))
                    {
                        return;
                    }

                    if (stringValue.IsBadDomain())
                    {
                        ex.AddValidationError(propertyInfo.Name, "email 格式錯誤。email 應為全小寫，且不可包含全型文字。", index, prefix);

                    }
                    else
                    {
                        ex.AddValidationError(propertyInfo.Name, "email 格式錯誤。email 應為全小寫，且不可包含全型文字。", index, prefix);

                    }
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        //[AttributeUsage(AttributeTargets.Property)]
        //public class ServiceItemUidValidation(bool isRequire = false) : Attribute, IValidationChecker
        //{
        //    readonly bool _isRequire = isRequire;

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="propertyInfo"></param>
        //    /// <param name="dto"></param>
        //    /// <param name="ex"></param>
        //    /// <param name="index"></param>
        //    public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
        //    {
        //        var value = propertyInfo.GetValue(dto, null);

        //        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        //        {
        //            if (ValidationHelper.IsServiceItemUid(Core.NewDbContext.JdcardContext, stringValue))
        //            {
        //                return;
        //            }

        //            ex.AddValidationError(propertyInfo.Name, "無法識別的 Service Uid: " + stringValue, index, prefix);
        //        }

        //        //未填寫
        //        ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
        //    }
        //}


        //[AttributeUsage(AttributeTargets.Property)]
        //public class StoreUidValidation(bool isRequire = false) : Attribute, IValidationChecker
        //{
        //    readonly bool _isRequire = isRequire;

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="propertyInfo"></param>
        //    /// <param name="dto"></param>
        //    /// <param name="ex"></param>
        //    /// <param name="index"></param>
        //    public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
        //    {
        //        var value = propertyInfo.GetValue(dto, null);

        //        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        //        {
        //            if (Core.Helpers.ValidationHelper.IsStoreUid(Core.NewDbContext.JdcardContext, stringValue))
        //            {
        //                return;
        //            }

        //            ex.AddValidationError(propertyInfo.Name, "無法識別的 Store Uid: " + stringValue, index, prefix);
        //        }

        //        //未填寫
        //        ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
        //    }
        //}


        //[AttributeUsage(AttributeTargets.Property)]
        //public class MemberUidValidation(bool isRequire = false) : Attribute, IValidationChecker
        //{
        //    readonly bool _isRequire = isRequire;

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="propertyInfo"></param>
        //    /// <param name="dto"></param>
        //    /// <param name="ex"></param>
        //    /// <param name="index"></param>
        //    public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
        //    {
        //        var value = propertyInfo.GetValue(dto, null);

        //        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        //        {
        //            if (Core.Helpers.ValidationHelper.IsMemberUid(Core.NewDbContext.JdcardContext, stringValue))
        //            {
        //                return;
        //            }

        //            ex.AddValidationError(propertyInfo.Name, "無法識別的 Member Uid: " + stringValue, index, prefix);
        //        }

        //        //未填寫
        //        ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
        //    }
        //}

        //[AttributeUsage(AttributeTargets.Property)]
        //public class CompanyUidValidation(bool isRequire = false) : Attribute, IValidationChecker
        //{
        //    readonly bool _isRequire = isRequire;

        //    /// <summary>
        //    /// 
        //    /// </summary>
        //    /// <param name="propertyInfo"></param>
        //    /// <param name="dto"></param>
        //    /// <param name="ex"></param>
        //    /// <param name="index"></param>
        //    public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
        //    {
        //        var value = propertyInfo.GetValue(dto, null);

        //        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        //        {
        //            var admin = Core.Helpers.CompanyAdminClaimHelper.LoginAdminFullInfo();
        //            if (stringValue == admin?.CompanyUid)
        //            {
        //                return;
        //            }

        //            ex.AddValidationError(propertyInfo.Name, "CompanyUid 錯誤", index, prefix);
        //        }

        //        //未填寫
        //        ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
        //    }
        //}

        /// <summary>
        /// 台灣統一編號
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class TwTaxIdValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsTaxNo())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "編一編號格式錯誤，應為 8 位數字，且符合驗証碼規則。", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 檢查電話是否正確(開頭須為+，並且不能包含其他符號)
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class PhoneValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    string mobilePattern = @"^\+8869\d{8}$"; // 台灣手機
                    string landlinePattern = @"^\+886[2-8]\d{5,7}$"; // 台灣市話
                    string internationalPattern = @"^\+\d{1,4}\d{6,12}$"; // 國際電話 (允許其他國碼)

                    if (Regex.IsMatch(stringValue, mobilePattern))
                    {
                        //台灣手機號碼
                        return;
                    }

                    if (Regex.IsMatch(stringValue, landlinePattern))
                    {
                        //台灣市話號碼
                        return;
                    }

                    if (!Regex.IsMatch(stringValue, internationalPattern))
                    {
                        //國際電話號碼
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "非有效的電話格式", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class TwMobileValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsMobile())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "手機號碼格式錯誤，應為 09 開頭的 10 位數字。", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 郵遞區號格式檢查，應為 3~6 位數字。
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class TwZipValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsTwZip())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "郵遞區號格式錯誤，應為 3~6 位數字。", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 必填欄位，不允許 null 或空字串
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class RequireValidation() : Attribute, IValidationChecker
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value == null
                    || value is string stringValue && string.IsNullOrEmpty(stringValue))
                {
                    // string pname = IsDefined(propertyInfo, typeof(DescriptionAttribute), false) ? (GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute)) as DescriptionAttribute).Description : propertyInfo.Name;
                    ex.AddRequireField(propertyInfo.Name, index, prefix: prefix);
                }
            }
        }

        /// <summary>
        /// 必填欄位，只允許 Y 和 N
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class YnValidation() : Attribute, IValidationChecker
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value != null && value is char charValue)
                {
                    if (charValue != 'Y' && charValue != 'N')
                    {
                        ex.AddValidationError(propertyInfo.Name, "只允許 Y 和 N", index, prefix);
                        return;
                    }
                    return;
                }

                if (value != null && value is string stringValue)
                {
                    if (stringValue != "Y" && stringValue != "N")
                    {
                        ex.AddValidationError(propertyInfo.Name, "只允許 Y 和 N", index, prefix);
                        return;
                    }
                    return;
                }

                ex.AddRequireField(propertyInfo.Name, index, prefix: prefix);
            }
        }

        /// <summary>
        /// 允許特定的整數
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class EnumValidation(params int[] list) : Attribute, IValidationChecker
        {
            readonly int[] _List = list;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is int intValue)
                {
                    if (_List.Contains(intValue))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, $"內容錯誤，應為 {_List.Select(i => i.ToString()).ToOneString(", ")} 之一的數字");
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, false, prefix);
            }
        }

        /// <summary>
        /// 可轉型為 Enum T
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class EnumTypeValidation(Type T, bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly Type _T = T;
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is int intValue)
                {
                    if (Enum.IsDefined(_T, intValue))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "內容錯誤");
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 在 client 端顯示的名稱
        /// </summary>
        /// <param name="name"></param>
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public class ValidationName(string name) : Attribute
        {
            /// <summary>
            /// 
            /// </summary>
            public string Name { get; set; } = name;
        }

        /// <summary>
        /// 限定範圍的數字 下限(包含) ~ 上限(包含)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullMessage"></param>
        /// <param name="lowerBound">可接受的下限(包含)，Null 時不檢查</param>
        /// <param name="upBound">可接受的上限(包含)，Null 時不檢查</param>
        [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        public class RangeNumberValidation(string? name = null, string? fullMessage = null, double lowerBound = double.MinValue, double upBound = double.MaxValue) : Attribute, IValidationChecker
        {
            /// <summary>
            /// 
            /// </summary>
            public string? Name { get; set; } = name;
            /// <summary>
            /// 
            /// </summary>
            public string? FullMessage { get; set; } = fullMessage;

            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value == null)
                {
                    return; //非 Null 的檢查應該放在其它地方。
                }

                switch (value.GetType().ToString())
                {
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Decimal":
                    case "System.Single":
                    case "System.Double":
                        {
                            if (Convert.ToDouble(value) < lowerBound)
                            {
                                ex.AddValidationError(propertyInfo.Name,
                                    string.IsNullOrEmpty(FullMessage) ? $"{ValidationHelper.GetDisplayName(Name, propertyInfo)}必需大於或等於 {lowerBound}" : FullMessage,
                                    index);
                            }

                            if (Convert.ToDouble(value) > upBound)
                            {
                                ex.AddValidationError(propertyInfo.Name,
                                    string.IsNullOrEmpty(FullMessage) ? $"{ValidationHelper.GetDisplayName(Name, propertyInfo)}必需小於或等於 {upBound}" : FullMessage,
                                    index);
                            }
                        }

                        break;
                    default:
                        ex.AddValidationError(propertyInfo.Name, "非數字的型別: " + value.GetType().ToString(), index, prefix);
                        break;
                }
            }
        }

        /// <summary>
        /// 是否為整點字串(00:00 ~ 23:00) (00:00:00 ~ 23:00:00) 
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class HourValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 驗証是不是正常的日期，不含時間 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.Length == 5 && stringValue.EndsWith(":00") && stringValue.IsTime24())
                    {
                        return;
                    }

                    if (stringValue.Length == 8 && stringValue.EndsWith(":00:00") && stringValue.IsTime24())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "時間格式錯誤", index, prefix);
                    return;
                }

                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 是否為 24 小時制的時間 00:00 ~ 23:59:59.9999999  
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class Time24Validation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 是否為 24 小時制的時間 00:00 ~ 23:59:59.9999999  
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsTime24())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "時間格式錯誤", index, prefix);
                }

                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 驗証是不是正常的日期，不含時間
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class DayValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 驗証是不是正常的日期，不含時間 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsDay())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "日期格式錯誤", index, prefix);
                }

                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 驗証是不是正常的日期+時間
        /// 另外有 DayValidation 和 Time24Validation
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class DatetimeValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 驗証是不是正常的日期+時間
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (stringValue.IsDateTime())
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "日期時間的格式錯誤", index, prefix);
                }

                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        public class BirthdayValidation() : Attribute, IValidationChecker
        {
            /// <summary>
            /// 驗証是不是正常的生日
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value == null)
                {
                    return;
                }

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    DateTime dt = default(DateTime);
                    if (DateTime.TryParse(stringValue, out dt) && dt.IsBetweenDate(DateTime.Parse("1900-01-01"), DateTime.Today))
                    {
                        return;
                    }
                }

                if (value is Nullable<DateTime> || value is DateTime)
                {
                    DateTime dt = (DateTime)value;
                    if (dt.IsBetweenDate(DateTime.Parse("1900-01-01"), DateTime.Today))
                    {
                        return;
                    }
                }

                if (value is Nullable<DateOnly> || value is DateOnly)
                {
                    DateOnly dateOnly = (DateOnly)value;
                    DateTime dt = dateOnly.ToDateTime(TimeOnly.Parse("00:00"));
                    if (dt.IsBetweenDate(DateTime.Parse("1900-01-01"), DateTime.Today))
                    {
                        return;
                    }
                }

                ex.AddValidationError(propertyInfo.Name, "生日日期錯誤", index, prefix);
            }
        }

        /// <summary>
        /// 檢查輸入值是不是在白名單
        /// </summary>
        /// <param name="whiteList">請用逗號分開白名單</param>
        /// <param name="isRequire">是否必填</param>
        [AttributeUsage(AttributeTargets.Property)]
        public class WhiteListValidation(string whiteList, bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly string _whiteList = whiteList;
            readonly bool _isRequire = isRequire;

            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (_whiteList.Split(',').Contains(stringValue))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "輸入值不在白名單裡", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 字串型白名單檢查
        /// </summary>
        /// <param name="whiteList">請帶入字串白名單 如 "A", "B", "C"</param>
        [AttributeUsage(AttributeTargets.Property)]
        public class StringWhiteListValidation(params string[] whiteList) : Attribute, IValidationChecker
        {
            readonly string[] _whiteList = whiteList;

            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    if (_whiteList.Contains(stringValue))
                    {
                        return;
                    }
                    ex.AddValidationError(propertyInfo.Name, "輸入值不在白名單裡", index, prefix);
                }
            }
        }

        /// <summary>
        /// 整數型白名單檢查，若有設定Enum請使用 EnumTypeValidation 
        /// </summary>
        /// <param name="whiteList">請帶入整數白名單 如 1, 2, 3</param>
        [AttributeUsage(AttributeTargets.Property)]
        public class IntWhiteListValidation(params int[] whiteList) : Attribute, IValidationChecker
        {
            readonly int[] _whiteList = whiteList;

            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is int || value is int?)
                {
                    int myValue = (int)value;
                    if (_whiteList.Contains(myValue))
                    {
                        return;
                    }
                    ex.AddValidationError(propertyInfo.Name, "輸入整數值不在白名單裡", index, prefix);
                }
            }
        }

        /// <summary>
        /// 使用regular expression檢查
        /// </summary>
        /// <param name="patten"></param>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class RegExpValidation(string patten, bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly string _patten = patten;
            readonly bool _isRequire = isRequire;

            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);
                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    System.Text.RegularExpressions.Regex regex = new(_patten);
                    if (regex.IsMatch(stringValue))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "輸入值不合法", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 檢查檔案類型是否為白名單
        /// </summary>
        /// <param name="isRequire"></param>
        /// <param name="whiteList"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class FileExtValidation(string whiteList, bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;
            readonly string _whiteList = whiteList;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {
                    var extension = Path.GetExtension(stringValue);
                    if (extension.Contains("?"))
                    {
                        extension = extension.Split('?')[0];
                    }
                    string fileType = extension.TrimStart('.').ToLowerInvariant();
                    if (_whiteList.Split(',').Contains(fileType))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "檔案類型不在白名單裡", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }

        /// <summary>
        /// 檢查網址格式
        /// </summary>
        /// <param name="isRequire"></param>
        [AttributeUsage(AttributeTargets.Property)]
        public class UrlValidation(bool isRequire = false) : Attribute, IValidationChecker
        {
            readonly bool _isRequire = isRequire;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="propertyInfo"></param>
            /// <param name="dto"></param>
            /// <param name="ex"></param>
            /// <param name="index"></param>
            public void CheckValue(PropertyInfo propertyInfo, object dto, CustomException ex, int? index = null, string? prefix = null)
            {
                var value = propertyInfo.GetValue(dto, null);

                if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                {

                    string pattern = @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";
                    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(stringValue))
                    {
                        return;
                    }

                    ex.AddValidationError(propertyInfo.Name, "網址格式異常", index, prefix);
                }

                //未填寫
                ex.AddRequireField(propertyInfo.Name, index, _isRequire, prefix);
            }
        }
    }
}

