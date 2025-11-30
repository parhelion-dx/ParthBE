using Microsoft.AspNetCore.Identity;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfileInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Навигационные свойства
        public ICollection<Equipment> AssignedEquipment { get; set; } = new List<Equipment>();
        public ICollection<Slot> CreatedSlots { get; set; } = new List<Slot>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
