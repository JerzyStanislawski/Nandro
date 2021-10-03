using Microsoft.EntityFrameworkCore;
using Nandro.Models;
using Nandro.Providers;
using System.Linq;

namespace Nandro.Data
{
    public class NandroDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Configuration> Configuration { get; set; }

        public string DbPath { get; private set; }

        public NandroDbContext()
        {
            DbPath = $".\\nandro.db";
            Database.EnsureCreated();

            if (!Configuration.Any())
            {
                Configuration.Add(new Configuration
                {
                    PublicNanoApiUri = "https://proxy.nanos.cc/proxy",
                    PublicNanoSocketUri = "wss://socket.nanos.cc",
                    TransactionTimeoutSec = 60,
                    CurrencyCode = PriceProvider.UsdCode
                });

                SaveChanges();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
