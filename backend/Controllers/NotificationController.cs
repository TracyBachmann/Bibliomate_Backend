using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
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

        // GET: api/Notifications
        /// <summary>
        /// Retrieves all notifications visible to the current user.
        /// </summary>
        /// <remarks>
        /// Admins and Librarians receive every notification,  
        /// while regular users only receive their own.
        /// </remarks>
        /// <returns>A collection of notifications.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian))
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

        // GET: api/Notifications/{id}
        /// <summary>
        /// Retrieves a specific notification by its identifier.
        /// </summary>
        /// <param name="id">The notification identifier.</param>
        /// <returns>
        /// The requested notification if found and authorized;  
        /// otherwise <c>403 Forbid</c> or <c>404 NotFound</c>.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id)
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

            return notification;
        }

        // POST: api/Notifications
        /// <summary>
        /// Creates a new notification.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="notification">The notification entity to create.</param>
        /// <returns>
        /// <c>201 Created</c> with the created entity and its URI.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification),
                new { id = notification.NotificationId }, notification);
        }

        // PUT: api/Notifications/{id}
        /// <summary>
        /// Updates an existing notification.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the notification to update.</param>
        /// <param name="notification">The updated notification entity.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if the IDs do not match;  
        /// <c>404 NotFound</c> if the notification does not exist.
        /// </returns>
        [Authorize(Roles = $"{UserRoles.Librarian},{UserRoles.Admin}")]
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
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Notifications/{id}
        /// <summary>
        /// Deletes a notification.
        /// Accessible to Librarians and Admins only.
        /// </summary>
        /// <param name="id">The identifier of the notification to delete.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>404 NotFound</c> if the notification is not found.
        /// </returns>
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
    }
}
