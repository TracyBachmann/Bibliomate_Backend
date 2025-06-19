using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers
{
    /// <summary>
    /// Manages user notifications.
    /// All endpoints require authentication.
    /// Only Admins and Librarians can create, update, or delete notifications.
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

        // GET: api/Notifications
        /// <summary>
        /// Retrieves all notifications for the current user.
        /// Admins and Librarians can view all notifications.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (User.IsInRole("Admin") || User.IsInRole("Librarian"))
            {
                return await _context.Notifications
                    .Include(n => n.User)
                    .ToListAsync();
            }

            return await _context.Notifications
                .Where(n => n.UserId == currentUserId)
                .Include(n => n.User)
                .ToListAsync();
        }

        // GET: api/Notifications/5
        /// <summary>
        /// Retrieves a specific notification by ID.
        /// Access is restricted to the notification's owner or authorized roles.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NotificationId == id);

            if (notification == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (notification.UserId != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Librarian"))
                return Forbid();

            return notification;
        }

        // POST: api/Notifications
        /// <summary>
        /// Creates a new notification. Restricted to Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notification);
        }

        // PUT: api/Notifications/5
        /// <summary>
        /// Updates an existing notification by ID. Restricted to Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, Notification notification)
        {
            if (id != notification.NotificationId)
                return BadRequest();

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Notifications.Any(n => n.NotificationId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Notifications/5
        /// <summary>
        /// Deletes a notification by ID. Restricted to Admins and Librarians.
        /// </summary>
        [Authorize(Roles = "Librarian,Admin")]
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
    }
}