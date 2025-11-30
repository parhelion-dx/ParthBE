using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public BookingsController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("my-bookings")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<BookingDto>>> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.Slot)
                .ThenInclude(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(b => b.Slot.Equipment.Location)
                .Include(b => b.Slot.CreatedByStaff)
                .Include(b => b.StudentUser)
                .Where(b => b.StudentUserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    SlotId = b.SlotId,
                    StudentUserId = b.StudentUserId,
                    StudentName = b.StudentUser.FullName,
                    StudentEmail = b.StudentUser.Email!,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Slot = new SlotDto
                    {
                        Id = b.Slot.Id,
                        EquipmentId = b.Slot.EquipmentId,
                        EquipmentTypeName = b.Slot.Equipment.Type.Name,
                        InventoryNumber = b.Slot.Equipment.InventoryNumber,
                        LocationName = b.Slot.Equipment.Location.Name,
                        CreatedByStaffId = b.Slot.CreatedByStaffId,
                        CreatedByStaffName = b.Slot.CreatedByStaff.FullName,
                        StartTime = b.Slot.StartTime,
                        EndTime = b.Slot.EndTime,
                        Status = b.Slot.Status
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var slot = await _context.Slots
                .Include(s => s.Booking)
                .FirstOrDefaultAsync(s => s.Id == dto.SlotId);

            if (slot == null)
                return NotFound("Slot not found");

            if (slot.Status != "available")
                return BadRequest("Slot is not available");

            if (slot.Booking != null)
                return BadRequest("Slot already booked");

            var booking = new Booking
            {
                SlotId = dto.SlotId,
                StudentUserId = userId!,
                Status = "pending_approval",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            slot.Status = "booked";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await _notificationService.NotifyBookingCreated(booking);

            return CreatedAtAction(nameof(GetMyBookings), await GetBookingDto(booking.Id));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<BookingDto>> UpdateBookingStatus(int id, UpdateBookingStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id && b.Slot.CreatedByStaffId == userId);

            if (booking == null)
                return NotFound();

            booking.Status = dto.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == "cancelled_by_staff")
            {
                booking.Slot.Status = "available";
                await _notificationService.NotifyBookingCancelled(booking, "staff");
            }
            else if (dto.Status == "confirmed")
            {
                await _notificationService.CreateNotificationAsync(
                    booking.StudentUserId,
                    "Ваше бронирование подтверждено"
                );
            }

            await _context.SaveChangesAsync();

            return Ok(await GetBookingDto(booking.Id));
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<BookingDto>> CancelMyBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.Id == id && b.StudentUserId == userId);

            if (booking == null)
                return NotFound();

            booking.Status = "cancelled_by_student";
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Slot.Status = "available";

            await _context.SaveChangesAsync();

            return Ok(await GetBookingDto(booking.Id));
        }

        private async Task<BookingDto> GetBookingDto(int id)
        {
            return await _context.Bookings
                .Include(b => b.Slot)
                .ThenInclude(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .Include(b => b.Slot.Equipment.Location)
                .Include(b => b.Slot.CreatedByStaff)
                .Include(b => b.StudentUser)
                .Where(b => b.Id == id)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    SlotId = b.SlotId,
                    StudentUserId = b.StudentUserId,
                    StudentName = b.StudentUser.FullName,
                    StudentEmail = b.StudentUser.Email!,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    Slot = new SlotDto
                    {
                        Id = b.Slot.Id,
                        EquipmentId = b.Slot.EquipmentId,
                        EquipmentTypeName = b.Slot.Equipment.Type.Name,
                        InventoryNumber = b.Slot.Equipment.InventoryNumber,
                        LocationName = b.Slot.Equipment.Location.Name,
                        CreatedByStaffId = b.Slot.CreatedByStaffId,
                        CreatedByStaffName = b.Slot.CreatedByStaff.FullName,
                        StartTime = b.Slot.StartTime,
                        EndTime = b.Slot.EndTime,
                        Status = b.Slot.Status
                    }
                })
                .FirstAsync();
        }
    }
}
