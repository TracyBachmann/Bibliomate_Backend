using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines operations for querying, retrieving and mutating book data,
    /// including paging, sorting, ETag support, and search activity logging.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Returns a paged list of books, sorted and projected to <see cref="BookReadDto"/>,
        /// including an ETag value and an optional 304 Not Modified result.
        /// </summary>
        /// <param name="pageNumber">Page number (1-based) to retrieve.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="sortBy">Field name to sort by (e.g. "Title", "PublicationYear").</param>
        /// <param name="ascending"><c>true</c> for ascending order; <c>false</c> for descending.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding a tuple:
        /// <list type="bullet">
        ///   <item><description>A <see cref="PagedResult{BookReadDto}"/> with the requested page.</description></item>
        ///   <item><description>A <c>string</c> ETag value for the page.</description></item>
        ///   <item><description>An <see cref="IActionResult"/> with <c>304 Not Modified</c> if unchanged; otherwise <c>null</c>.</description></item>
        /// </list>
        /// </returns>
        Task<(PagedResult<BookReadDto> Page, string ETag, IActionResult? NotModified)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string sortBy,
            bool ascending,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a single book by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the book to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding the <see cref="BookReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<BookReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new book record.
        /// </summary>
        /// <param name="dto">Data transfer object containing book properties.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding the created <see cref="BookReadDto"/>.
        /// </returns>
        Task<BookReadDto> CreateAsync(
            BookCreateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="id">Identifier of the book to update.</param>
        /// <param name="dto">Data transfer object with updated values.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding <c>true</c> if the update succeeded; <c>false</c> if the book was not found.
        /// </returns>
        Task<bool> UpdateAsync(
            int id,
            BookUpdateDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a book by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the book to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding <c>true</c> if deletion succeeded; <c>false</c> if the book was not found.
        /// </returns>
        Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs a filtered search over books based on optional criteria.
        /// </summary>
        /// <param name="dto">Search criteria DTO (title, author, genre, etc.).</param>
        /// <param name="userId">Optional identifier of the performing user, used for logging.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding the collection of matching <see cref="BookReadDto"/> items.
        /// </returns>
        Task<IEnumerable<BookReadDto>> SearchAsync(
            BookSearchDto dto,
            int? userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the list of all distinct book genres.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task yielding a read-only list of genre names.
        /// </returns>
        Task<IReadOnlyList<string>> GetAllGenresAsync(
            CancellationToken cancellationToken = default);
    }
}
