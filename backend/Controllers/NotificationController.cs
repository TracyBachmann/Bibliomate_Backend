using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models.Enums;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing user notifications.
    /// All endpoints require authentication.
    /// Only Librarians and Admins may create, update, or delete notifications.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly BiblioMateDbContext      _context;
        private readonly INotificationService     _notificationService;
        private readonly INotificationLogService  _notificationLogService;

        public NotificationsController(
            BiblioMateDbContext      context,
            INotificationService     notificationService,
            INotificationLogService  notificationLogService)
        {
            _context                = context;
            _notificationService    = notificationService;
            _notificationLogService = notificationLogService;
        }

        /// <summary>
        /// Retrieves all notifications visible to the current user.
        /// </summary>
        /// <returns>A list of <see cref="NotificationReadDto"/>.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationReadDto>>> GetNotifications()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPrivileged  = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            var query = _context.Notifications
                                .Include(n => n.User)
                                .AsQueryable();

            if (!isPrivileged)
                query = query.Where(n => n.UserId == currentUserId);

            var list = await query.ToListAsync();
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// Retrieves a specific notification by its identifier.
        /// </summary>
        /// <param name="id">The notification identifier.</param>
        /// <returns>The <see cref="NotificationReadDto"/> or 404.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationReadDto>> GetNotification(int id)
        {
            var n = await _context.Notifications
                                  .Include(x => x.User)
                                  .FirstOrDefaultAsync(x => x.NotificationId == id);
            if (n == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPrivileged  = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);
            if (!isPrivileged && n.UserId != currentUserId)
                return Forbid();

            return Ok(ToDto(n));
        }

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="dto">The notification creation DTO.</param>
        /// <returns>The created <see cref="NotificationReadDto"/>.</returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<NotificationReadDto>> CreateNotification(NotificationCreateDto dto)
        {
            var n = new Notification
            {
                UserId  = dto.UserId,
                Title   = dto.Title,
                Message = dto.Message
            };
            _context.Notifications.Add(n);
            await _context.SaveChangesAsync();

            // dispatch & log
            await _notificationService.NotifyUser(dto.UserId, dto.Message);
            await _notificationLogService.LogAsync(dto.UserId, NotificationType.Custom, dto.Message);

            var created = await _context.Notifications
                                        .Include(x => x.User)
                                        .FirstAsync(x => x.NotificationId == n.NotificationId);

            return CreatedAtAction(nameof(GetNotification),
                new { id = n.NotificationId },
                ToDto(created));
        }

        /// <summary>
        /// Updates an existing notification.
        /// </summary>
        /// <param name="id">The identifier of the notification.</param>
        /// <param name="dto">The updated notification DTO.</param>
        /// <returns>204 NoContent on success.</returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, NotificationUpdateDto dto)
        {
            if (id != dto.NotificationId) return BadRequest();

            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();

            n.UserId  = dto.UserId;
            n.Title   = dto.Title;
            n.Message = dto.Message;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a notification.
        /// </summary>
        /// <param name="id">The identifier of the notification.</param>
        /// <returns>204 NoContent on success.</returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n == null) return NotFound();

            _context.Notifications.Remove(n);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static NotificationReadDto ToDto(Notification n) => new()
        {
            NotificationId = n.NotificationId,
            UserId         = n.UserId,
            UserName       = n.User.Name,
            Title          = n.Title,
            Message        = n.Message
        };
    }
}