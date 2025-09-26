using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines the contract for managing library <c>Zone</c> entities.
    /// A zone typically represents a physical section of the library
    /// (e.g., floor, aisle, or dedicated thematic area).
    /// Provides standard CRUD operations with support for pagination.
    /// </summary>
    public interface IZoneService
    {
        /// <summary>
        /// Retrieves a paginated list of zones.
        /// Results are typically ordered by their unique identifier.
        /// </summary>
        /// <param name="page">1-based page index (must be &gt;= 1).</param>
        /// <param name="pageSize">Number of items per page (must be &gt;= 1).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> producing an <see cref="IEnumerable{ZoneReadDto}"/>
        /// containing the requested subset of zones.
        /// </returns>
        Task<IEnumerable<ZoneReadDto>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a single zone by its identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the zone.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{ZoneReadDto}"/> that yields the matching <see cref="ZoneReadDto"/>
        /// if found; otherwise <c>null</c>.
        /// </returns>
        Task<ZoneReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new zone in the library catalog.
        /// </summary>
        /// <param name="dto">The data transfer object containing zone details (e.g. name, location info).</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{ZoneReadDto}"/> that yields the newly created <see cref="ZoneReadDto"/>
        /// including its generated identifier.
        /// </returns>
        Task<ZoneReadDto> CreateAsync(
            ZoneCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        /// <param name="id">The identifier of the zone to update.</param>
        /// <param name="dto">The updated zone data transfer object.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> yielding <c>true</c> if the update succeeded;
        /// <c>false</c> if no zone with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            ZoneUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a zone from the system.
        /// </summary>
        /// <param name="id">The identifier of the zone to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests (optional).</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> yielding <c>true</c> if deletion succeeded;
        /// <c>false</c> if no zone with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
