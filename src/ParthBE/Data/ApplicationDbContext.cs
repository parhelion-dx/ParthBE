using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Переименование таблиц Identity в нижний регистр
            builder.Entity<User>().ToTable("users");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("user_roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("user_claims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("user_logins");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("user_tokens");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("role_claims");

            // Настройка User
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.UserName).HasColumnName("user_name");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.FullName).HasColumnName("full_name");
                entity.Property(e => e.ProfileInfo).HasColumnName("profile_info");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
                entity.Property(e => e.RefreshTokenExpiryTime).HasColumnName("refresh_token_expiry_time");
            });

            // Настройка Subject
            builder.Entity<Subject>(entity =>
            {
                entity.ToTable("subjects");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Настройка Location
            builder.Entity<Location>(entity =>
            {
                entity.ToTable("locations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
            });

            // Настройка EquipmentType
            builder.Entity<EquipmentType>(entity =>
            {
                entity.ToTable("equipment_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.SubjectId).HasColumnName("subject_id");

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.EquipmentTypes)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Equipment
            builder.Entity<Equipment>(entity =>
            {
                entity.ToTable("equipment");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.InventoryNumber).HasColumnName("inventory_number").IsRequired();
                entity.Property(e => e.LocationId).HasColumnName("location_id");
                entity.Property(e => e.AssignedStaffId).HasColumnName("assigned_staff_id");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("available");

                entity.HasIndex(e => e.InventoryNumber).IsUnique();

                entity.HasOne(e => e.Type)
                    .WithMany(t => t.Equipment)
                    .HasForeignKey(e => e.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Equipment)
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedStaff)
                    .WithMany(u => u.AssignedEquipment)
                    .HasForeignKey(e => e.AssignedStaffId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Настройка Slot
            builder.Entity<Slot>(entity =>
            {
                entity.ToTable("slots");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.EquipmentId).HasColumnName("equipment_id");
                entity.Property(e => e.CreatedByStaffId).HasColumnName("created_by_staff_id");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("available");

                entity.HasIndex(e => new { e.EquipmentId, e.StartTime }).IsUnique();

                entity.HasOne(e => e.Equipment)
                    .WithMany(eq => eq.Slots)
                    .HasForeignKey(e => e.EquipmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByStaff)
                    .WithMany(u => u.CreatedSlots)
                    .HasForeignKey(e => e.CreatedByStaffId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка Booking
            builder.Entity<Booking>(entity =>
            {
                entity.ToTable("bookings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.SlotId).HasColumnName("slot_id");
                entity.Property(e => e.StudentUserId).HasColumnName("student_user_id");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("pending_approval");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

                entity.HasIndex(e => e.SlotId).IsUnique();

                entity.HasOne(e => e.Slot)
                    .WithOne(s => s.Booking)
                    .HasForeignKey<Booking>(e => e.SlotId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.StudentUser)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.StudentUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка EquipmentRequest
            builder.Entity<EquipmentRequest>(entity =>
            {
                entity.ToTable("equipment_requests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RequestedByUserId).HasColumnName("requested_by_user_id");
                entity.Property(e => e.SubjectId).HasColumnName("subject_id");
                entity.Property(e => e.EquipmentName).HasColumnName("equipment_name").IsRequired();
                entity.Property(e => e.Justification).HasColumnName("justification");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("pending");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.RequestedByUser)
                    .WithMany(u => u.EquipmentRequests)
                    .HasForeignKey(e => e.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.EquipmentRequests)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Настройка Notification
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Message).HasColumnName("message").IsRequired();
                entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
