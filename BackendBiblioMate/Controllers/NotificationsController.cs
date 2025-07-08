using System.Security.Claims;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing user notifications.
    /// Provides CRUD operations on <see cref="Notification"/> entities,
    /// dispatches real-time SignalR notifications, and logs each dispatch.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
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
        private Task<Notification?> FindNotificationAsync(int id, CancellationToken ct) =>
            _context.Notifications
                    .Include(n => n.User)
                    .FirstOrDefaultAsync(n => n.NotificationId == id, ct);

        /// <summary>
        /// Retrieves all notifications visible to the current user.
        /// </summary>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all notifications (v1)",
            Description = "Returns all notifications visible to the current user.",
            Tags = ["Notifications"]
        )]
        [ProducesResponseType(typeof(IEnumerable<NotificationReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationReadDto>>> GetNotifications(
            CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications.Include(n => n.User).AsQueryable();
            if (!IsPrivileged)
                query = query.Where(n => n.UserId == CurrentUserId);

            var list = await query.ToListAsync(cancellationToken);
            return Ok(list.Select(ToDto));
        }

        /// <summary>
        /// Retrieves a specific notification by its identifier.
        /// </summary>
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a notification by ID (v1)",
            Description = "Returns a single notification by ID. Enforces authorization.",
            Tags = ["Notifications"]
        )]
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
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPost]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates and dispatches a notification (v1)",
            Description = "Creates a new notification, sends it via SignalR, and logs it. Requires Librarian or Admin role.",
            Tags = ["Notifications"]
        )]
        [ProducesResponseType(typeof(NotificationReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NotificationReadDto>> CreateNotification(
            [FromBody] NotificationCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Notification
            {
                UserId    = dto.UserId,
                Title     = dto.Title,
                Message   = dto.Message,
                Timestamp = DateTime.UtcNow
            };

            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.NotifyUser(dto.UserId, dto.Message, cancellationToken);
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
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Updates a notification (v1)",
            Description = "Updates title, message, and user. Requires Librarian or Admin role.",
            Tags = ["Notifications"]
        )]
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

            var note = await _context.Notifications.FindAsync(new object[]{ id }, cancellationToken);
            if (note == null)
                return NotFound();

            note.UserId  = dto.UserId;
            note.Title   = dto.Title;
            note.Message = dto.Message;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Deletes a notification record.
        /// Only Librarians and Admins may call this.
        /// </summary>
        [Authorize(Roles = UserRoles.Librarian + "," + UserRoles.Admin)]
        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Deletes a notification (v1)",
            Description = "Deletes a notification by ID. Requires Librarian or Admin role.",
            Tags = ["Notifications"]
        )]
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
            Message        = n.Message,
            Timestamp      = n.Timestamp
        };
    }
}