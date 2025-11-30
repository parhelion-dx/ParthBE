namespace Backend.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string? AssignedStaffId { get; set; }
        public string Status { get; set; } = "available"; // available, in_maintenance, retired

        // Навигационные свойства
        public EquipmentType Type { get; set; } = null!;
        public Location Location { get; set; } = null!;
        public User? AssignedStaff { get; set; }
        public ICollection<Slot> Slots { get; set; } = new List<Slot>();
    }
}
