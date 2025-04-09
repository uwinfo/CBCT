using System.ComponentModel;

namespace Core.Constants
{
    public class General
    {
        public class Status
        {
            /// <summary>
            /// 停用
            /// </summary>
            [Description("停用")]
            public const int Disabled = -100;

            /// <summary>
            /// 啟用
            /// </summary>
            [Description("啟用")]
            public const int Enabled = 100;
        }
    }
}
