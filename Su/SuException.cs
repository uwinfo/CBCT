using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Su.Api;

namespace Su
{
    //public class ErrorField
    //{
    //    public string Name { get; set; }
    //    public List<string> Errors { get; set; }

    //    public ErrorField(string name, List<string> errors)
    //    {
    //        Name = name;
    //        Errors = errors;
    //    }

    //    public ErrorField(string name, string errors)
    //    {
    //        Name = name;
    //        Errors = new List<string>();
    //        Errors.Add(errors);
    //    }
    //}

    public class SuException : Exception
    {
        public HttpStatusCode HttpStatus;
        new public string Message;
        public object ExtraData;
        public List<ErrorField> ValidateErrors;
        public string IpAll;

        public SuException(HttpStatusCode httpStatus, string errorMessage, object extraData = null, List<ErrorField> validateErrors = null)
        {
            HttpStatus = httpStatus;
            Message = errorMessage;
            ExtraData = extraData;
            ValidateErrors = validateErrors;
        }
    }

    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode;
        new public string Response;

        public ApiException(HttpStatusCode statusCode, string response)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }
}
