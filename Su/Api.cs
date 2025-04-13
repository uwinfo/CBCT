namespace Su
{
    public class Api
    {
        ///// <summary>
        ///// 預設不會加入 test 用的 TimeMark
        ///// </summary> 
        //public static bool IsAddTestTimeMarkOnly = false;

        //public static string TimeMarkKey = "API_TimeMark";
        ///// <summary>
        ///// 這個 function 不能在 Thread 中被呼叫, 發生 Exception 不會處理.
        ///// 預設只有在 test 的環境才會加入 TimeMark.
        ///// 計得在 Global.asax 把 IsAddTestTimeMark 打開.
        ///// </summary>
        ///// <param name="Mark"></param>
        ///// <param name="IsTestOnly"></param>
        //static public void AddTimeMark(string Mark, bool IsTestOnly = true)
        //{
        //    //U2.FileLogger.AppendDailyLog(@"D:\ASP.NET Project\Disonic\生活工場\Data\OneDayLog", "AddTimeMark: " + Mark, 3, "AddTimeMark");
        //    if ((!IsAddTestTimeMarkOnly) || (!IsTestOnly))
        //    {
        //        //U2.FileLogger.AppendDailyLog(@"D:\ASP.NET Project\Disonic\生活工場\Data\OneDayLog", "Mark 1", 3, "AddTimeMark");

        //        try //避免在 Tread 中被呼叫.
        //        {
        //            if (CurrentContext.Current.Items[TimeMarkKey] == null)
        //            {
        //                CurrentContext.Current.Items[TimeMarkKey] = new List<string>();
        //            }

        //            ((List<string>)CurrentContext.Current.Items[TimeMarkKey]).Add(DateTime.Now.ToString("HH:mm:ss.fff") + ", " + Mark);

        //            //U2.FileLogger.AppendDailyLog(@"D:\ASP.NET Project\Disonic\生活工場\Data\OneDayLog", "Mark 2", 3, "AddTimeMark");
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //}

        //static public string TimeMark()
        //{
        //    if (CurrentContext.Current != null)
        //    {
        //        if (CurrentContext.Current.Items[TimeMarkKey] != null)
        //        {
        //            return string.Join("\r\n", (List<string>)CurrentContext.Current.Items[TimeMarkKey]);
        //        }
        //    }

        //    return "";
        //}

        //public class ReturnObject
        //{
        //    /// <summary>
        //    /// ReturnCode 的縮寫
        //    /// 0 表示 OK, 其它自定義.
        //    /// </summary>
        //    public int Rc { get; set; }

        //    public string Msg { get; set; }
        //    public object Data { get; set; }

        //    public string Source { get; set; }
        //    public string TimeMark
        //    {
        //        get
        //        {
        //            return Su.Api.TimeMark();
        //        }
        //    }
        //    public object DebugData { get; set; }
        //}

        ///// <summary>
        ///// 傳入 API 的物件為已知形別時, 用這個.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T RequestObject<T>()
        //{
        //    return RequestString.JsonDeserialize<T>();
        //}

        ///// <summary>
        ///// 傳入為 anonymous object 時, 用這個. 若有非基本型別的屬性時, 要先轉為一般物件, 再轉 json, 再 Deserialize 一次.
        ///// </summary>
        //public static dynamic RequestDynamicObject
        //{
        //    get
        //    {
        //        return RequestObject<dynamic>();
        //    }
        //}

        //public static JsonResult SuccessMessage(string message)
        //{
        //    return new JsonResult(new ReturnObject
        //    {
        //        Rc = 0,
        //        Msg = message
        //    });
        //}

        //public static JsonResult SuccessObject(object data, string msg, JsonSerializerSettings settings)
        //{
        //    return new Microsoft.AspNetCore.Mvc.JsonResult(new ReturnObject
        //    {
        //        Rc = 0,
        //        Msg = msg,
        //        Data = data
        //    }, settings);
        //}

        //public static JsonResult SuccessObject(object data, JsonSerializerSettings settings)
        //{
        //    return SuccessObject(data, "", settings);
        //}

        //public static JsonResult SuccessObject(object data, string message = null)
        //{
        //    //很奇怪, 不能叫用這個 function. 會錯誤.
        //    //return SuccessObject(data, message, null);

        //    return new Microsoft.AspNetCore.Mvc.JsonResult(new ReturnObject
        //    {
        //        Rc = 0,
        //        Msg = message,
        //        Data = data
        //    });
        //}

        //public static JsonResult ErrorObject(object data, string message = null, int returnCode = -1)
        //{
        //    return new Microsoft.AspNetCore.Mvc.JsonResult(new ReturnObject
        //    {
        //        Rc = returnCode,
        //        Msg = message,
        //        Data = data
        //    });
        //}

        //public static JsonResult ErrorMessage(string message, int returnCode = -1)
        //{
        //    return ErrorObject(null, message, returnCode);
        //}

        ///// <summary>
        ///// 從 Client 端送來的資訊.
        ///// </summary>
        //public static string RequestString
        //{
        //    get
        //    {
        //        if (CurrentContext.Current.Items["RequestString"] == null)
        //        {
        //            CurrentContext.Current.Items["RequestString"] = Su.Wu.GetPostedString();
        //        }

        //        return (string)CurrentContext.Current.Items["RequestString"];
        //    }
        //}

        public class ValidateError
        {
            public string FieldName;
            public string Message;

            public ValidateError(string FieldName, string Message)
            {
                this.FieldName = FieldName;
                this.Message = Message;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="fieldName">Form 裡面的 name</param>
        ///// <param name="displayName">顯示用的名稱</param>
        ///// <param name="type"></param>
        ///// <param name="isEmptyOK"></param>
        ///// <param name="maxValue"></param>
        ///// <param name="minValue"></param>
        ///// <param name="Action"></param>
        ///// <param name="MaxLength"></param>
        //public static void Validate(string fieldName, string displayName, Validator.Type type = Validator.Type._string, bool isEmptyOK = false,
        //    double? maxValue = null, double? minValue = null, string Action = null, int MaxLength = 0)
        //{
        //    var errorMessage = Validator.GetErrorMessageFromInput(fieldName, displayName, type, isEmptyOK, maxValue, minValue, Action, MaxLength);
        //    if (!String.IsNullOrEmpty(errorMessage))
        //    {
        //        AddValidateError(fieldName, errorMessage);
        //    }
        //}

        //public static void ValidateObject(object obj, string fieldName, string displayName, Validator.Type type = Validator.Type._string, bool isEmptyOK = false,
        //    double? maxValue = null, double? minValue = null, string Action = null, int MaxLength = 0)
        //{
        //    var errorMessage = Validator.GetErrorMessageFromObject(obj, fieldName, displayName, type, isEmptyOK, maxValue, minValue, Action, MaxLength);
        //    if (!String.IsNullOrEmpty(errorMessage))
        //    {
        //        AddValidateError(fieldName, errorMessage);
        //    }
        //}

        ///// <summary>
        ///// 給 json 轉 ExpandoObject 時用的
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="fieldName"></param>
        ///// <param name="displayName"></param>
        ///// <param name="type"></param>
        ///// <param name="isEmptyOK"></param>
        ///// <param name="maxValue"></param>
        ///// <param name="minValue"></param>
        ///// <param name="Action"></param>
        ///// <param name="MaxLength"></param>
        //public static void ValidateObject(System.Dynamic.ExpandoObject obj, string fieldName, string displayName, Validator.Type type = Validator.Type._string, bool isEmptyOK = false,
        //    double? maxValue = null, double? minValue = null, string Action = null, int MaxLength = 0)
        //{
        //    var errorMessage = Validator.GetErrorMessageFromObject(obj, fieldName, displayName, type, isEmptyOK, maxValue, minValue, Action, MaxLength);
        //    if (!String.IsNullOrEmpty(errorMessage))
        //    {
        //        AddValidateError(fieldName, errorMessage);
        //    }
        //}



        ///// <summary>
        ///// 檢查檔案大小, 類別, 是否必上傳
        ///// </summary>
        ///// <param name="fieldName"></param>
        ///// <param name="displayName"></param>
        ///// <param name="exts"></param>
        ///// <param name="maxSize"></param>
        ///// <param name="isEmptyOk"></param>
        //public static void ValidateFile(string fieldName, string displayName, string exts = "jpg,gif,png,jpeg,xls,xlsx,svg", int maxSize = 0, bool isEmptyOk = true)
        //{
        //    AddValidateError(fieldName, Validator.GetErrorMessageFromFile(Su.Wu.Files[fieldName], displayName, exts, maxSize, isEmptyOk));
        //}

        ///// <summary>
        ///// 檢查上傳圖檔的長寬, 大小(Bytes)和類別
        ///// </summary>
        ///// <param name="fieldName"></param>
        ///// <param name="displayName"></param>
        ///// <param name="w"></param>
        ///// <param name="h"></param>
        ///// <param name="checkType">0: 長寬必需吻合; 1: 長寬必需大於 W, H; 2: 長寬必需小於 W, H</param>
        ///// <param name="isEmptyOK"></param>
        ///// <param name="maxSize"></param>
        ///// <param name="exts"></param>
        //public static void ValidateImageWH(string fieldName, string displayName, int w, int h, int checkType, bool isEmptyOK = true, int maxSize = 0,
        //    string exts = "jpg,gif,png,jpeg,xls,xlsx,svg")
        //{
        //    AddValidateError(fieldName,
        //    Validator.GetErrorMessageFromImage(Su.Wu.Files[fieldName], displayName, w, h, checkType, isEmptyOK, maxSize));
        //}

        //public static void AddValidateError(string FieldName, string Message)
        //{
        //    if (!string.IsNullOrEmpty(Message))
        //    {
        //        LtValidateError.Add(new ValidateError(FieldName, Message));
        //    }
        //}

        //public static string AllValidateError(string seperator = "\r\n")
        //{
        //    var ltErrors = LtValidateError.Select(r => r.Message).ToList();

        //    return string.Join(seperator, LtValidateError.Select(r => r.Message).ToList());
        //}

        ///// <summary>
        ///// 單頁使用
        ///// </summary>
        //public static List<ValidateError> LtValidateError
        //{
        //    get
        //    {
        //        if (CurrentContext.Current.Items["APIValidateError"] == null)
        //        {
        //            CurrentContext.Current.Items["APIValidateError"] = new List<ValidateError>();
        //        }

        //        return (List<ValidateError>)CurrentContext.Current.Items["APIValidateError"];
        //    }
        //}

        //public static void ClearValidateError()
        //{
        //    if (CurrentContext.Current.Items.ContainsKey("APIValidateError"))
        //    {
        //        CurrentContext.Current.Items.Remove("APIValidateError");
        //    }
        //}
    }
}
