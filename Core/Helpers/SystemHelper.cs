namespace Core.Helpers
{
    public static class SystemHelper
    {
        /// <summary>
        /// 存放在 Su.CurrentContext.Items
        /// </summary>
        public static string? ComputerUid
        {
            get
            {
                if (Su.CurrentContext.Items.ContainsKey("ComputerUid"))
                {
                    return (string)Su.CurrentContext.Items["ComputerUid"];
                }

                return null;
            }
            set
            {
                if(value == null)
                {
                    Su.CurrentContext.Items.Remove("ComputerUid");
                }
                else
                {
                    Su.CurrentContext.Items["ComputerUid"] = value;
                }
            }
        }
    }
}
