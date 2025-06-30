using backend.Models.Mongo;

namespace backend.Services
{
    /// <summary>
    /// Defines operations to record and retrieve search activity logs.
    /// </summary>
    public interface ISearchActivityLogService
    {
        /// <summary>
        /// Inserts a new search activity log document into the data store.
        /// </summary>
        /// <param name="doc">
        /// The <see cref="SearchActivityLogDocument"/> containing UserId, QueryText and Timestamp.
        /// </param>
        /// <returns>A <see cref="Task"/> that completes when the document has been inserted.</returns>
        Task LogAsync(SearchActivityLogDocument doc);

        /// <summary>
        /// Retrieves all search log documents for a specific user, ordered from newest to oldest.
        /// </summary>
        /// <param name="userId">The identifier of the user whose search logs are requested.</param>
        /// <returns>
        /// A <see cref="Task{List}"/> producing a <see cref="List{SearchActivityLogDocument}"/> 
        /// sorted by descending <c>Timestamp</c>.
        /// </returns>
        Task<List<SearchActivityLogDocument>> GetByUserAsync(int userId);
    }
}