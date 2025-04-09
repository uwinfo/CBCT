using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class SessionHelper
    {
        public static HashSet<string> SequenceControllers = new HashSet<string>();
        public static HashSet<string> SequenceActions = new HashSet<string>();

        public static void AddSequenceAction(string key)
        {
            if (!SequenceActions.Contains(key)){
                SequenceControllers.Add(key);
            }
        }

        public static void AddSequenceController(string key)
        {
            if (!SequenceControllers.Contains(key))
            {
                SequenceControllers.Add(key);
            }
        }

        /// <summary>
        /// 是否必需循序執行
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static bool IsSequence(string? controllerName, string? actionName)
        {
            if(string.IsNullOrEmpty(controllerName))
            {
                return false;
            }

            if (SequenceControllers.Contains($"{controllerName.ToLower()}"))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(actionName))
            {
                if (SequenceControllers.Contains($"{controllerName.ToLower()}^{actionName.ToLower()}")
                    || SequenceControllers.Contains($"{controllerName.ToLower()}^{actionName.ToLower()}async")) // 含 Asnyc 結尾的 Action
                {
                    return true;
                }
            }

            return false;
        }
    }
}
