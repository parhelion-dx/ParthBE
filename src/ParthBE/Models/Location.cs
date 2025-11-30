namespace Backend.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Навигационные свойства
        public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    }
}
