using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing notification log entries.
    /// Provides endpoints to retrieve and create <see cref="NotificationLogDocument"/> entries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NotificationsLogsController : ControllerBase
    {
        private readonly IMongoLogService _mongoLogService;

        /// <summary>
        /// Initializes a new instance of <see cref="NotificationsLogsController"/>.
        /// </summary>
        /// <param name="mongoLogService">Service for handling notification log persistence.</param>
        public NotificationsLogsController(IMongoLogService mongoLogService)
        {
            _mongoLogService = mongoLogService;
        }

        /// <summary>
        /// Retrieves all notification log entries.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="NotificationLogDocument"/>.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationLogDocument>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationLogDocument>>> GetAll(CancellationToken cancellationToken)
        {
            var logs = await _mongoLogService.GetAllAsync(cancellationToken);
            return Ok(logs);
        }

        /// <summary>
        /// Retrieves a notification log entry by its identifier.
        /// </summary>
        /// <param name="id">The ObjectId of the log entry.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>200 OK</c> with the <see cref="NotificationLogDocument"/>,
        /// or <c>404 NotFound</c> if not found.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NotificationLogDocument), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([
            FromRoute(Name = "id")] string id,
            CancellationToken cancellationToken)
        {
            var log = await _mongoLogService.GetByIdAsync(id, cancellationToken);
            if (log is null)
                return NotFound();

            return Ok(log);
        }

        /// <summary>
        /// Creates a new notification log entry.
        /// </summary>
        /// <param name="dto">The data transfer object containing log details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="NotificationLogDocument"/>
        /// and a Location header pointing to <see cref="GetById(string, CancellationToken)"/>.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(NotificationLogDocument), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] NotificationLogCreateDto dto,
            CancellationToken cancellationToken)
        {
            // Map DTO to domain document
            var document = new NotificationLogDocument
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Message = dto.Message,
                SentAt = dto.SentAt
            };

            await _mongoLogService.AddAsync(document, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = document.Id },
                document
            );
        }
    }
}
