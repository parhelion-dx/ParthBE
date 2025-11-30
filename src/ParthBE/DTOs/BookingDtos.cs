namespace Backend.DTOs
{
    public class CreateBookingDto
    {
        public int SlotId { get; set; }
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public int SlotId { get; set; }
        public string StudentUserId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public SlotDto? Slot { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
