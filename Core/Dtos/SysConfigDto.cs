namespace Core.Dtos
{
        /// <summary>
        /// 系統參數資料
        /// </summary>
        public class SysConfigList
        {
            public long Id { get; set; }

            public string Uid { get; set; } = null!;

            /// <summary>
            /// 參數名稱
            /// </summary>
            public string Name { get; set; } = null!;

            /// <summary>
            /// 參數內容
            /// </summary>
            public string Content { get; set; } = null!;

            /// <summary>
            /// 參數描述
            /// </summary>
            public string? Description { get; set; }

            /// <summary>
            /// 建立日
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// 建立人(admin_user.uid)
            /// </summary>
            public string CreatorUid { get; set; } = null!;

            /// <summary>
            /// 建立人名稱(admin_user.name)
            /// </summary>
            public string? CreatorName { get; set; }

            /// <summary>
            /// 修改日
            /// </summary>
            public DateTime ModifiedAt { get; set; }

            /// <summary>
            /// 修改人(admin_user.uid)
            /// </summary>
            public string ModifierUid { get; set; } = null!;

            /// <summary>
            /// 修改人名稱(admin_user.name)
            /// </summary>
            public string? ModifierName { get; set; }
        }

        public class UpsertSysConfig()
        {
            public string Uid { get; set; } = null!;

            /// <summary>
            /// 參數名稱
            /// </summary>
            [Su.ValidationAttributes.RequireValidation]
            public string? Name { get; set; }

            /// <summary>
            /// 參數內容
            /// </summary>
            [Su.ValidationAttributes.RequireValidation]
            public string? Content { get; set; }

            /// <summary>
            /// 參數描述
            /// </summary>
            public string? Description { get; set; }
        }
}
