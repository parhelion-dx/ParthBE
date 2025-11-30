namespace Backend.Models
{
    public class Slot
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string CreatedByStaffId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "available"; // available, booked

        // Навигационные свойства
        public Equipment Equipment { get; set; } = null!;
        public User CreatedByStaff { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}
