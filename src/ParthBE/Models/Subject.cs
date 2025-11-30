namespace Backend.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Навигационные свойства
        public ICollection<EquipmentType> EquipmentTypes { get; set; } = new List<EquipmentType>();
        public ICollection<EquipmentRequest> EquipmentRequests { get; set; } = new List<EquipmentRequest>();
    }
}
