using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for retrieving and ensuring library location data,
    /// including floors, aisles, shelves, and shelf levels.
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Retrieves all floors in the library.
        /// </summary>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding an <see cref="IEnumerable{FloorReadDto}"/>
        /// containing all floor metadata.
        /// </returns>
        Task<IEnumerable<FloorReadDto>> GetFloorsAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves all aisles for a specific floor.
        /// </summary>
        /// <param name="floorNumber">The floor number where aisles are located.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding an <see cref="IEnumerable{AisleReadDto}"/>
        /// containing aisle metadata for the specified floor.
        /// </returns>
        Task<IEnumerable<AisleReadDto>> GetAislesAsync(
            int floorNumber,
            CancellationToken ct = default);

        /// <summary>
        /// Retrieves all shelves in a given aisle on a specific floor.
        /// </summary>
        /// <param name="floorNumber">The floor number containing the aisle.</param>
        /// <param name="aisleCode">The unique code of the aisle.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding an <see cref="IEnumerable{ShelfMiniReadDto}"/>
        /// containing shelf metadata for the specified aisle.
        /// </returns>
        Task<IEnumerable<ShelfMiniReadDto>> GetShelvesAsync(
            int floorNumber,
            string aisleCode,
            CancellationToken ct = default);

        /// <summary>
        /// Retrieves all shelf levels for a given shelf.
        /// </summary>
        /// <param name="shelfId">Identifier of the shelf.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> yielding an <see cref="IEnumerable{LevelReadDto}"/>
        /// containing all levels for the given shelf.
        /// </returns>
        Task<IEnumerable<LevelReadDto>> GetLevelsAsync(
            int shelfId,
            CancellationToken ct = default);

        /// <summary>
        /// Ensures that a complete location hierarchy exists
        /// (Zone → Shelf → ShelfLevel).
        /// Creates missing entities if they do not already exist
        /// and returns the full resolved location.
        /// </summary>
        /// <param name="dto">Location definition DTO specifying floor, aisle, shelf, and level.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{LocationReadDto}"/> yielding the fully created or resolved location.
        /// </returns>
        Task<LocationReadDto> EnsureAsync(
            LocationEnsureDto dto,
            CancellationToken ct = default);
    }
}
