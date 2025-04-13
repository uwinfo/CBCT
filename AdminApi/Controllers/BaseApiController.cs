using Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AdminApi
{
    [Route("[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        //protected readonly string _envName;
        protected readonly Core.Models.AdminAppSettings.CommonClass _commonSettings;
        //protected readonly IWebHostEnvironment _env;
        protected readonly Core.Ef.CBCTContext _dbContext;
        protected readonly AuthHelper _authHelper;

        //public BaseApiController(IOptions<Core.Models.AdminAppSettings> appSettings, IWebHostEnvironment env, Core.Ef.CBCTContext dbContext, AuthHelper authHelper)
        //{
        //    //_envName = env.EnvironmentName;
        //    _commonSettings = appSettings.Value.Common;
        //    //_env = env;
        //    _dbContext = dbContext;
        //    _authHelper = authHelper;
        //}

        public BaseApiController(IOptions<Core.Models.AdminAppSettings.CommonClass> commonSettings, IWebHostEnvironment env, Core.Ef.CBCTContext dbContext, AuthHelper authHelper)
        {
            //_envName = env.EnvironmentName;
            _commonSettings = commonSettings.Value;
            //_env = env;
            _dbContext = dbContext;
            _authHelper = authHelper;
        }

        /// <summary>
        /// 登入者的完整資
        /// </summary>
        /// <returns></returns>
        protected Core.Dtos.AdminUserDto? LoginAdmin
        {
            get
            {
                return _authHelper.LoginAdmin;
            }
        }
    }
}
