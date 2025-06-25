using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for retrieving a user’s history of domain events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        /// <summary>
        /// Initializes a new instance of <see cref="HistoryController"/>.
        /// </summary>
        /// <param name="historyService">Service to log and fetch history events.</param>
        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// Retrieves a page of history events for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose history is requested.</param>
        /// <param name="page">Page number (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>20</c>.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="HistoryReadDto"/>;  
        /// <c>403 Forbidden</c> if the current user is neither the owner nor an Admin/Librarian.
        /// </returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<HistoryReadDto>>> GetUserHistory(
            int userId,
            int page = 1,
            int pageSize = 20)
        {
            var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdminOrStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);

            if (currentId != userId && !isAdminOrStaff)
                return Forbid();

            var history = await _historyService.GetHistoryForUserAsync(userId, page, pageSize);
            return Ok(history);
        }
    }
}