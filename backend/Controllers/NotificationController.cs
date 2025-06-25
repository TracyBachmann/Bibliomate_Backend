using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Models.Enums;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing user notifications.
    /// All endpoints require authentication.  
    /// Only Librarians and Admins may create, update, or delete notifications.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;

        public NotificationsController(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all notifications visible to the current user.
        /// GET: api/Notifications
        /// </summary>
        /// <remarks>
        /// Admins and Librarians receive every notification,  
        /// while regular users only receive their own.
        /// </remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationReadDto>>> GetNotifications()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var notifications = (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian))
                ? await _context.Notifications.Include(n => n.User).ToListAsync()
                : await _context.Notifications
                    .Where(n => n.UserId == currentUserId)
                    .Include(n => n.User)
                    .ToListAsync();

            return Ok(notifications.Select(ToNotificationReadDto));
        }

        /// <summary>
        /// Retrieves a specific notification by its identifier.
        /// GET: api/Notifications/{id}
        /// </summary>
        /// <param name="id">The notification identifier.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationReadDto>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (notification.UserId != currentUserId &&
                !User.IsInRole(UserRoles.Admin) && !User.IsInRole(UserRoles.Librarian))
                return Forbid();

            return Ok(ToNotificationReadDto(notification));
        }

        /// <summary>
        /// Creates a new notification.
        /// POST: api/Notifications
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="dto">The notification creation DTO.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<NotificationReadDto>> CreateNotification(NotificationCreateDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var readDto = await _context.Notifications
                .Include(n => n.User)
                .Where(n => n.NotificationId == notification.NotificationId)
                .Select(n => ToNotificationReadDto(n))
                .FirstAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, readDto);
        }

        /// <summary>
        /// Updates an existing notification.
        /// PUT: api/Notifications/{id}
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the notification to update.</param>
        /// <param name="dto">The updated notification DTO.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, NotificationUpdateDto dto)
        {
            if (id != dto.NotificationId)
                return BadRequest();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            notification.UserId = dto.UserId;
            notification.Title = dto.Title;
            notification.Message = dto.Message;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes a notification.
        /// DELETE: api/Notifications/{id}
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the notification to delete.</param>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound();

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Maps a <see cref="Notification"/> entity to a <see cref="NotificationReadDto"/>.
        /// </summary>
        private static NotificationReadDto ToNotificationReadDto(Notification n) => new()
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            UserName = n.User.Name,
            Title = n.Title,
            Message = n.Message
        };
    }
}
