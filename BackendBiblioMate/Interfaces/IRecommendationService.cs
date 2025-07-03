using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that returns a <see cref="List{RecommendationReadDto}"/>
        /// containing up to ten recommended books for the user.
        /// </returns>
        Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}