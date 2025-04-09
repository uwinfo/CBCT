//using Core.Dtos;
using Microsoft.AspNetCore.Http;
using Su;
using System.Net;

namespace Core.Helpers
{
    public class SecurityHelper
    {
        public static void CheckFileInfoAsync(HttpContext context, 
            string safeFileExts = ",.jpg,.jpeg,.png,.gif,.xls,.xlsx,.doc,.docx,.csv,.txt,",
            string imageFileExts = ",.jpg,.jpeg,.png,.gif,",
            int maxImageK = 800)
        {
            if (!context.Request.HasFormContentType || !context.Request.Form.Files.Any())
            {
                return;
            }

            if (!context.Request.ContentType.Contains("multipart/form-data"))
            {
                throw new CustomException("不支援的檔案格式", HttpStatusCode.UnsupportedMediaType);
            }

            foreach (IFormFile file in context.Request.Form.Files)
            {
                if (IsDangerousFileName(file.FileName))
                {
                    //var options = new Dictionary<string, string>();
                    //options.Add("filename", file.FileName);
                    //options.Add("end", "..");
                    //throw new Core.Dtos.CustomException(ERROR_CODE.DANGEROUS_FILENAME, HttpStatusCode.BadRequest, options);
                    throw new CustomException("高危險群檔案名稱，請修正", HttpStatusCode.BadRequest);
                }

                var ext = Path.GetExtension(file.FileName).ToLower();
                if (!safeFileExts.Contains("," + ext + ","))
                {
                    throw new CustomException("不支援的檔案格式", HttpStatusCode.BadRequest);
                }

                
                if (imageFileExts.Contains("," + ext + ",") && file.Length > maxImageK * 1024)
                {
                    throw new CustomException($"圖片過大，請優化後上傳, 最大限制 {maxImageK} K", HttpStatusCode.BadRequest);
                }
            }
        }

        public static bool IsDangerousFileName(string str)
        {
            if (str.Contains("..")
                || str.Contains("/")
                || str.Contains("\\")
                || str.Contains("|")
                || str.Contains("?")
                || str.Contains("\"")
                || str.Contains("*")
                || str.Contains(":")
                || str.Contains("<")
                || str.Contains(">"))
            {
                return true;
            }

            for (int i = 0; i < 32; i++)
            {
                if (str.Contains(char.ConvertFromUtf32(i)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
