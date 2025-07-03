using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD and query operations for shelves.
    /// </summary>
    public interface IShelfService
    {
        /// <summary>
        /// Retrieves a paged list of shelves, optionally filtered by zone.
        /// </summary>
        /// <param name="zoneId">Optional zone identifier to filter by.</param>
        /// <param name="page">Page index (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{ShelfReadDto}"/>
        /// containing the matching shelves.
        /// </returns>
        Task<IEnumerable<ShelfReadDto>> GetAllAsync(
            int? zoneId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single shelf by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ShelfReadDto}"/> that yields the matching <see cref="ShelfReadDto"/>,
        /// or <c>null</c> if not found.
        /// </returns>
        Task<ShelfReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new shelf.
        /// </summary>
        /// <param name="dto">Data transfer object containing new shelf details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ShelfReadDto}"/> that yields the created <see cref="ShelfReadDto"/>.
        /// </returns>
        Task<ShelfReadDto> CreateAsync(
            ShelfCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing shelf.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated shelf values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no shelf with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            ShelfUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a shelf by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the shelf to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no shelf with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}