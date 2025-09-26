using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD and query operations for <see cref="TagReadDto"/> entities.
    /// Tags are keywords associated with books that allow
    /// thematic classification, filtering, and advanced search.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Retrieves all tags in the system.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A collection of <see cref="TagReadDto"/> representing all tags.
        /// </returns>
        Task<IEnumerable<TagReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific tag by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the tag to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The matching <see cref="TagReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<TagReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="dto">The data transfer object containing tag details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The created <see cref="TagReadDto"/>.
        /// </returns>
        Task<TagReadDto> CreateAsync(
            TagCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <param name="dto">The DTO containing the updated tag values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update succeeded; <c>false</c> if the tag was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            TagUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a tag by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the tag to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion succeeded; <c>false</c> if the tag was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        // ===== Extended operations =====

        /// <summary>
        /// Searches for tags matching a given term.
        /// </summary>
        /// <param name="search">The optional search string (partial match supported).</param>
        /// <param name="take">The maximum number of results to return.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A collection of <see cref="TagReadDto"/> that match the search criteria.
        /// </returns>
        Task<IEnumerable<TagReadDto>> SearchAsync(
            string? search,
            int take,
            CancellationToken ct);

        /// <summary>
        /// Ensures that a tag with the specified name exists.
        /// If the tag already exists, returns it; otherwise, creates a new one.
        /// </summary>
        /// <param name="name">The name of the tag to find or create.</param>
        /// <param name="ct">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><see cref="TagReadDto"/> — the existing or newly created tag.</item>
        ///   <item><c>Created</c> — <c>true</c> if the tag was newly created; <c>false</c> if it already existed.</item>
        /// </list>
        /// </returns>
        Task<(TagReadDto Dto, bool Created)> EnsureAsync(
            string name,
            CancellationToken ct);
    }
}
