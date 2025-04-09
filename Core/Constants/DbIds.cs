using Su.Sql;

namespace Core.Constants
{
    /// <summary>
    /// 
    /// </summary>
    public class DbIds
    {
        private static readonly DbId _cbct = new(1);
        /// <summary>
        /// 
        /// </summary>
        public static DbId CBCT
        {
            get
            {
                return _cbct;
            }
        }
    }
}