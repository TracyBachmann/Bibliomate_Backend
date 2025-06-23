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
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly RecommendationService _service;

        /// <summary>
        /// Constructs a RecommendationsController with the specified service.
        /// </summary>
        /// <param name="service">The recommendation service.</param>
        public RecommendationsController(RecommendationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets a list of recommended books for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of recommended books.</returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "User,Librarian,Admin")]
        public async Task<ActionResult<List<RecommendationReadDto>>> GetRecommendations(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Interdiction de consulter les recommandations d’un autre utilisateur sauf si Admin
            if (userRole != "Admin" && currentUserId != userId)
                return Forbid();

            var recommendations = await _service.GetRecommendationsForUser(userId);
            return Ok(recommendations);
        }
    }
}