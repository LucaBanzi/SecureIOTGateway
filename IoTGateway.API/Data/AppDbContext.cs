using IoTGateway.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTGateway.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryData> Telemetries => Set<TelemetryData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rendiamo il SerialNumber unico a livello di Database
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.SerialNumber)
            .IsUnique();

        // Configurazione della Foreign Key
        modelBuilder.Entity<TelemetryData>()
            .HasOne(t => t.Device)
            .WithMany(d => d.Telemetries)
            .HasForeignKey(t => t.DeviceId);
    }
}