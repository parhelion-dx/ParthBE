namespace Backend.DTOs
{
    public class CreateBookingDto
    {
        public int SlotId { get; set; }
        public DateTime StartTime { get; set; }  // Время начала бронирования внутри слота
        public DateTime EndTime { get; set; }    // Время окончания бронирования внутри слота
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public int SlotId { get; set; }
        public string StudentUserId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }  // Время начала бронирования
        public DateTime EndTime { get; set; }    // Время окончания бронирования
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
