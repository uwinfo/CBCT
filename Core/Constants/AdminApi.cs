
namespace Core.Constants
{
    /// <summary>
    /// 在 AdminApi
    /// </summary>
    public class Env
    {
        static Models.AdminAppSettings.CommonClass? _AdminCommonSettings = null;

        /// <summary>
        /// 初始化 AdminCommonSettings
        /// </summary>
        /// <param name="commonClass"></param>
        /// <exception cref="Exception"></exception>
        public static void SetAdminCommonSettings(Models.AdminAppSettings.CommonClass commonClass)
        {
            _AdminCommonSettings = _AdminCommonSettings == null ? commonClass : throw new Exception("不可重覆設定 AdminCommonSettings");
        }

        internal static Models.AdminAppSettings.CommonClass AdminCommonSettings
        {
            get {
                if (_AdminCommonSettings == null)
                {
                    throw new Exception("AdminCommonSettings 未設定");
                }

                return _AdminCommonSettings; 
            }
        }

        //static Models.WwwSettings.CommonClass? _WwwCommonSettings = null;

        ///// <summary>
        ///// 初始化 WwwCommonSettings
        ///// </summary>
        ///// <param name="commonClass"></param>
        ///// <exception cref="Exception"></exception>
        //public static void SetWwwCommonSettings(Models.WwwSettings.CommonClass commonClass)
        //{
        //    _WwwCommonSettings = _WwwCommonSettings == null ? commonClass : throw new Exception("不可重覆設定 WwwCommonSettings");
        //}

        //internal static Models.WwwSettings.CommonClass WwwCommonSettings
        //{
        //    get
        //    {
        //        if (_WwwCommonSettings == null)
        //        {
        //            throw new Exception("請先使用 SetWwwCommonSettings 來初始化 WwwCommonSettings");
        //        }

        //        return _WwwCommonSettings;
        //    }
        //}
    }
}
