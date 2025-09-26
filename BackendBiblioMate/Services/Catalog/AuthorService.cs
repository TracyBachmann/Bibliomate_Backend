using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Author"/> entities.
    /// </summary>
    public class AuthorService : IAuthorService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorService"/> class.
        /// </summary>
        /// <param name="db">Database context for BiblioMate.</param>
        public AuthorService(BiblioMateDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves all authors from the data store.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{AuthorReadDto}"/> containing all authors.
        /// </returns>
        public async Task<IEnumerable<AuthorReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _db.Authors
                .AsNoTracking()
                .Select(a => new AuthorReadDto
                {
                    AuthorId = a.AuthorId,
                    Name     = a.Name
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single author by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the author to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item>The <see cref="AuthorReadDto"/> if found, otherwise null.</item>
        /// <item>An <see cref="IActionResult"/> <c>NotFound</c> when missing, otherwise null.</item>
        /// </list>
        /// </returns>
        public async Task<(AuthorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var author = await _db.Authors.FindAsync(new object[] { id }, cancellationToken);
            if (author == null)
            {
                return (null, new NotFoundResult());
            }

            return (MapToReadDto(author), null);
        }

        /// <summary>
        /// Creates a new author in the data store.
        /// </summary>
        /// <param name="dto">Data transfer object containing author creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing the created <see cref="AuthorReadDto"/> and a <see cref="CreatedAtActionResult"/>.
        /// </returns>
        public async Task<(AuthorReadDto Dto, CreatedAtActionResult Result)> CreateAsync(
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Author { Name = dto.Name };
            _db.Authors.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            var readDto = MapToReadDto(entity);
            
            var result = new CreatedAtActionResult(
                actionName: nameof(Controllers.AuthorsController.GetAuthor),
                controllerName: "Authors",
                routeValues: new { id = readDto.AuthorId },
                value: readDto);

            return (readDto, result);
        }

        /// <summary>
        /// Updates an existing author in the data store.
        /// </summary>
        /// <param name="id">Identifier of the author to update.</param>
        /// <param name="dto">Data transfer object containing updated author data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the author was not found.
        /// </returns>
        public async Task<bool> UpdateAsync(
            int id,
            AuthorCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var author = await _db.Authors.FindAsync(new object[] { id }, cancellationToken);
            if (author == null)
            {
                return false;
            }

            author.Name = dto.Name;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes an author from the data store.
        /// </summary>
        /// <param name="id">Identifier of the author to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the author was not found.
        /// </returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var author = await _db.Authors.FindAsync(new object[] { id }, cancellationToken);
            if (author == null)
            {
                return false;
            }

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Maps an <see cref="Author"/> entity to an <see cref="AuthorReadDto"/>.
        /// </summary>
        /// <param name="author">The author entity to map.</param>
        /// <returns>A new <see cref="AuthorReadDto"/> instance.</returns>
        private static AuthorReadDto MapToReadDto(Author author) => new()
        {
            AuthorId = author.AuthorId,
            Name     = author.Name
        };
        
        public async Task<IEnumerable<AuthorReadDto>> SearchAsync(string? search, int take, CancellationToken ct)
        {
            var q = _db.Authors.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(a => a.Name.ToLower().Contains(s));
            }
            return await q.OrderBy(a => a.Name)
                .Take(Math.Clamp(take, 1, 100))
                .Select(a => new AuthorReadDto { AuthorId = a.AuthorId, Name = a.Name })
                .ToListAsync(ct);
        }

        public async Task<(AuthorReadDto Dto, bool Created)> EnsureAsync(string name, CancellationToken ct)
        {
            var normalized = (name ?? "").Trim();
            if (normalized.Length < 2) throw new ArgumentException("Name too short", nameof(name));

            var existing = await _db.Authors
                .AsNoTracking()
                .Where(a => a.Name.ToLower() == normalized.ToLower())
                .Select(a => new AuthorReadDto { AuthorId = a.AuthorId, Name = a.Name })
                .FirstOrDefaultAsync(ct);

            if (existing != null) return (existing, false);

            var entity = new Author { Name = normalized };
            _db.Authors.Add(entity);
            await _db.SaveChangesAsync(ct);

            return (new AuthorReadDto { AuthorId = entity.AuthorId, Name = entity.Name }, true);
        }

    }
}