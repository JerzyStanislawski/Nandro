using Microsoft.EntityFrameworkCore;
using Nandro.Models;

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
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
