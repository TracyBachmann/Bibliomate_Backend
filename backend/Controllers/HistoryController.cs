using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly HistoryService _historyService;

        public HistoryController(HistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// GET: api/History/user/{userId}
        /// Returns the history of events for the specified user.
        /// Users can only access their own history (unless Admin/Librarian).
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<HistoryReadDto>>> GetUserHistory(int userId, int page = 1, int pageSize = 20)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdminOrStaff = User.IsInRole("Admin") || User.IsInRole("Librarian");
            if (currentUserId != userId && !isAdminOrStaff)
                return Forbid();

            var history = await _historyService.GetHistoryForUserAsync(userId, page, pageSize);
            return Ok(history);
        }
    }
}