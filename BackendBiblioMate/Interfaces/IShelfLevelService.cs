using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{ShelfLevelReadDto}"/>
        /// containing the matching shelf levels.
        /// </returns>
        Task<IEnumerable<ShelfLevelReadDto>> GetAllAsync(
            int? shelfId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single shelf level by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf level to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ShelfLevelReadDto}"/> that yields the matching <see cref="ShelfLevelReadDto"/>,
        /// or <c>null</c> if not found.
        /// </returns>
        Task<ShelfLevelReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new shelf level.
        /// </summary>
        /// <param name="dto">Data transfer object containing new shelf level details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ShelfLevelReadDto}"/> that yields the created <see cref="ShelfLevelReadDto"/>.
        /// </returns>
        Task<ShelfLevelReadDto> CreateAsync(
            ShelfLevelCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing shelf level.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated shelf level values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no shelf level with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            ShelfLevelUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a shelf level by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf level to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no shelf level with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}