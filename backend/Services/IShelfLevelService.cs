using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines CRUD and query operations for shelf levels.
    /// </summary>
    public interface IShelfLevelService
    {
        /// <summary>
        /// Retrieves a paged list of shelf levels, optionally filtered by shelf.
        /// </summary>
        /// <param name="shelfId">Optional shelf identifier to filter by.</param>
        /// <param name="page">Page index (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        Task<IEnumerable<ShelfLevelReadDto>> GetAllAsync(int? shelfId, int page, int pageSize);

        /// <summary>
        /// Retrieves a single shelf level by its identifier.
        /// </summary>
        /// <param name="id">ShelfLevel identifier.</param>
        Task<ShelfLevelReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new shelf level.
        /// </summary>
        /// <param name="dto">Data for the new shelf level.</param>
        Task<ShelfLevelReadDto> CreateAsync(ShelfLevelCreateDto dto);

        /// <summary>
        /// Updates an existing shelf level.
        /// </summary>
        /// <param name="dto">Data for the shelf level to update.</param>
        /// <returns>True if updated; false if not found.</returns>
        Task<bool> UpdateAsync(ShelfLevelUpdateDto dto);

        /// <summary>
        /// Deletes a shelf level by its identifier.
        /// </summary>
        /// <param name="id">ShelfLevel identifier.</param>
        /// <returns>True if deleted; false if not found.</returns>
        Task<bool> DeleteAsync(int id);
    }
}