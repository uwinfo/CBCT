using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    /// <summary>
    /// 重新定義 CamelCaseContractResolver, 這個版本只會把第一個字元轉為小寫，其它的保持不變( _ 也會被保留)。
    /// NewtownSoft 內建的版本在轉換有 _ 的名稱時，會很奇怪。
    /// </summary>
    public class CamelCaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.LowerFirstCharacter();
        }
    }
}
