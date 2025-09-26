using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendBiblioMate.Controllers
{
    /// <summary>
    /// Controller for managing book recommendations for users.
    /// Provides endpoints to retrieve personalized book suggestions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
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
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Retrieves up to 10 recommended books for the specified user based on their preferred genres.
        /// </summary>
        /// <param name="userId">Identifier of the user to get recommendations for.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><c>200 OK</c> with a list of <see cref="RecommendationReadDto"/>.</description></item>
        /// <item><description><c>403 Forbidden</c> if a non-admin attempts to access another user's recommendations.</description></item>
        /// <item><description><c>401 Unauthorized</c> if the request does not contain valid authentication.</description></item>
        /// </list>
        /// </returns>
        [HttpGet("user/{userId}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = UserRoles.User + "," + UserRoles.Librarian + "," + UserRoles.Admin)]
        [SwaggerOperation(
            Summary = "Retrieves recommended books for a user (v1)",
            Description = "Returns up to 10 personalized book recommendations based on preferred genres.",
            Tags = ["Recommendations"]
        )]
        [ProducesResponseType(typeof(List<RecommendationReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RecommendationReadDto>>> GetRecommendations(
            [FromRoute] int userId,
            CancellationToken cancellationToken = default)
        {
            var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(currentUserIdClaim) || string.IsNullOrEmpty(currentUserRole))
                return Unauthorized();

            var currentUserId = int.Parse(currentUserIdClaim);

            // Only allow if same user or Admin role
            if (currentUserRole != UserRoles.Admin && currentUserId != userId)
                return Forbid();

            var recommendations = await _service.GetRecommendationsForUserAsync(userId, cancellationToken);
            return Ok(recommendations);
        }
    }
}