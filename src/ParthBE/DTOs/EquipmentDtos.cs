namespace Backend.DTOs
{
    public class CreateEquipmentDto
    {
        public int TypeId { get; set; }
        // InventoryNumber убран - генерируется автоматически
        public int LocationId { get; set; }
        public string AssignedStaffId { get; set; } = string.Empty; // Обязательное поле
    }

    public class EquipmentDto
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InventoryNumber { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string AssignedStaffId { get; set; } = string.Empty;
        public string AssignedStaffName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class CreateEquipmentTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
    }

    public class EquipmentTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
}
