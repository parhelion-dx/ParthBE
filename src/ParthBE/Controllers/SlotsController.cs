using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SlotsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Нормализация времени к 15-минутным интервалам (округление вниз)
        private DateTime NormalizeTime(DateTime time)
        {
            var minutes = (time.Minute / 15) * 15;
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, minutes, 0, DateTimeKind.Utc);
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<SlotDto>>> GetAvailableSlots()
        {
            var now = DateTime.UtcNow;

            var slots = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Include(s => s.Bookings)
                .ThenInclude(b => b.StudentUser)
                .Where(s => s.Status == "available" && s.EndTime > now)
                .OrderBy(s => s.StartTime)
                .Select(s => new SlotDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentTypeName = s.Equipment.Type.Name,
                    InventoryNumber = s.Equipment.InventoryNumber,
                    LocationName = s.Equipment.Location.Name,
                    CreatedByStaffId = s.CreatedByStaffId,
                    CreatedByStaffName = s.CreatedByStaff.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    Bookings = s.Bookings
                        .Where(b => b.Status == "pending_approval" || b.Status == "confirmed")
                        .Select(b => new BookingDto
                        {
                            Id = b.Id,
                            SlotId = b.SlotId,
                            StudentUserId = b.StudentUserId,
                            StudentName = b.StudentUser.FullName,
                            StudentEmail = b.StudentUser.Email!,
                            StartTime = b.StartTime,
                            EndTime = b.EndTime,
                            Status = b.Status,
                            CreatedAt = b.CreatedAt,
                            UpdatedAt = b.UpdatedAt
                        }).ToList()
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpGet("my-slots")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult<List<SlotDto>>> GetMySlots()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var now = DateTime.UtcNow;

            var query = _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Include(s => s.Bookings)
                .ThenInclude(b => b.StudentUser)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(s => s.CreatedByStaffId == userId);
            }

            var slots = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new SlotDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentTypeName = s.Equipment.Type.Name,
                    InventoryNumber = s.Equipment.InventoryNumber,
                    LocationName = s.Equipment.Location.Name,
                    CreatedByStaffId = s.CreatedByStaffId,
                    CreatedByStaffName = s.CreatedByStaff.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.EndTime < now && s.Status == "available" ? "expired" : s.Status,
                    Bookings = s.Bookings.Select(b => new BookingDto
                    {
                        Id = b.Id,
                        SlotId = b.SlotId,
                        StudentUserId = b.StudentUserId,
                        StudentName = b.StudentUser.FullName,
                        StudentEmail = b.StudentUser.Email!,
                        StartTime = b.StartTime,
                        EndTime = b.EndTime,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult<SlotDto>> CreateSlot(CreateSlotDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var startTime = NormalizeTime(dto.StartTime);
            var endTime = NormalizeTime(dto.EndTime);

            if (startTime >= endTime)
                return BadRequest("Время начала должно быть раньше времени окончания");

            if ((endTime - startTime).TotalMinutes < 15)
                return BadRequest("Минимальная длительность слота - 15 минут");

            var equipment = isAdmin
                ? await _context.Equipment.FirstOrDefaultAsync(e => e.Id == dto.EquipmentId)
                : await _context.Equipment.FirstOrDefaultAsync(e => e.Id == dto.EquipmentId && e.AssignedStaffId == userId);

            if (equipment == null)
                return BadRequest("Оборудование не найдено или не назначено вам");

            var hasConflict = await _context.Slots
                .AnyAsync(s => s.EquipmentId == dto.EquipmentId &&
                    s.Status == "available" &&
                    ((startTime >= s.StartTime && startTime < s.EndTime) ||
                     (endTime > s.StartTime && endTime <= s.EndTime) ||
                     (startTime <= s.StartTime && endTime >= s.EndTime)));

            if (hasConflict)
                return BadRequest("Конфликт времени с существующим слотом");

            var slot = new Slot
            {
                EquipmentId = dto.EquipmentId,
                CreatedByStaffId = userId!,
                StartTime = startTime,
                EndTime = endTime,
                Status = "available"
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailableSlots), await GetSlotDto(slot.Id));
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult<List<SlotDto>>> CreateBulkSlots(CreateBulkSlotsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (dto.EquipmentIds == null || dto.EquipmentIds.Count == 0)
                return BadRequest("Не выбрано ни одной единицы оборудования");

            var startTime = NormalizeTime(dto.StartTime);
            var endTime = NormalizeTime(dto.EndTime);

            if (startTime >= endTime)
                return BadRequest("Время начала должно быть раньше времени окончания");

            if ((endTime - startTime).TotalMinutes < 15)
                return BadRequest("Минимальная длительность слота - 15 минут");

            var equipmentQuery = _context.Equipment.Where(e => dto.EquipmentIds.Contains(e.Id));
            if (!isAdmin)
            {
                equipmentQuery = equipmentQuery.Where(e => e.AssignedStaffId == userId);
            }
            var validEquipmentIds = await equipmentQuery.Select(e => e.Id).ToListAsync();

            if (validEquipmentIds.Count == 0)
                return BadRequest("Не найдено оборудования, которое назначено вам");

            var conflictingEquipmentIds = await _context.Slots
                .Where(s => validEquipmentIds.Contains(s.EquipmentId) &&
                    s.Status == "available" &&
                    ((startTime >= s.StartTime && startTime < s.EndTime) ||
                     (endTime > s.StartTime && endTime <= s.EndTime) ||
                     (startTime <= s.StartTime && endTime >= s.EndTime)))
                .Select(s => s.EquipmentId)
                .Distinct()
                .ToListAsync();

            var equipmentIdsToCreate = validEquipmentIds.Except(conflictingEquipmentIds).ToList();

            if (equipmentIdsToCreate.Count == 0)
                return BadRequest("Для всего выбранного оборудования есть конфликты времени");

            var slots = equipmentIdsToCreate.Select(equipmentId => new Slot
            {
                EquipmentId = equipmentId,
                CreatedByStaffId = userId!,
                StartTime = startTime,
                EndTime = endTime,
                Status = "available"
            }).ToList();

            _context.Slots.AddRange(slots);
            await _context.SaveChangesAsync();

            var createdSlotIds = slots.Select(s => s.Id).ToList();
            var result = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Where(s => createdSlotIds.Contains(s.Id))
                .Select(s => new SlotDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentTypeName = s.Equipment.Type.Name,
                    InventoryNumber = s.Equipment.InventoryNumber,
                    LocationName = s.Equipment.Location.Name,
                    CreatedByStaffId = s.CreatedByStaffId,
                    CreatedByStaffName = s.CreatedByStaff.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    Bookings = new List<BookingDto>()
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult> DeleteSlot(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var slot = isAdmin
                ? await _context.Slots.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == id)
                : await _context.Slots.Include(s => s.Bookings).FirstOrDefaultAsync(s => s.Id == id && s.CreatedByStaffId == userId);

            if (slot == null)
                return NotFound();

            var hasActiveBookings = slot.Bookings.Any(b => b.Status == "pending_approval" || b.Status == "confirmed");
            if (hasActiveBookings)
                return BadRequest("Невозможно удалить слот с активными бронированиями");

            _context.Slots.Remove(slot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<SlotDto> GetSlotDto(int id)
        {
            return await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Include(s => s.Bookings)
                .ThenInclude(b => b.StudentUser)
                .Where(s => s.Id == id)
                .Select(s => new SlotDto
                {
                    Id = s.Id,
                    EquipmentId = s.EquipmentId,
                    EquipmentTypeName = s.Equipment.Type.Name,
                    InventoryNumber = s.Equipment.InventoryNumber,
                    LocationName = s.Equipment.Location.Name,
                    CreatedByStaffId = s.CreatedByStaffId,
                    CreatedByStaffName = s.CreatedByStaff.FullName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    Bookings = s.Bookings.Select(b => new BookingDto
                    {
                        Id = b.Id,
                        SlotId = b.SlotId,
                        StudentUserId = b.StudentUserId,
                        StudentName = b.StudentUser.FullName,
                        StudentEmail = b.StudentUser.Email!,
                        StartTime = b.StartTime,
                        EndTime = b.EndTime,
                        Status = b.Status,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    }).ToList()
                })
                .FirstAsync();
        }
    }
}
