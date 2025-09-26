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
    /// API controller for retrieving a user’s history of domain events.
    /// Accessible only to authenticated users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Authorize]
    public class HistoriesController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoriesController"/> class.
        /// </summary>
        /// <param name="historyService">The service responsible for logging and retrieving history events.</param>
        public HistoriesController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        /// <summary>
        /// Retrieves a paged list of history events for a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose history is requested.</param>
        /// <param name="page">The page number (1-based). Default is <c>1</c>.</param>
        /// <param name="pageSize">The number of items per page. Default is <c>20</c>.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="HistoryReadDto"/>,  
        /// <c>403 Forbidden</c> if the current user is neither the owner nor has Librarian/Admin role.
        /// </returns>
        [HttpGet("user/{userId}")]
        [MapToApiVersion("1.0")]
        [SwaggerOperation(
            Summary = "Retrieves a user's history (v1)",
            Description = "Returns a paged list of history events for the specified user. " +
                          "Accessible by the user themselves, or by Librarian/Admin roles.",
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
            // Retrieve ID of the currently authenticated user
            var currentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Allow access if requesting own history or if user has elevated roles
            var isStaff = User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Librarian);
            if (currentId != userId && !isStaff)
                return Forbid();

            var history = await _historyService
                .GetHistoryForUserAsync(userId, page, pageSize, cancellationToken);

            return Ok(history);
        }
    }
}
