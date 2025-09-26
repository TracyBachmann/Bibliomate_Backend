using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD and query operations for shelf levels within the library system.
    /// Shelf levels represent the physical subdivisions (levels/étages) of a shelf.
    /// </summary>
    public interface IShelfLevelService
    {
        /// <summary>
        /// Retrieves a paged list of shelf levels, optionally filtered by a specific shelf.
        /// </summary>
        /// <param name="shelfId">
        /// Optional identifier of the parent shelf. 
        /// If <c>null</c>, all shelf levels across all shelves are returned.
        /// </param>
        /// <param name="page">
        /// Page index (1-based). Must be greater than or equal to <c>1</c>.
        /// </param>
        /// <param name="pageSize">
        /// Maximum number of items per page. Must be greater than or equal to <c>1</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{ShelfLevelReadDto}"/>
        /// containing the requested shelf levels.
        /// If no shelf levels match the filter, the result is an empty sequence.
        /// </returns>
        Task<IEnumerable<ShelfLevelReadDto>> GetAllAsync(
            int? shelfId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single shelf level by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// Identifier of the shelf level to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ShelfLevelReadDto}"/> that yields the matching <see cref="ShelfLevelReadDto"/>,
        /// or <c>null</c> if no shelf level with the specified identifier exists.
        /// </returns>
        Task<ShelfLevelReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new shelf level within a specified shelf.
        /// </summary>
        /// <param name="dto">
        /// Data transfer object containing details of the new shelf level
        /// (e.g., label, order/index, parent shelf reference).
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ShelfLevelReadDto}"/> that yields the newly created <see cref="ShelfLevelReadDto"/>.
        /// </returns>
        Task<ShelfLevelReadDto> CreateAsync(
            ShelfLevelCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing shelf level.
        /// </summary>
        /// <param name="dto">
        /// Data transfer object containing updated values for the shelf level.
        /// The identifier inside the DTO must correspond to an existing entity.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no shelf level with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            ShelfLevelUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes a shelf level.
        /// </summary>
        /// <param name="id">
        /// Identifier of the shelf level to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no shelf level with the given identifier was found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
