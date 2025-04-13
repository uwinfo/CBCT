using Su;

namespace Core.Helpers
{
    public class SystemHelper
    {
        private readonly HttpContextWrapper _httpContext;
        public SystemHelper(HttpContextWrapper httpContext)
        {
            _httpContext = httpContext;
        }

        public string? ComputerUid
        {
            get
            {
                if (_httpContext.Items.ContainsKey("ComputerUid"))
                {
                    return (string)_httpContext.Items["ComputerUid"];
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    _httpContext.Items.Remove("ComputerUid");
                }
                else
                {
                    _httpContext.Items["ComputerUid"] = value;
                }
            }
        }
    }
}