namespace Backend.DTOs
{
    public class CreateSlotDto
    {
        public int EquipmentId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    // Создание слотов сразу для нескольких единиц оборудования
    public class CreateBulkSlotsDto
    {
        public List<int> EquipmentIds { get; set; } = new();
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
        // Теперь слот может иметь несколько бронирований
        public List<BookingDto> Bookings { get; set; } = new();
    }
}
