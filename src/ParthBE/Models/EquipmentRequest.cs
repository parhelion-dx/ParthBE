namespace Backend.Models
{
    public class EquipmentRequest
    {
        public int Id { get; set; }
        public string RequestedByUserId { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string? Justification { get; set; }
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public User RequestedByUser { get; set; } = null!;
        public Subject? Subject { get; set; }
    }
}
