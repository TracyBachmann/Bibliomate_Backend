using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Implements <see cref="IBookService"/> by coordinating EF Core,
    /// projection to DTO, pagination, sorting, ETag generation, and search‐activity logging.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly BiblioMateDbContext       _db;
        private readonly ISearchActivityLogService _searchLog;

        /// <summary>
        /// Initializes a new instance of <see cref="BookService"/>.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        /// <param name="searchLog">Service for logging search activities.</param>
        public BookService(
            BiblioMateDbContext       db,
            ISearchActivityLogService searchLog)
        {
            _db        = db;
            _searchLog = searchLog;
        }

        /// <summary>
        /// Retrieves a paged list of books, sorted and with an ETag for caching.
        /// </summary>
        /// <param name="pageNumber">Number of the page to retrieve (1-based).</param>
        /// <param name="pageSize">Size of each page.</param>
        /// <param name="sortBy">Field to sort by (e.g. "Title", "PublicationYear").</param>
        /// <param name="ascending">True for ascending order; false for descending.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><see cref="PagedResult{BookReadDto}"/> with the page of items.</item>
        ///   <item>ETag string based on the page content.</item>
        ///   <item><see cref="IActionResult"/> <c>NotModified</c> if the client cache is fresh; otherwise null.</item>
        /// </list>
        /// </returns>
        public async Task<(PagedResult<BookReadDto> Page, string ETag, IActionResult? NotModified)>
            GetPagedAsync(
                int pageNumber,
                int pageSize,
                string sortBy,
                bool ascending,
                CancellationToken cancellationToken = default)
        {
            var query = _db.Books
                .AsNoTracking()
                .Select(BookToDtoExpression);

            query = (sortBy, ascending) switch
            {
                ("BookId", true)           => query.OrderBy(d => d.BookId),
                ("BookId", false)          => query.OrderByDescending(d => d.BookId),

                ("PublicationYear", true)  => query.OrderBy(d => d.PublicationYear),
                ("PublicationYear", false) => query.OrderByDescending(d => d.PublicationYear),

                ("Title",     false)       => query.OrderByDescending(d => d.Title),
                _                          => query.OrderBy(d => d.Title)
            };

            var page = await query
                .ToPagedResultAsync(pageNumber, pageSize, cancellationToken)
                .ConfigureAwait(false);

            // Incorporate Description into ETag source to catch description changes
            var eTagSource = string.Join(";", page.Items.Select(i =>
                $"{i.BookId}-{i.Title}-{(i.Description ?? string.Empty)}"));
            var hash       = MD5.HashData(Encoding.UTF8.GetBytes(eTagSource));
            var eTag       = $"\"{Convert.ToBase64String(hash)}\"";

            return (page, eTag, null);
        }

        /// <summary>
        /// Retrieves a single book by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the book to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// The <see cref="BookReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<BookReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var book = await _db.Books
                .AsNoTracking()
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .FirstOrDefaultAsync(b => b.BookId == id, cancellationToken)
                .ConfigureAwait(false);

            return book == null
                ? null
                : BookToDtoExpression.Compile().Invoke(book);
        }

        /// <summary>
        /// Creates a new book in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing book creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>The created <see cref="BookReadDto"/>.</returns>
        public async Task<BookReadDto> CreateAsync(
            BookCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var book = new Book
            {
                Title           = dto.Title,
                Isbn            = dto.Isbn,
                Description     = dto.Description,
                PublicationDate = dto.PublicationDate,
                AuthorId        = dto.AuthorId,
                GenreId         = dto.GenreId,
                EditorId        = dto.EditorId,
                ShelfLevelId    = dto.ShelfLevelId,
                CoverUrl        = dto.CoverUrl,
                BookTags        = (dto.TagIds ?? new List<int>())
                    .Select(tagId => new BookTag { TagId = tagId })
                    .ToList()
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await _db.Entry(book).Reference(b => b.Author).LoadAsync(cancellationToken).ConfigureAwait(false);
            await _db.Entry(book).Reference(b => b.Genre).LoadAsync(cancellationToken).ConfigureAwait(false);
            await _db.Entry(book).Reference(b => b.Editor).LoadAsync(cancellationToken).ConfigureAwait(false);
            await _db.Entry(book).Reference(b => b.ShelfLevel).LoadAsync(cancellationToken).ConfigureAwait(false);
            await _db.Entry(book)
                     .Collection(b => b.BookTags)
                     .Query()
                     .Include(bt => bt.Tag)
                     .LoadAsync(cancellationToken)
                     .ConfigureAwait(false);
            await _db.Entry(book).Reference(b => b.Stock).LoadAsync(cancellationToken).ConfigureAwait(false);

            return BookToDtoExpression.Compile().Invoke(book);
        }

        /// <summary>
        /// Updates an existing book in the data store.
        /// </summary>
        /// <param name="id">Identifier of the book to update.</param>
        /// <param name="dto">Data transfer object containing updated book data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if the update was successful; <c>false</c> if not found or ID mismatch.</returns>
        public async Task<bool> UpdateAsync(
            int id,
            BookUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            if (id != dto.BookId)
                return false;

            var book = await _db.Books
                .Include(b => b.BookTags)
                .FirstOrDefaultAsync(b => b.BookId == id, cancellationToken)
                .ConfigureAwait(false);

            if (book == null)
                return false;

            book.Title           = dto.Title;
            book.Isbn            = dto.Isbn;
            book.Description     = dto.Description;
            book.PublicationDate = dto.PublicationDate;
            book.AuthorId        = dto.AuthorId;
            book.GenreId         = dto.GenreId;
            book.EditorId        = dto.EditorId;
            book.ShelfLevelId    = dto.ShelfLevelId;

            _db.BookTags.RemoveRange(book.BookTags);
            if (dto.TagIds?.Any() == true)
            {
                var newTags = dto.TagIds.Select(tagId => new BookTag { BookId = id, TagId = tagId });
                await _db.BookTags.AddRangeAsync(newTags, cancellationToken).ConfigureAwait(false);
            }

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Deletes a book from the data store.
        /// </summary>
        /// <param name="id">Identifier of the book to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if the deletion was successful; <c>false</c> if not found.</returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var book = await _db.Books.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (book == null)
                return false;

            _db.Books.Remove(book);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Searches books by various criteria and logs the activity.
        /// </summary>
        /// <param name="dto">Search criteria.</param>
        /// <param name="userId">Optional user identifier for logging.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="IEnumerable{BookReadDto}"/> of matching books.</returns>
        public async Task<IEnumerable<BookReadDto>> SearchAsync(
            BookSearchDto dto,
            int? userId,
            CancellationToken cancellationToken = default)
        {
            await _searchLog.LogAsync(
                new Models.Mongo.SearchActivityLogDocument
                {
                    UserId    = userId,
                    QueryText = dto.ToString()!
                },
                cancellationToken).ConfigureAwait(false);

            var query = _db.Books
                .AsNoTracking()
                .Include(b => b.Genre)
                .Include(b => b.Author)
                .Include(b => b.Editor)
                .Include(b => b.ShelfLevel)
                .Include(b => b.BookTags).ThenInclude(bt => bt.Tag)
                .Include(b => b.Stock)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                query = query.Where(b => b.Title.Contains(dto.Title));
            if (!string.IsNullOrWhiteSpace(dto.Author))
                query = query.Where(b => b.Author.Name.Contains(dto.Author));
            if (!string.IsNullOrWhiteSpace(dto.Genre))
                query = query.Where(b => b.Genre.Name.Contains(dto.Genre));
            if (!string.IsNullOrWhiteSpace(dto.Publisher))
                query = query.Where(b => b.Editor.Name.Contains(dto.Publisher));
            if (!string.IsNullOrWhiteSpace(dto.Isbn))
                query = query.Where(b => b.Isbn.Contains(dto.Isbn));
            if (dto.YearMin.HasValue)
                query = query.Where(b => b.PublicationDate >= new DateTime(dto.YearMin.Value, 1, 1));
            if (dto.YearMax.HasValue)
                query = query.Where(b => b.PublicationDate <= new DateTime(dto.YearMax.Value, 12, 31));
            if (dto.IsAvailable.HasValue)
                query = query.Where(b => b.Stock != null && b.Stock.IsAvailable == dto.IsAvailable.Value);
            if (dto.TagIds.Any())
                query = query.Where(b => b.BookTags.Any(bt => dto.TagIds.Contains(bt.TagId)));
            if (!string.IsNullOrWhiteSpace(dto.Description))
                query = query.Where(b => b.Description != null && b.Description.Contains(dto.Description));

            var list = await query
                .Select(BookToDtoExpression)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return list;
        }

        /// <summary>
        /// Expression to project <see cref="Book"/> into <see cref="BookReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Book,BookReadDto>> BookToDtoExpression = book => new BookReadDto
        {
            BookId          = book.BookId,
            Title           = book.Title,
            Isbn            = book.Isbn,
            Description     = book.Description,
            PublicationYear = book.PublicationDate.Year,
            AuthorName      = book.Author.Name,
            GenreName       = book.Genre.Name,
            EditorName      = book.Editor.Name,
            IsAvailable     = book.Stock != null && book.Stock.IsAvailable,
            CoverUrl        = book.CoverUrl,
            Tags            = book.BookTags.Select(bt => bt.Tag.Name).ToList()
        };
    }
}
