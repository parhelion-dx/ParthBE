namespace Backend.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string InventoryNumber { get; set; } = string.Empty; // Генерируется автоматически
        public int LocationId { get; set; }
        public string AssignedStaffId { get; set; } = string.Empty; // Обязательное поле
        public string Status { get; set; } = "available"; // available, in_maintenance, retired

        // Навигационные свойства
        public EquipmentType Type { get; set; } = null!;
        public Location Location { get; set; } = null!;
        public User AssignedStaff { get; set; } = null!;
        public ICollection<Slot> Slots { get; set; } = new List<Slot>();
    }
}
