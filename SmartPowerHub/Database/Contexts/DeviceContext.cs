using Microsoft.EntityFrameworkCore;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Database.Contexts
{
    public class DeviceContext : DbContext
    {
        public DbSet<DeviceModel> Devices { get; set; }
        public DbSet<ScheduledProgramModel> ScheduledPrograms { get; set; }

        public DeviceContext(DbContextOptions<DeviceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceModel>();
            modelBuilder.Entity<ScheduledProgramModel>();
        }
    }
}
