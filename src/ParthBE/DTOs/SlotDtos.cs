namespace Backend.DTOs
{
    public class CreateSlotDto
    {
        public int EquipmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class SlotDto
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentTypeName { get; set; } = string.Empty;
        public string InventoryNumber { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string CreatedByStaffId { get; set; } = string.Empty;
        public string CreatedByStaffName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public BookingDto? Booking { get; set; }
    }
}
