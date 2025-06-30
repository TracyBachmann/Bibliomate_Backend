using backend.DTOs;
using backend.Models.Mongo;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    /// <summary>
    /// Provides endpoints to manage notification log entries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationLogsController : ControllerBase
    {
        private readonly IMongoLogService _mongoLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationLogsController"/> class.
        /// </summary>
        /// <param name="mongoLogService">
        /// Service responsible for reading and writing notification logs in MongoDB.
        /// </param>
        public NotificationLogsController(IMongoLogService mongoLogService)
        {
            _mongoLogService = mongoLogService;
        }

        /// <summary>
        /// Retrieves all notification log entries.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of all <see cref="NotificationLogDocument"/> objects.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _mongoLogService.GetAllAsync();
            return Ok(logs);
        }

        /// <summary>
        /// Retrieves a single notification log entry by its identifier.
        /// </summary>
        /// <param name="id">The ObjectId of the log entry to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the matching <see cref="NotificationLogDocument"/>,
        /// or <c>404 Not Found</c> if no entry exists with the given id.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var log = await _mongoLogService.GetByIdAsync(id);
            if (log is null) return NotFound();
            return Ok(log);
        }

        /// <summary>
        /// Creates a new notification log entry.
        /// </summary>
        /// <param name="dto">
        /// The <see cref="CreateNotificationLogDto"/> containing the properties for the new log entry.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> with <c>201 Created</c> and a Location header
        /// pointing to <see cref="GetById(string)"/> for the newly-created entry.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationLogDto dto)
        {
            var document = new NotificationLogDocument
            {
                UserId  = dto.UserId,
                Type    = dto.Type,
                Message = dto.Message,
                SentAt  = dto.SentAt
            };

            await _mongoLogService.AddAsync(document);

            return CreatedAtAction(
                nameof(GetById),
                new { id = document.Id },
                document
            );
        }
    }
}
