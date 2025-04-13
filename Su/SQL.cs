namespace Su.Sql
{
    /// <summary>
    /// 不要把 DbcNames 轉為數字。
    /// 這裡應該不用指定數字。
    /// </summary>
    public class DbId
    {
        /// <summary>
        /// 控制 Id 不可重覆
        /// </summary>
        public HashSet<int> allIds = new HashSet<int>();

        int _Id = -1;

        /// <summary>
        /// 建立 Cache 時會用到這個 Id
        /// </summary>
        public int Id
        {
            get
            {
                return _Id;
            }
        }

        public DbId(int id)
        {
            lock (Su.LockerProvider.GetLocker("Create DbId"))
            {
                if (allIds.Contains(id))
                {
                    throw new Exception("不可立重覆 Id 的 DbId");
                }

                allIds.Add(id);
            }

            this._Id = id;
        }
    }
}

