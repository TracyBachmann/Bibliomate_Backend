using System.Security.Claims;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing user notifications.
    /// Provides CRUD operations on <see cref="Notification"/> entities,
    /// dispatches real-time SignalR notifications, and logs each dispatch.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly BiblioMateDbContext      _context;
        private readonly INotificationService     _notificationService;
        private readonly INotificationLogService  _notificationLogService;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationsController"/>.
        /// </summary>
        /// <param name="context">EF Core DB context for notifications.</param>
        /// <param name="notificationService">
        /// Service for dispatching real-time notifications via SignalR.
        /// </param>
        /// <param name="notificationLogService">
        /// Service for logging notification events to MongoDB.
        /// </param>
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
        /// Gets the current authenticated user's identifier.
        /// </summary>
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Indicates whether the current user has librarian or admin rights.
        /// </summary>
        private bool IsPrivileged =>
            User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

        /// <summary>
        /// Finds a notification entity by its primary key.
        /// </summary>
        /// <param name="id">The notification identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The matching <see cref="Notification"/> or <c>null</c>.</returns>
        private Task<Notification?> FindNotificationAsync(int id, CancellationToken ct) =>
            _context.Notifications
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.NotificationId == id, ct);

        /// <summary>
        /// Retrieves all notifications visible to the current user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="NotificationReadDto"/>.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationReadDto>>> GetNotifications(
            CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications
                                .Include(n => n.User)
                                .AsQueryable();

            if (!IsPrivileged)
                query = query.Where(n => n.UserId == CurrentUserId);

            var list = await query.ToListAsync(cancellationToken);
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// Retrieves a specific notification by its identifier.
        /// </summary>
        /// <param name="id">Notification identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="NotificationReadDto"/>,  
        /// <c>404 NotFound</c> if not found,  
        /// <c>403 Forbidden</c> if the user lacks permission.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<NotificationReadDto>> GetNotification(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var n = await FindNotificationAsync(id, cancellationToken);
            if (n == null) return NotFound();
            if (!IsPrivileged && n.UserId != CurrentUserId)
                return Forbid();
            return Ok(ToDto(n));
        }

        /// <summary>
        /// Creates a new notification, dispatches it in real time via SignalR, and logs the event.
        /// Only users with Librarian or Admin roles may call this.
        /// </summary>
        /// <param name="dto">Creation data for the notification.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>201 Created</c> with <see cref="NotificationReadDto"/>,  
        /// <c>400 BadRequest</c> if the model is invalid.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [ProducesResponseType(typeof(NotificationReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NotificationReadDto>> CreateNotification(
            [FromBody] NotificationCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Notification
            {
                UserId  = dto.UserId,
                Title   = dto.Title,
                Message = dto.Message
            };

            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Dispatch real-time via SignalR
            await _notificationService.NotifyUser(dto.UserId, dto.Message, cancellationToken);
            // Log event
            await _notificationLogService.LogAsync(
                dto.UserId,
                NotificationType.Custom,
                dto.Message,
                cancellationToken);

            var created = await FindNotificationAsync(entity.NotificationId, cancellationToken)
                             ?? throw new InvalidOperationException("Created notification not found.");

            return CreatedAtAction(
                nameof(GetNotification),
                new { id = created.NotificationId },
                ToDto(created));
        }

        /// <summary>
        /// Updates an existing notification’s title, message, and owner.
        /// Only Librarians and Admins may call this.
        /// </summary>
        /// <param name="id">Identifier of the notification to update.</param>
        /// <param name="dto">Updated notification data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success,  
        /// <c>400 BadRequest</c> if the IDs mismatch,  
        /// <c>404 NotFound</c> if the notification does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateNotification(
            [FromRoute] int id,
            [FromBody] NotificationUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.NotificationId)
                return BadRequest();

            var rows = await _context.Notifications
                .Where(n => n.NotificationId == id)
                .ExecuteUpdateAsync(u => u
                        .SetProperty(n => n.UserId,  dto.UserId)
                        .SetProperty(n => n.Title,   dto.Title)
                        .SetProperty(n => n.Message, dto.Message),
                    cancellationToken);

            if (rows == 0)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Deletes a notification record.
        /// Only Librarians and Admins may call this.
        /// </summary>
        /// <param name="id">Identifier of the notification to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>404 NotFound</c> if not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(
            [FromRoute] int id,
            CancellationToken cancellationToken = default)
        {
            var existing = await _context.Notifications.FindAsync(new object[]{ id }, cancellationToken);
            if (existing == null) return NotFound();

            _context.Notifications.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Maps a <see cref="Notification"/> entity to its DTO.
        /// </summary>
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