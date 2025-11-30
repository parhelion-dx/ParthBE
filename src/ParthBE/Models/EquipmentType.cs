namespace Backend.Models
{
    public class EquipmentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }

        // Навигационные свойства
        public Subject Subject { get; set; } = null!;
        public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    }
}
