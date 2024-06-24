using Microsoft.EntityFrameworkCore;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Database.Contexts
{
    public class ApplianceContext : DbContext
    {
        public DbSet<ApplianceModel> Appliances { get; set; }

        public ApplianceContext(DbContextOptions<ApplianceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplianceModel>();
        }
    }
}
