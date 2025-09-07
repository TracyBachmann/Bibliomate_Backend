// BackendBiblioMate/Services/Catalog/BookService.cs
using System.Linq.Expressions;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    public class BookService : IBookService
    {
        private readonly BiblioMateDbContext _db;
        private readonly ISearchActivityLogService? _searchLog;

        public BookService(
            BiblioMateDbContext db,
            ISearchActivityLogService? searchLog)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _searchLog = searchLog; // peut être null si non configuré
        }

        #region Projection unique -> DTO

        /// <summary>
        /// Projection unique réutilisée partout (liste/détail/recherche).
        /// Localisation : Zone/Shelf/ShelfLevel.
        /// Disponibilité : Quantity - prêts actifs (ReturnDate == null) > 0.
        /// IMPORTANT : on compte une clé non nulle (LoanId) pour éviter le faux "1" dû aux LEFT JOIN.
        /// </summary>
        private Expression<Func<Book, BookReadDto>> ReadProjection => b => new BookReadDto
        {
            BookId = b.BookId,
            Title = b.Title,
            Isbn = b.Isbn,
            PublicationYear = b.PublicationDate.Year,
            AuthorName = b.Author.Name,
            GenreName = b.Genre.Name,
            EditorName = b.Editor.Name,

            // Dispo calculée côté SQL : (stock - prêts actifs) > 0
            // ► Stock: 1ère quantité trouvée ou 0 si pas de stock
            // ► Prêts actifs: COUNT(LoanId) avec filtre ReturnDate IS NULL (COUNT ignore les NULL)
            IsAvailable =
                (
                    (_db.Stocks
                        .Where(s => s.BookId == b.BookId)
                        .Select(s => (int?)s.Quantity)
                        .FirstOrDefault() ?? 0)
                    -
                    _db.Loans
                        .Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                        .Select(l => (int?)l.LoanId)
                        .Count()
                ) > 0,

            CoverUrl = b.CoverUrl,
            Description = b.Description,

            // Localisation aplatie
            Floor = b.ShelfLevel.Shelf.Zone.FloorNumber,
            Aisle = b.ShelfLevel.Shelf.Zone.AisleCode,
            Rayon = b.ShelfLevel.Shelf.Name,
            Shelf = b.ShelfLevel.LevelNumber,

            // Tags
            Tags = b.BookTags.Select(bt => bt.Tag.Name).ToList()
        };

        #endregion

        #region Lecture

        public async Task<(PagedResult<BookReadDto> Page, string? ETag, IActionResult? NotModified)>
            GetPagedAsync(int pageNumber, int pageSize, string sortBy, bool ascending, CancellationToken ct = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var baseQuery = _db.Books.AsNoTracking();

            // Tri
            IOrderedQueryable<Book> ordered = (sortBy ?? "Title").ToLowerInvariant() switch
            {
                "bookid" or "id" => ascending ? baseQuery.OrderBy(b => b.BookId) : baseQuery.OrderByDescending(b => b.BookId),
                "title" => ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title),
                "isbn" => ascending ? baseQuery.OrderBy(b => b.Isbn) : baseQuery.OrderByDescending(b => b.Isbn),
                "publicationdate" or "publicationyear" => ascending ? baseQuery.OrderBy(b => b.PublicationDate) : baseQuery.OrderByDescending(b => b.PublicationDate),
                _ => ascending ? baseQuery.OrderBy(b => b.Title) : baseQuery.OrderByDescending(b => b.Title)
            };

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await ordered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ReadProjection)
                .ToListAsync(ct);

            var page = PagedResult<BookReadDto>.Create(items, pageNumber, pageSize, totalCount);

            return (page, null, null); // pas d'ETag dans cette version
        }

        public async Task<BookReadDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
            await _db.Books
                .AsNoTracking()
                .Where(b => b.BookId == id)
                .Select(ReadProjection)
                .SingleOrDefaultAsync(ct);

        public async Task<IEnumerable<BookReadDto>> SearchAsync(
            BookSearchDto dto,
            int? userId,
            CancellationToken ct = default)
        {
            var q = _db.Books.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                var t = dto.Title.Trim();
                q = q.Where(b => EF.Functions.Like(b.Title, $"%{t}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Author))
            {
                var a = dto.Author.Trim();
                q = q.Where(b => EF.Functions.Like(b.Author.Name, $"%{a}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Genre))
            {
                var g = dto.Genre.Trim();
                q = q.Where(b => EF.Functions.Like(b.Genre.Name, $"%{g}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Publisher))
            {
                var p = dto.Publisher.Trim();
                q = q.Where(b => EF.Functions.Like(b.Editor.Name, $"%{p}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Isbn))
            {
                var i = dto.Isbn.Trim();
                q = q.Where(b => b.Isbn == i);
            }

            if (dto.YearMin.HasValue) q = q.Where(b => b.PublicationDate.Year >= dto.YearMin.Value);
            if (dto.YearMax.HasValue) q = q.Where(b => b.PublicationDate.Year <= dto.YearMax.Value);

            // ► Filtre "disponible / indisponible" avec comptage robuste (COUNT(LoanId))
            if (dto.IsAvailable.HasValue)
            {
                if (dto.IsAvailable.Value)
                {
                    q = q.Where(b =>
                        (
                            (_db.Stocks
                                .Where(s => s.BookId == b.BookId)
                                .Select(s => (int?)s.Quantity)
                                .FirstOrDefault() ?? 0)
                            -
                            _db.Loans
                                .Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                                .Select(l => (int?)l.LoanId)
                                .Count()
                        ) > 0);
                }
                else
                {
                    q = q.Where(b =>
                        (
                            (_db.Stocks
                                .Where(s => s.BookId == b.BookId)
                                .Select(s => (int?)s.Quantity)
                                .FirstOrDefault() ?? 0)
                            -
                            _db.Loans
                                .Where(l => l.BookId == b.BookId && l.ReturnDate == null)
                                .Select(l => (int?)l.LoanId)
                                .Count()
                        ) <= 0);
                }
            }

            if (dto.TagIds is { Count: > 0 })
            {
                var ids = dto.TagIds.Distinct().ToArray();
                q = q.Where(b => b.BookTags.Any(bt => ids.Contains(bt.TagId)));
            }

            if (dto.TagNames is { Count: > 0 })
            {
                var names = dto.TagNames
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToArray();

                if (names.Length > 0)
                    q = q.Where(b => b.BookTags.Any(bt => names.Contains(bt.Tag.Name)));
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                var d = dto.Description.Trim();
                q = q.Where(b => b.Description != null && EF.Functions.Like(b.Description, $"%{d}%"));
            }

            if (!string.IsNullOrWhiteSpace(dto.Exclude))
            {
                var ex = dto.Exclude.Trim();
                q = q.Where(b =>
                    (b.Description == null || !EF.Functions.Like(b.Description, $"%{ex}%")) &&
                    !EF.Functions.Like(b.Title, $"%{ex}%"));
            }

            var results = await q.Select(ReadProjection).ToListAsync(ct);

            // 🔸 Logging optionnel désactivé (signature inconnue dans ton projet)
            // if (userId.HasValue && _searchLog != null)
            // {
            //     try { await _searchLog.LogAsync(userId.Value, dto, results.Count, ct); }
            //     catch { /* non bloquant */ }
            // }

            return results;
        }

        public async Task<IReadOnlyList<string>> GetAllGenresAsync(CancellationToken ct = default) =>
            await _db.Genres.AsNoTracking()
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync(ct);

        #endregion

        #region CRUD

        public async Task<BookReadDto> CreateAsync(BookCreateDto dto, CancellationToken ct = default)
        {
            var book = new Book
            {
                Title = dto.Title,
                Isbn = dto.Isbn,
                Description = dto.Description,
                PublicationDate = dto.PublicationDate,
                AuthorId = dto.AuthorId,
                GenreId = dto.GenreId,
                EditorId = dto.EditorId,
                ShelfLevelId = dto.ShelfLevelId,
                CoverUrl = dto.CoverUrl
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync(ct);

            // Stock initial si absent → 0
            var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == book.BookId, ct);
            if (stock == null)
            {
                _db.Stocks.Add(new Stock { BookId = book.BookId, Quantity = 0 });
                await _db.SaveChangesAsync(ct);
            }

            // Tags
            if (dto.TagIds is { Count: > 0 })
            {
                foreach (var tagId in dto.TagIds.Distinct())
                    _db.BookTags.Add(new BookTag { BookId = book.BookId, TagId = tagId });

                await _db.SaveChangesAsync(ct);
            }

            // Retourne le DTO projeté
            return await _db.Books.AsNoTracking()
                .Where(b => b.BookId == book.BookId)
                .Select(ReadProjection)
                .SingleAsync(ct);
        }

        public async Task<bool> UpdateAsync(int id, BookUpdateDto dto, CancellationToken ct = default)
        {
            var b = await _db.Books.FirstOrDefaultAsync(x => x.BookId == id, ct);
            if (b == null) return false;

            b.Title = dto.Title;
            b.Isbn = dto.Isbn;
            b.Description = dto.Description;
            b.PublicationDate = dto.PublicationDate;
            b.AuthorId = dto.AuthorId;
            b.GenreId = dto.GenreId;
            b.EditorId = dto.EditorId;
            b.ShelfLevelId = dto.ShelfLevelId;
            b.CoverUrl = dto.CoverUrl;

            // Tags (remplace)
            if (dto.TagIds != null)
            {
                var current = _db.BookTags.Where(bt => bt.BookId == id);
                _db.BookTags.RemoveRange(current);

                foreach (var tagId in dto.TagIds.Distinct())
                    _db.BookTags.Add(new BookTag { BookId = id, TagId = tagId });
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var b = await _db.Books.FirstOrDefaultAsync(x => x.BookId == id, ct);
            if (b == null) return false;

            // dépendances simples
            var tags = _db.BookTags.Where(bt => bt.BookId == id);
            _db.BookTags.RemoveRange(tags);

            var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == id, ct);
            if (stock != null) _db.Stocks.Remove(stock);

            _db.Books.Remove(b);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        #endregion
    }
}
