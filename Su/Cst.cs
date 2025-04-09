using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    namespace Cst
    {
        /// <summary>
        /// 命名原則: 分類_名稱
        /// 請不要再增加類別，以免搜尋困難。
        /// </summary>
        public class ErrorMessage
        {
            public const string SomeFieldIsRequired = "請填寫所有必填欄位";

            public const string TwMobileError = "手機號碼格式錯誤，必需為 09 開頭 10 位數字。";

            public const string MobileEncFail = "手機號碼無法加密儲存";

            public const string UsernameDuplicate = "使用者帳號重覆";

            public const string MobileDuplicate = "手機號碼重覆";

            public const string MustHaveOneSocialMediaUrl = "社群網址擇一必填";

            public const string OldDataNotFound = "找不到舊資料";

            public const string MarketNotFound = "找不到賣場";

            public const string EmailFormatError = "Email 格式錯誤";

            public const string EmailDuplicate = "Email 重覆";

            public const string CombinationIdError = "組合編號有誤";

            public const string UnAuthorized = "權限不足";

            public const string EndAtShouldBiggerThenStartAt = "結束時間應大於開始時間";

            public const string PriceShouldGreaterThenZero = "價格應大於 0";

            public const string CaptchaError = "驗證碼錯誤。";

            public const string LoginFirst = "請先登入。";

            public const string ConfrimSecretNotTheSame = "確認密碼錯誤。";
        }

    }

}
