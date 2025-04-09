using Newtonsoft.Json;
using Su;

namespace Core.Helpers
{
    public class LogHelper
    {
        /// <summary>
        /// 記錄在 Exception 之中，並保留 30 天。
        /// </summary>
        /// <param name="ex"></param>
        public static void AddExceptionLog(string prefix, Exception ex, bool isThrowExctption = false)
        {
            Su.FileLogger.AddDailyLog("Exception", ex.FullInfo(), removeFilesBeforeDays: 30, isThrowException: isThrowExctption, filenamePrefix: prefix);
        }

        /// <summary>
        /// 新增 Log 資料
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordUid"></param>
        /// <param name="beforeObject"></param>
        /// <param name="afterObject"></param>
        /// <param name="beforeAdditionalObject"></param>
        /// <param name="afterAdditionalObject"></param>
        /// <returns></returns>
        public static dynamic InsertSysLog(string tableName, string recordUid, object beforeObject, object? afterObject, object? beforeAdditionalObject = null, object? afterAdditionalObject = null)
        {
            var adminUser = AuthHelper.LoginAdmin;
            var creatorUid = adminUser == null ? "" : adminUser.Uid;
            var creatorName = adminUser == null ? "" : adminUser.Name;

            // 將 beforeObject 和 afterObject 轉換為 JSON 字串
            var beforeObjectJson = JsonConvert.SerializeObject(beforeObject);
            var afterObjectJson = afterObject == null ? "" : JsonConvert.SerializeObject(afterObject);
            var before_additional_json = beforeAdditionalObject == null ? "" : JsonConvert.SerializeObject(beforeAdditionalObject);
            var after_additional_json = afterAdditionalObject == null ? "" : JsonConvert.SerializeObject(afterAdditionalObject);

            return DapperHelper.ExecuteSQL(@"INSERT INTO sys_log (table_name, record_uid, before_json, after_json, before_additional_json, after_additional_json, created_at, creator_uid, creator_name) 
            VALUES (@tableName, @recordUid, @beforeObjectJson, @afterObjectJson, @before_additional_json, @after_additional_json, now(), @creatorUid, @creatorName)",
            new { tableName, recordUid, beforeObjectJson, afterObjectJson, before_additional_json, after_additional_json, creatorUid, creatorName });
        }
    }
}
