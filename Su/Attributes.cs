using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Su
{
    namespace CustomAttributes
    {
        /// <summary>
        /// 使用 CopyTo 時要略過
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class CopyToIgnore() : Attribute
        {

        }
    }
}


