using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing book recommendations for users.
    /// Provides endpoints to retrieve personalized book suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _service;

        /// <summary>
        /// Constructs a new <see cref="RecommendationsController"/>.
        /// </summary>
        /// <param name="service">Service providing recommendation logic.</param>
        public RecommendationsController(IRecommendationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves up to 10 recommended books for the specified user based on their preferred genres.
        /// </summary>
        /// <param name="userId">Identifier of the user to get recommendations for.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="RecommendationReadDto"/>;
        /// <c>403 Forbidden</c> if a non-admin attempts to access another user's recommendations.
        /// </returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = UserRoles.User + "," + UserRoles.Librarian + "," + UserRoles.Admin)]
        [ProducesResponseType(typeof(List<RecommendationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<RecommendationReadDto>>> GetRecommendations(
            [FromRoute] int userId,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            if (userRole != UserRoles.Admin && currentUserId != userId)
                return Forbid();

            var recommendations = await _service.GetRecommendationsForUserAsync(userId, cancellationToken);
            return Ok(recommendations);
        }
    }
}