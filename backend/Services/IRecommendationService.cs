using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines operations to generate book recommendations for users.
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Retrieves a list of recommended books for the specified user,
        /// selecting up to 10 titles that match the user's preferred genres.
        /// </summary>
        /// <param name="userId">The identifier of the user to get recommendations for.</param>
        /// <returns>
        /// A task that returns a list of <see cref="RecommendationReadDto"/> containing book recommendations.
        /// </returns>
        Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(int userId);
    }
}