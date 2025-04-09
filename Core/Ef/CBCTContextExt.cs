using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Core.Ef
{
     public partial class CBCTContext : DbContext
    {
        private static string? _dbc;
        public static void SetDbc(string dbc)
        {
            _dbc = dbc;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_dbc)
                    .LogTo(Console.WriteLine, LogLevel.Information);
            }
        }

        /// <summary>
        /// 取得新的 DbContext
        /// </summary>
        internal static CBCTContext NewDbContext
        {
            get
            {
                if (string.IsNullOrEmpty(_dbc))
                {
                    throw new Exception("在請在 program.cs 中叫用 CBCTContext.SetDbc 來設定 Connection String");
                }

                return new Ef.CBCTContext(new DbContextOptionsBuilder<Ef.CBCTContext>()
                                    .UseNpgsql(_dbc)
                                    .LogTo(msg => System.Diagnostics.Debug.WriteLine(msg))
                                    .Options);
            }
        }
    }
}
