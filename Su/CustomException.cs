using System.Net;

namespace Su
{
    public class ErrorField
    {
        public string Name { get; set; }
        public List<string> Errors { get; set; }

        public ErrorField(string name, List<string> errors)
        {
            Name = name;
            Errors = errors;
        }

        public ErrorField(string name, string errors)
        {
            Name = name;
            Errors = new List<string>();
            Errors.Add(errors);
        }
    }

    //public class CustomException : Exception
    //{
    //    public HttpStatusCode HttpStatus;
    //    new public string Message;
    //    public object ExtraData;
    //    public List<ErrorField> ValidateErrors;
    //    public string IpAll;

    //    public CustomException(HttpStatusCode httpStatus, string errorMessage, object extraData = null, List<ErrorField> validateErrors = null)
    //    {
    //        HttpStatus = httpStatus;
    //        Message = errorMessage;
    //        ExtraData = extraData;
    //        ValidateErrors = validateErrors;
    //    }
    //}

    public static class ValidateExtension
    {
        /// <summary>
        /// 檢查 Attribute 中的規則
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dto"></param>
        /// <param name="index">陣列中的第幾筆</param>
        /// <param name="prefix">回傳屬性名稱時加上前述字</param>
        /// <returns></returns>
        public static CustomException GetCustomException<T>(this T dto, int? index = null, CustomException? ex = null, string? prefix = null)
        {
            ex ??= new CustomException();
            var properties = dto!.GetType().GetProperties(); //dto 不應該是 null
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is IValidationChecker checker)
                    {
                        checker.CheckValue(property, dto!, ex, index, prefix);
                    }
                }
            }

            return ex;
        }

        /// <summary>
        /// 檢查後，直接丟出 Exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dto"></param>
        public static void GetCustomExceptionsAndThrow<T>(this T dto)
        {
            dto.GetCustomException().TryThrowValidationException();
        }
    }

    /// <summary>
    /// 自定義錯誤內容
    /// </summary>
    public class CustomException : Exception
    {
        public HttpStatusCode Code;

        //public ERROR_CODE ErrorCode;
        /// <summary>
        /// 會用來取代 Error Message 的字串，被取代的格式為: {key}
        /// </summary>
        public Dictionary<string, string>? Option;
        public Dictionary<string, List<string>>? InvalidatedPayload;

        public bool isAddStackTrace = false;
        public string? debugInfo;

        new public string? Message;
        public string IpAll;
        public object? ExtraData;
        public List<ErrorField>? ValidateErrors;

        public CustomException()
        {
        }

        public CustomException(HttpStatusCode code, string? message, object? extraData = null, List<ErrorField>? validateErrors = null)
        {
            Code = code;
            Message = message;
            ExtraData = extraData;
            ValidateErrors = validateErrors;
        }

        /// <summary>
        /// 回傳錯誤訊息給前台顯示即可。
        /// </summary>
        /// <param name="message"></param>
        public CustomException(string message, HttpStatusCode code = HttpStatusCode.BadRequest, string? debugInfo = null) : base(message)
        {
            this.Code = code;
            //this.ErrorCode = ERROR_CODE.MESSAGE;
            this.debugInfo = debugInfo;
            this.Message = message;
        }

        public CustomException(HttpStatusCode code = HttpStatusCode.OK, Dictionary<string, string>? option = null,
            Dictionary<string, List<string>>? invalidatedPayload = null, string? debugInfo = null, bool isAddStackTrace = false, string? message = null) : base(message)
        {
            this.Code = code;
            //this.ErrorCode = errorCode;
            this.Option = option;
            this.InvalidatedPayload = NormalizationDictionary(invalidatedPayload);
            this.debugInfo = debugInfo;
            this.isAddStackTrace = isAddStackTrace;
            this.Message = message;
        }

        //public CustomException(Dictionary<string, List<ERROR_CODE>> invalidatedPayloadCode, 
        //    HttpStatusCode code = HttpStatusCode.BadRequest, 
        //    bool isAddStackTrace = false, string? message = null) : base(message)
        //{
        //    this.Code = code;
        //    this.ErrorCode = ERROR_CODE.INVALID_FORM;
        //    this.InvalidatedPayloadCode = NormalizationDictionary(invalidatedPayloadCode);
        //    this.isAddStackTrace = isAddStackTrace;
        //}

        public CustomException(Dictionary<string, List<string>> invalidatedPayload, HttpStatusCode code = HttpStatusCode.BadRequest, bool isAddStackTrace = false, string? message = null) : base(message)
        {
            this.Code = code;
            this.InvalidatedPayload = NormalizationDictionary(invalidatedPayload);
            this.isAddStackTrace = isAddStackTrace;
            this.Message = message;
        }

        /// <summary>
        /// 把 key 的第一個字母轉小寫
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalInvalidatedPayload"></param>
        /// <returns></returns>
        public Dictionary<string, T>? NormalizationDictionary<T>(Dictionary<string, T>? originalInvalidatedPayload)
        {
            if (originalInvalidatedPayload == null)
            {
                return null;
            }

            var newInvalidatedPayload = new Dictionary<string, T>();

            foreach (var key in originalInvalidatedPayload.Keys)
            {
                newInvalidatedPayload[key.LowerFirstCharacter()] = originalInvalidatedPayload[key];
            }
            return newInvalidatedPayload;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="index"></param>
        /// <param name="isRequire">用來簡化呼叫方的程式用。</param>
        public void AddRequireField(string fieldName, int? index = null, bool isRequire = true, string? prefix = null)
        {
            if (!isRequire)
            {
                return;
            }

            AddValidationError(fieldName, Cst.ErrorMessage.SomeFieldIsRequired, index, prefix);
        }

        /// <summary>
        /// 檢查必要欄位 (在 property 上方中有加入 [Required] 的 attribute)
        /// </summary>
        /// <param name="dto"></param>
        public void CheckRequrieProperties<T>(T dto)
        {
            var properties = typeof(T).GetProperties();

            foreach (System.Reflection.PropertyInfo property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute is IValidationChecker checker)
                    {
                        checker.CheckValue(property, dto!, this);
                    }
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="propertyName"></param>
        ///// <param name="code"></param>
        ///// <param name="index"></param>
        //public void AddValidationError(string propertyName, string message, int? index = null, string? prefix = null)
        //{
        //    AddValidationError(propertyName, message, index, prefix);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="message"></param>
        /// <param name="index">從 0 開始</param>
        public void AddValidationError(string propertyName, string message, int? index = null, string? prefix = null)
        {
            InvalidatedPayload ??= [];

            propertyName = propertyName.LowerFirstCharacter()!;
            propertyName = prefix.LowerFirstCharacter().Attach(propertyName, "-").Attach(index?.ToString(), "-")!;

            if (!InvalidatedPayload.ContainsKey(propertyName))
            {
                InvalidatedPayload[propertyName] = new List<string>();
            }

            InvalidatedPayload[propertyName].Add(message);
        }

        /// <summary>
        /// 如果 InvalidatedPayload 不為空的，就丟出 Exception.
        /// </summary>
        public void TryThrowValidationException()
        {
            if (InvalidatedPayload != null && InvalidatedPayload.Count > 0)
            {
                this.Code = HttpStatusCode.BadRequest;
                throw this;
            }
        }
    }
}
