using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for retrieving a user’s history of domain events.
    /// </summary>
    [ApiController]
    [Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class HistoriesController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        /// <summary>
        /// Initializes a new instance of <see cref="HistoriesController"/>.
        /// </summary>
        /// <param name="historyService">Service to log and fetch history events.</param>
        public HistoriesController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// Retrieves a page of history events for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose history is requested.</param>
        /// <param name="page">Page number (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">Items per page. Default is <c>20</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with list of <see cref="HistoryReadDto"/>;  
        /// <c>403 Forbidden</c> if the current user is neither the owner nor has Librarian/Admin role.
        /// </returns>
        [HttpGet("user/{userId}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a user's history (v1)",
            Description = "Returns a paged list of history events for the specified user.",
            Tags = ["Histories"]
        )]
        [ProducesResponseType(typeof(IEnumerable<HistoryReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<HistoryReadDto>>> GetUserHistory(
            [FromRoute] int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // ID of the current authenticated user
            var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Only allow if requesting own history, or if user is Librarian or Admin
            var isStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);
            if (currentId != userId && !isStaff)
                return Forbid();

            var history = await _historyService
                .GetHistoryForUserAsync(userId, page, pageSize, cancellationToken);

            return Ok(history);
        }
    }
}