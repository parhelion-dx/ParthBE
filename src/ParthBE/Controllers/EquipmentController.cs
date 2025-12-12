using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EquipmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly InventoryNumberService _inventoryNumberService;

        public EquipmentController(ApplicationDbContext context, InventoryNumberService inventoryNumberService)
        {
            _context = context;
            _inventoryNumberService = inventoryNumberService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EquipmentDto>>> GetAll()
        {
            var equipment = await _context.Equipment
                .Include(e => e.Type)
                .ThenInclude(t => t.Subject)
                .Include(e => e.Location)
                .Include(e => e.AssignedStaff)
                .Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    TypeId = e.TypeId,
                    TypeName = e.Type.Name,
                    SubjectName = e.Type.Subject.Name,
                    InventoryNumber = e.InventoryNumber,
                    LocationId = e.LocationId,
                    LocationName = e.Location.Name,
                    AssignedStaffId = e.AssignedStaffId,
                    AssignedStaffName = e.AssignedStaff.FullName,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(equipment);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentDto>> GetById(int id)
        {
            var equipment = await _context.Equipment
                .Include(e => e.Type)
                .ThenInclude(t => t.Subject)
                .Include(e => e.Location)
                .Include(e => e.AssignedStaff)
                .Where(e => e.Id == id)
                .Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    TypeId = e.TypeId,
                    TypeName = e.Type.Name,
                    SubjectName = e.Type.Subject.Name,
                    InventoryNumber = e.InventoryNumber,
                    LocationId = e.LocationId,
                    LocationName = e.Location.Name,
                    AssignedStaffId = e.AssignedStaffId,
                    AssignedStaffName = e.AssignedStaff.FullName,
                    Status = e.Status
                })
                .FirstOrDefaultAsync();

            if (equipment == null)
                return NotFound();

            return Ok(equipment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EquipmentDto>> Create(CreateEquipmentDto dto)
        {
            // Проверяем что AssignedStaffId указан
            if (string.IsNullOrWhiteSpace(dto.AssignedStaffId))
                return BadRequest("Необходимо указать ответственного сотрудника");

            // Проверяем что сотрудник существует
            var staff = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AssignedStaffId);
            if (staff == null)
                return BadRequest("Сотрудник не найден");

            // Получаем тип оборудования для генерации инвентарного номера
            var equipmentType = await _context.EquipmentTypes.FindAsync(dto.TypeId);
            if (equipmentType == null)
                return BadRequest("Тип оборудования не найден");

            // Генерируем инвентарный номер автоматически
            var inventoryNumber = await _inventoryNumberService.GenerateInventoryNumber(dto.TypeId, equipmentType.Name);

            var equipment = new Equipment
            {
                TypeId = dto.TypeId,
                InventoryNumber = inventoryNumber,
                LocationId = dto.LocationId,
                AssignedStaffId = dto.AssignedStaffId,
                Status = "available"
            };

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = equipment.Id },
                await GetEquipmentDto(equipment.Id));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EquipmentDto>> Update(int id, CreateEquipmentDto dto)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
                return NotFound();

            // Проверяем что AssignedStaffId указан
            if (string.IsNullOrWhiteSpace(dto.AssignedStaffId))
                return BadRequest("Необходимо указать ответственного сотрудника");

            // Проверяем что сотрудник существует
            var staff = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.AssignedStaffId);
            if (staff == null)
                return BadRequest("Сотрудник не найден");

            // Если изменился тип, перегенерируем инвентарный номер
            if (equipment.TypeId != dto.TypeId)
            {
                var equipmentType = await _context.EquipmentTypes.FindAsync(dto.TypeId);
                if (equipmentType == null)
                    return BadRequest("Тип оборудования не найден");

                equipment.InventoryNumber = await _inventoryNumberService.GenerateInventoryNumber(dto.TypeId, equipmentType.Name);
            }

            equipment.TypeId = dto.TypeId;
            equipment.LocationId = dto.LocationId;
            equipment.AssignedStaffId = dto.AssignedStaffId;

            await _context.SaveChangesAsync();

            return Ok(await GetEquipmentDto(equipment.Id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
                return NotFound();

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<EquipmentDto> GetEquipmentDto(int id)
        {
            return await _context.Equipment
                .Include(e => e.Type)
                .ThenInclude(t => t.Subject)
                .Include(e => e.Location)
                .Include(e => e.AssignedStaff)
                .Where(e => e.Id == id)
                .Select(e => new EquipmentDto
                {
                    Id = e.Id,
                    TypeId = e.TypeId,
                    TypeName = e.Type.Name,
                    SubjectName = e.Type.Subject.Name,
                    InventoryNumber = e.InventoryNumber,
                    LocationId = e.LocationId,
                    LocationName = e.Location.Name,
                    AssignedStaffId = e.AssignedStaffId,
                    AssignedStaffName = e.AssignedStaff.FullName,
                    Status = e.Status
                })
                .FirstAsync();
        }

        [HttpGet("types")]
        public async Task<ActionResult<List<EquipmentTypeDto>>> GetTypes()
        {
            var types = await _context.EquipmentTypes
                .Include(t => t.Subject)
                .Select(t => new EquipmentTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    SubjectId = t.SubjectId,
                    SubjectName = t.Subject.Name
                })
                .ToListAsync();

            return Ok(types);
        }

        [HttpPost("types")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EquipmentTypeDto>> CreateType(CreateEquipmentTypeDto dto)
        {
            var type = new EquipmentType
            {
                Name = dto.Name,
                Description = dto.Description,
                SubjectId = dto.SubjectId
            };

            _context.EquipmentTypes.Add(type);
            await _context.SaveChangesAsync();

            var result = await _context.EquipmentTypes
                .Include(t => t.Subject)
                .Where(t => t.Id == type.Id)
                .Select(t => new EquipmentTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    SubjectId = t.SubjectId,
                    SubjectName = t.Subject.Name
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetTypes), result);
        }
    }
}
