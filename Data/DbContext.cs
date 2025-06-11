using Microsoft.EntityFrameworkCore;
using Registration_API.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Registration_API.Data
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DeviceModel> device_models { get; set; }
        public DbSet<FirmwareVersion> firmware_versions { get; set; }
        public DbSet<Device> devices { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<DeviceCommand> device_commands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Device → DeviceModel (Many-to-One)
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Model)
                .WithMany(m => m.Devices)
                .HasForeignKey(d => d.model_id);

            // Device → FirmwareVersion (Many-to-One)
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Firmware)
                .WithMany(f => f.Devices)
                .HasForeignKey(d => d.firmware_id);

            // User → Device (Many-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Device)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.device_id)
                .OnDelete(DeleteBehavior.SetNull);

            // Device → DeviceCommand (One-to-Many)
            modelBuilder.Entity<Device>()
                .HasMany(d => d.DeviceCommands)
                .WithOne(dc => dc.Device)
                .HasForeignKey(dc => dc.device_id);
        }
    }

}
