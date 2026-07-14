using FrotaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<FuelRecord> FuelRecords => Set<FuelRecord>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.LicensePlate).IsUnique();
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Chassis).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Chassis).IsUnique();
            entity.Property(e => e.Fuel).HasConversion<int>();
            entity.Property(e => e.Transmission).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IdentityCardNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Category).HasConversion<int>();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Observations).HasMaxLength(500);

            entity.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Instructor)
                .WithMany()
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Cost).HasPrecision(18, 2);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FuelRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Litres).HasPrecision(10, 2);
            entity.Property(e => e.CostPerLitre).HasPrecision(10, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VehicleDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.FileUrl).HasMaxLength(500);

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
