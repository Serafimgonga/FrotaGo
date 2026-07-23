using FrotaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<FuelRecord> FuelRecords => Set<FuelRecord>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();
    public DbSet<Accident> Accidents => Set<Accident>();
    public DbSet<VehicleLocation> VehicleLocations => Set<VehicleLocation>();
    public DbSet<TrackingSession> TrackingSessions => Set<TrackingSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ShortName).HasMaxLength(50);
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.NIF).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.NIF).IsUnique();
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Plan).HasMaxLength(50);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(30);

            entity.HasOne(e => e.School)
                .WithMany(s => s.Branches)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.IdentityCardNumber).HasMaxLength(50);

            entity.HasOne(e => e.School)
                .WithMany(s => s.Users)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(250);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
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

        modelBuilder.Entity<TrackingSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.StartedAt).IsRequired();

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Instructor)
                .WithMany()
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Lesson)
                .WithMany()
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VehicleLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Speed).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TrackingSession)
                .WithMany(s => s.Locations)
                .HasForeignKey(e => e.TrackingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
