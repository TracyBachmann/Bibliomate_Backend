using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations to generate and retrieve personalized book recommendations for users.
    /// Recommendations are typically based on user preferences such as favorite genres,
    /// reading history, or system-wide popularity.
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Retrieves a curated list of recommended books for the specified user.
        /// The recommendation algorithm selects up to 10 titles that best match
        /// the user's preferred genres or other configured criteria.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user for whom to generate recommendations.
        /// Must correspond to a valid user in the system.
        /// </param>
        /// <param name="cancellationToken">
        /// A token to observe cancellation requests during the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that, when completed successfully, yields a
        /// <see cref="List{RecommendationReadDto}"/> containing up to ten recommended books.
        /// The list may be empty if no suitable matches are found.
        /// </returns>
        Task<List<RecommendationReadDto>> GetRecommendationsForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}