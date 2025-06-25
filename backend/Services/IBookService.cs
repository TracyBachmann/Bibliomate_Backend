using backend.DTOs;
using backend.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services
{
    /// <summary>
    /// Defines operations for querying, retrieving and mutating book data,
    /// including paging, sorting, ETag support and search activity logging.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Returns a paged list of books, sorted, projected to <see cref="BookReadDto"/>,
        /// including an ETag value and an optional 304 result if not modified.
        /// </summary>
        /// <param name="pageNumber">1-based page index.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="sortBy">Field to sort by (e.g. "Title" or "PublicationYear").</param>
        /// <param name="ascending">True for ascending order, false for descending.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description>
        ///     <see cref="PagedResult{BookReadDto}"/>: the paged data.
        ///   </description></item>
        ///   <item><description>
        ///     <c>string</c> ETag value for the response header.
        ///   </description></item>
        ///   <item><description>
        ///     <see cref="IActionResult"/>? representing a 304-NotModified response
        ///     (null if a full 200 should be returned).
        ///   </description></item>
        /// </list>
        /// </returns>
        Task<(PagedResult<BookReadDto> Page, string ETag, IActionResult? NotModified)>
            GetPagedAsync(int pageNumber, int pageSize, string sortBy, bool ascending);

        /// <summary>
        /// Finds a single book by its identifier.
        /// </summary>
        /// <param name="id">Book primary key.</param>
        Task<BookReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new book record.
        /// </summary>
        /// <param name="dto">Data required to create the book.</param>
        Task<BookReadDto> CreateAsync(BookCreateDto dto);

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="id">Identifier of the book to update.</param>
        /// <param name="dto">Updated book data.</param>
        Task<bool> UpdateAsync(int id, BookUpdateDto dto);

        /// <summary>
        /// Deletes a book by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the book to delete.</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Performs a filtered search over books based on optional criteria.
        /// </summary>
        /// <param name="dto">Search filters.</param>
        /// <param name="userId">
        /// Optional ID of the user performing the search, for logging purposes.
        /// </param>
        Task<IEnumerable<BookReadDto>> SearchAsync(BookSearchDto dto, int? userId);
    }
}
