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

        [HttpGet("available")]
        public async Task<ActionResult<List<SlotDto>>> GetAvailableSlots()
        {
            var slots = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Where(s => s.Status == "available" && s.StartTime > DateTime.UtcNow)
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
                    Status = s.Status
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpGet("my-slots")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<List<SlotDto>>> GetMySlots()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var slots = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(s => s.Equipment.Location)
                .Include(s => s.CreatedByStaff)
                .Include(s => s.Booking)
                .ThenInclude(b => b!.StudentUser)
                .Where(s => s.CreatedByStaffId == userId)
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
                    Booking = s.Booking != null ? new BookingDto
                    {
                        Id = s.Booking.Id,
                        SlotId = s.Booking.SlotId,
                        StudentUserId = s.Booking.StudentUserId,
                        StudentName = s.Booking.StudentUser.FullName,
                        StudentEmail = s.Booking.StudentUser.Email!,
                        Status = s.Booking.Status,
                        CreatedAt = s.Booking.CreatedAt,
                        UpdatedAt = s.Booking.UpdatedAt
                    } : null
                })
                .ToListAsync();

            return Ok(slots);
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<SlotDto>> CreateSlot(CreateSlotDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var equipment = await _context.Equipment
                .FirstOrDefaultAsync(e => e.Id == dto.EquipmentId && e.AssignedStaffId == userId);

            if (equipment == null)
                return BadRequest("Equipment not assigned to you");

            var hasConflict = await _context.Slots
                .AnyAsync(s => s.EquipmentId == dto.EquipmentId &&
                    ((dto.StartTime >= s.StartTime && dto.StartTime < s.EndTime) ||
                     (dto.EndTime > s.StartTime && dto.EndTime <= s.EndTime)));

            if (hasConflict)
                return BadRequest("Time slot conflict");

            var slot = new Slot
            {
                EquipmentId = dto.EquipmentId,
                CreatedByStaffId = userId!,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = "available"
            };

            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailableSlots), await GetSlotDto(slot.Id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult> DeleteSlot(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var slot = await _context.Slots
                .Include(s => s.Booking)
                .FirstOrDefaultAsync(s => s.Id == id && s.CreatedByStaffId == userId);

            if (slot == null)
                return NotFound();

            if (slot.Booking != null)
                return BadRequest("Cannot delete booked slot. Cancel booking first.");

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
                    Status = s.Status
                })
                .FirstAsync();
        }
    }
}
