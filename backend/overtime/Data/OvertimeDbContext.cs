using Microsoft.EntityFrameworkCore;
using Overtime.Data.Entities;

namespace Overtime.Data;

public sealed class OvertimeDbContext : DbContext
{
    public OvertimeDbContext(DbContextOptions<OvertimeDbContext> options) : base(options) { }

    public DbSet<LocationEntity> Locations => Set<LocationEntity>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Time");

        modelBuilder.Entity<LocationEntity>(e =>
        {
            e.ToTable("locations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Slug).HasColumnName("slug").IsRequired();
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.Property(x => x.Address).HasColumnName("address").IsRequired(false);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.Slug).IsUnique();
            e.OwnsOne(x => x.OpeningHours, b =>
            {
                b.ToJson("opening_hours");
                b.OwnsOne(h => h.Monday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Tuesday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Wednesday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Thursday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Friday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Saturday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
                b.OwnsOne(h => h.Sunday, d => { d.Property(o => o.OpenTime); d.Property(o => o.CloseTime); });
            });
        });

        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username").IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.Role).HasColumnName("role").IsRequired();
            e.Property(x => x.FullName).HasColumnName("full_name").IsRequired();
            e.Property(x => x.FirstName).HasColumnName("first_name").IsRequired().HasDefaultValue(string.Empty);
            e.Property(x => x.LastName).HasColumnName("last_name").IsRequired().HasDefaultValue(string.Empty);
            e.Property(x => x.LocationId).HasColumnName("location_id").IsRequired(false);
            e.OwnsMany(x => x.WorkingTimes, b =>
            {
                b.ToJson("working_times");
                b.Property(w => w.Day);
                b.Property(w => w.ShiftStart);
                b.Property(w => w.ShiftEnd);
            });
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne(x => x.Location)
             .WithMany()
             .HasForeignKey(x => x.LocationId)
             .IsRequired(false);
        });

        modelBuilder.Entity<BookingEntity>(e =>
        {
            e.ToTable("bookings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.BookingRef).HasColumnName("booking_ref").IsRequired();
            e.Property(x => x.StaffId).HasColumnName("staff_id");
            e.Property(x => x.SlotDate).HasColumnName("slot_date");
            e.Property(x => x.StartTime).HasColumnName("start_time");
            e.Property(x => x.EndTime).HasColumnName("end_time");
            e.Property(x => x.CustomerName).HasColumnName("customer_name").IsRequired();
            e.Property(x => x.CustomerEmail).HasColumnName("customer_email").IsRequired();
            e.Property(x => x.CustomerPhone).HasColumnName("customer_phone").IsRequired();
            e.Property(x => x.Notes).HasColumnName("notes");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.BookingRef).IsUnique();
            e.HasIndex(new[] { "StaffId", "SlotDate", "StartTime" })
             .IsUnique()
             .HasDatabaseName("uq_staff_slot");
            e.HasOne(x => x.Staff)
             .WithMany()
             .HasForeignKey(x => x.StaffId);
        });
    }
}
