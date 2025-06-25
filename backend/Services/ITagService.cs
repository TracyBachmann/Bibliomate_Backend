using backend.DTOs;

namespace backend.Services
{
    /// <summary>
    /// Defines CRUD operations for tags.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        /// <returns>A collection of <see cref="TagReadDto"/>.</returns>
        Task<IEnumerable<TagReadDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <returns>The <see cref="TagReadDto"/> if found; otherwise null.</returns>
        Task<TagReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="dto">The data to create the tag.</param>
        /// <returns>The created <see cref="TagReadDto"/>.</returns>
        Task<TagReadDto> CreateAsync(TagCreateDto dto);

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <param name="dto">The updated tag data.</param>
        /// <returns>True if update succeeded; false if tag not found.</returns>
        Task<bool> UpdateAsync(TagUpdateDto dto);

        /// <summary>
        /// Deletes a tag by its identifier.
        /// </summary>
        /// <param name="id">The tag identifier.</param>
        /// <returns>True if deletion succeeded; false if tag not found.</returns>
        Task<bool> DeleteAsync(int id);
    }
}