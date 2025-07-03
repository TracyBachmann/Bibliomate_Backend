using BackendBiblioMate.DTOs;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines CRUD operations for tags.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that yields an <see cref="IEnumerable{TagReadDto}"/>
        /// containing all tags.
        /// </returns>
        Task<IEnumerable<TagReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TagReadDto}"/> that yields the matching <see cref="TagReadDto"/>,
        /// or <c>null</c> if no tag with the given identifier exists.
        /// </returns>
        Task<TagReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="dto">Data transfer object containing new tag details.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{TagReadDto}"/> that yields the created <see cref="TagReadDto"/>.
        /// </returns>
        Task<TagReadDto> CreateAsync(
            TagCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <param name="dto">Data transfer object containing updated tag values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the update succeeded;
        /// <c>false</c> if no tag with the given identifier exists.
        /// </returns>
        Task<bool> UpdateAsync(
            TagUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task{Boolean}"/> that yields <c>true</c> if the deletion succeeded;
        /// <c>false</c> if no tag with the given identifier exists.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}