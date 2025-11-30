namespace Backend.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int SlotId { get; set; }
        public string StudentUserId { get; set; } = string.Empty;
        public string Status { get; set; } = "pending_approval"; // pending_approval, confirmed, cancelled_by_staff, cancelled_by_student
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public Slot Slot { get; set; } = null!;
        public User StudentUser { get; set; } = null!;
    }
}
