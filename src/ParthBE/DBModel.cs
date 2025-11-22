using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParthBE.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ProfileInfo { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Equipment> AssignedEquipment { get; set; } = new List<Equipment>();
        public virtual ICollection<Slot> CreatedSlots { get; set; } = new List<Slot>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    public class Role
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }

    public class Subject
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public virtual ICollection<EquipmentType> EquipmentTypes { get; set; } = new List<EquipmentType>();
        public virtual ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
    }

    public class Location
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    }

    public class EquipmentType
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public int SubjectId { get; set; }
        
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;
        
        public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    }

    public class Equipment
    {
        [Key]
        public int Id { get; set; }
        
        public int TypeId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string InventoryNumber { get; set; } = string.Empty;
        
        public int LocationId { get; set; }
        
        public int? AssignedStaffId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "available";
        
        [ForeignKey("TypeId")]
        public virtual EquipmentType EquipmentType { get; set; } = null!;
        
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; } = null!;
        
        [ForeignKey("AssignedStaffId")]
        public virtual User? AssignedStaff { get; set; }
        
        public virtual ICollection<Slot> Slots { get; set; } = new List<Slot>();
    }

    public class Slot
    {
        [Key]
        public int Id { get; set; }
        
        public int EquipmentId { get; set; }
        
        public int CreatedByStaffId { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "available";
        
        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; } = null!;
        
        [ForeignKey("CreatedByStaffId")]
        public virtual User CreatedByStaff { get; set; } = null!;
        
        public virtual Booking? Booking { get; set; }
    }

    public class Booking
    {
        [Key]
        public int Id { get; set; }
        
        public int SlotId { get; set; }
        
        public int StudentUserId { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "pending_approval";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("SlotId")]
        public virtual Slot Slot { get; set; } = null!;
        
        [ForeignKey("StudentUserId")]
        public virtual User StudentUser { get; set; } = null!;
    }

    public class EquipmentRequest
    {
        [Key]
        public int Id { get; set; }
        
        public int RequestedByUserId { get; set; }
        
        public int? SubjectId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string EquipmentName { get; set; } = string.Empty;
        
        public string Justification { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "pending";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("RequestedByUserId")]
        public virtual User RequestedByUser { get; set; } = null!;
        
        [ForeignKey("SubjectId")]
        public virtual Subject? Subject { get; set; }
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public bool IsRead { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}