using ParthBE.Models;
using Microsoft.EntityFrameworkCore;

namespace ParthBE.Data
{
    public class ParthBEDbContext : DbContext
    {
        public ParthBEDbContext(DbContextOptions<ParthBEDbContext> options) : base(options)
        {
        }

        // Таблицы остаются без изменений
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Для PostgreSQL рекомендуется явно указывать схемы
            modelBuilder.HasDefaultSchema("public");

            // Составной первичный ключ для UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Уникальные индексы
            modelBuilder.Entity<UserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Equipment>()
                .HasIndex(e => e.InventoryNumber)
                .IsUnique();

            modelBuilder.Entity<Slot>()
                .HasIndex(s => new { s.EquipmentId, s.StartTime })
                .IsUnique();

            // Один-к-одному: один слот = одно бронирование
            modelBuilder.Entity<Slot>()
                .HasOne(s => s.Booking)
                .WithOne(b => b.Slot)
                .HasForeignKey<Booking>(b => b.SlotId);

            // Для PostgreSQL можно настроить конкретные типы данных
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Slot>()
                .Property(s => s.StartTime)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Slot>()
                .Property(s => s.EndTime)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Booking>()
                .Property(b => b.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Booking>()
                .Property(b => b.UpdatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<EquipmentRequest>()
                .Property(er => er.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Notification>()
                .Property(n => n.CreatedAt)
                .HasColumnType("timestamp with time zone");
        }
    }
}