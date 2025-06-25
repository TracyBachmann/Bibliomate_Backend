using backend.Models.Mongo;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationLogsController : ControllerBase
    {
        private readonly MongoLogService _mongoLogService;

        public NotificationLogsController(MongoLogService mongoLogService)
        {
            _mongoLogService = mongoLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _mongoLogService.GetAllAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var log = await _mongoLogService.GetByIdAsync(id);
            if (log is null) return NotFound();
            return Ok(log);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationLogDocument log)
        {
            await _mongoLogService.AddAsync(log);
            return CreatedAtAction(nameof(GetById), new { id = log.Id }, log);
        }
    }
}