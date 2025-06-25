using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    /// <summary>
    /// API controller that handles book recommendations for users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _service;

        /// <summary>
        /// Constructs a <see cref="RecommendationsController"/> with the specified recommendation service.
        /// </summary>
        /// <param name="service">The recommendation service.</param>
        public RecommendationsController(IRecommendationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets a list of recommended books for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user to retrieve recommendations for.</param>
        /// <returns>
        /// <c>200 OK</c> with a list of <see cref="RecommendationReadDto"/> on success;
        /// <c>403 Forbidden</c> if a non-admin attempts to view another user's recommendations.
        /// </returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "User,Librarian,Admin")]
        public async Task<ActionResult<List<RecommendationReadDto>>> GetRecommendations(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole      = User.FindFirst(ClaimTypes.Role)!.Value;

            // Prevent non-admins from accessing other users' recommendations
            if (userRole != "Admin" && currentUserId != userId)
                return Forbid();

            var recommendations = await _service.GetRecommendationsForUserAsync(userId);
            return Ok(recommendations);
        }
    }
}