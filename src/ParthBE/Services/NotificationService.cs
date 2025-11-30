using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyBookingCreated(Booking booking)
        {
            var slot = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .FirstOrDefaultAsync(s => s.Id == booking.SlotId);

            if (slot != null)
            {
                await CreateNotificationAsync(
                    slot.CreatedByStaffId,
                    $"Новое бронирование оборудования {slot.Equipment.Type.Name} ({slot.Equipment.InventoryNumber}) на {slot.StartTime:dd.MM.yyyy HH:mm}"
                );
            }
        }

        public async Task NotifyBookingCancelled(Booking booking, string cancelledBy)
        {
            var slot = await _context.Slots
                .Include(s => s.Equipment)
                .ThenInclude(e => e.Type)
                .FirstOrDefaultAsync(s => s.Id == booking.SlotId);

            if (slot != null)
            {
                await CreateNotificationAsync(
                    booking.StudentUserId,
                    $"Ваше бронирование оборудования {slot.Equipment.Type.Name} на {slot.StartTime:dd.MM.yyyy HH:mm} было отменено"
                );
            }
        }
    }
}
