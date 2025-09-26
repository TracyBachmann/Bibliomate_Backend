using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD and query operations for shelves.
    /// Shelves represent physical storage units within a library zone,
    /// and can contain multiple shelf levels.
    /// </summary>
    public interface IShelfService
    {
        /// <summary>
        /// Retrieves a paged list of shelves, optionally filtered by zone.
        /// </summary>
        /// <param name="zoneId">
        /// Optional identifier of the parent zone. 
        /// If <c>null</c>, shelves across all zones are returned.
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
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{ShelfReadDto}"/>
        /// containing the shelves for the requested page.
        /// If no shelves match the filter, the result is an empty sequence.
        /// </returns>
        Task<IEnumerable<ShelfReadDto>> GetAllAsync(
            int? zoneId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single shelf by its unique identifier.
        /// </summary>
        /// <param name="id">
        /// Identifier of the shelf to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ShelfReadDto}"/> that yields the matching <see cref="ShelfReadDto"/>,
        /// or <c>null</c> if no shelf with the given identifier exists.
        /// </returns>
        Task<ShelfReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new shelf within a specified zone.
        /// </summary>
        /// <param name="dto">
        /// Data transfer object containing details of the new shelf
        /// (e.g., label, code, parent zone reference).
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ShelfReadDto}"/> that yields the newly created <see cref="ShelfReadDto"/>.
        /// </returns>
        Task<ShelfReadDto> CreateAsync(
            ShelfCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing shelf.
        /// </summary>
        /// <param name="dto">
        /// Data transfer object containing updated values for the shelf.
        /// The identifier inside the DTO must correspond to an existing entity.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no shelf with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            ShelfUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes a shelf.
        /// </summary>
        /// <param name="id">
        /// Identifier of the shelf to delete.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no shelf with the given identifier was found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
