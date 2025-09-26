using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Mvc;
using BackendBiblioMate.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// API controller for managing notification log entries.
    /// Provides endpoints to retrieve and create <see cref="NotificationLogDocument"/> entries
    /// stored in the MongoDB collection.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
            _mongoLogService = mongoLogService ?? throw new ArgumentNullException(nameof(mongoLogService));
        }

        /// <summary>
        /// Retrieves all notification log entries.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for request abortion.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="NotificationLogDocument"/>.
        /// </returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves all notification logs (v1)",
            Description = "Returns all notification log entries stored in the system.",
            Tags = ["NotificationsLogs"]
        )]
        [ProducesResponseType(typeof(IEnumerable<NotificationLogDocument>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationLogDocument>>> GetAll(CancellationToken cancellationToken = default)
        {
            var logs = await _mongoLogService.GetAllAsync(cancellationToken);
            return Ok(logs);
        }

        /// <summary>
        /// Retrieves a notification log entry by its identifier.
        /// </summary>
        /// <param name="id">The ObjectId of the log entry as a string.</param>
        /// <param name="cancellationToken">Cancellation token for request abortion.</param>
        /// <returns>
        /// <c>200 OK</c> with the <see cref="NotificationLogDocument"/>,
        /// or <c>404 Not Found</c> if no matching document exists.
        /// </returns>
        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a notification log by ID (v1)",
            Description = "Returns a single notification log entry by its unique identifier.",
            Tags = ["NotificationsLogs"]
        )]
        [ProducesResponseType(typeof(NotificationLogDocument), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            [FromRoute(Name = "id")] string id,
            CancellationToken cancellationToken = default)
        {
            var log = await _mongoLogService.GetByIdAsync(id, cancellationToken);
            if (log is null)
                return NotFound(new { error = $"Notification log with ID '{id}' not found." });

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
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Creates a new notification log entry (v1)",
            Description = "Creates a new notification log document.",
            Tags = ["NotificationsLogs"]
        )]
        [ProducesResponseType(typeof(NotificationLogDocument), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] NotificationLogCreateDto? dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                return BadRequest(new { error = "Invalid payload." });

            // Map DTO to domain document
            var document = new NotificationLogDocument
            {
                UserId  = dto.UserId,
                Type    = dto.Type,
                Message = dto.Message,
                SentAt  = dto.SentAt
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
