using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for managing library zones.
    /// </summary>
    public interface IZoneService
    {
        /// <summary>
        /// Retrieves a paginated list of zones.
        /// </summary>
        /// <param name="page">1-based page index.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{ZoneReadDto}"/>
        /// containing the requested page of zones.
        /// </returns>
        Task<IEnumerable<ZoneReadDto>> GetAllAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a zone by its identifier.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ZoneReadDto}"/> that yields the matching <see cref="ZoneReadDto"/>,
        /// or <c>null</c> if none exists.
        /// </returns>
        Task<ZoneReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new zone.
        /// </summary>
        /// <param name="dto">The data transfer object containing new zone details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{ZoneReadDto}"/> that yields the created <see cref="ZoneReadDto"/>.
        /// </returns>
        Task<ZoneReadDto> CreateAsync(
            ZoneCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing zone.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <param name="dto">The updated zone data transfer object.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no zone with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            ZoneUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a zone.
        /// </summary>
        /// <param name="id">The zone identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if deletion succeeded;
        /// <c>false</c> if no zone with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}