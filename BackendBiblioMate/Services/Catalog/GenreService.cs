using System.Linq.Expressions;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides CRUD operations for <see cref="Genre"/> entities using EF Core.
    /// </summary>
    public class GenreService : IGenreService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenreService"/> class.
        /// </summary>
        /// <param name="db">The EF Core database context.</param>
        public GenreService(BiblioMateDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves all genres from the data store.
        /// </summary>
        public async Task<IEnumerable<GenreReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _db.Genres
                .AsNoTracking()
                .Select(MapToReadDto)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single genre by its identifier.
        /// </summary>
        public async Task<(GenreReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Genres.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return (null, new NotFoundResult());

            return (new GenreReadDto
            {
                GenreId = entity.GenreId,
                Name    = entity.Name
            }, null);
        }

        /// <summary>
        /// Creates a new genre in the data store.
        /// </summary>
        public async Task<(GenreReadDto CreatedDto, CreatedAtActionResult Result)> CreateAsync(
            GenreCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = new Genre { Name = dto.Name };
            _db.Genres.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            var createdDto = new GenreReadDto
            {
                GenreId = entity.GenreId,
                Name    = entity.Name
            };

            var result = new CreatedAtActionResult(
                actionName: nameof(GenresController.GetGenre),
                controllerName: "Genres",
                routeValues: new { id = createdDto.GenreId },
                value: createdDto);

            return (createdDto, result);
        }

        /// <summary>
        /// Updates an existing genre in the data store.
        /// </summary>
        public async Task<bool> UpdateAsync(
            int id,
            GenreUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Genres
                .FirstOrDefaultAsync(g => g.GenreId == id, cancellationToken);

            if (entity == null)
                return false;

            entity.Name = dto.Name;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a genre from the data store.
        /// </summary>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _db.Genres.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                return false;

            _db.Genres.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Expression to project <see cref="Genre"/> into <see cref="GenreReadDto"/>.
        /// </summary>
        private static readonly Expression<Func<Genre, GenreReadDto>> MapToReadDto = g => new GenreReadDto
        {
            GenreId = g.GenreId,
            Name    = g.Name
        };

        // ===== AJOUTS: Search + Ensure ======================================

        public async Task<IEnumerable<GenreReadDto>> SearchAsync(string? search, int take, CancellationToken ct)
        {
            var q = _db.Genres.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(g => g.Name.ToLower().Contains(s));
            }

            take = Math.Clamp(take, 1, 100);

            return await q.OrderBy(g => g.Name)
                          .Take(take)
                          .Select(MapToReadDto)
                          .ToListAsync(ct);
        }

        public async Task<(GenreReadDto Dto, bool Created)> EnsureAsync(string name, CancellationToken ct)
        {
            var normalized = (name ?? "").Trim();
            if (normalized.Length < 2) throw new ArgumentException("Name too short", nameof(name));

            var existing = await _db.Genres
                .AsNoTracking()
                .Where(g => g.Name.ToLower() == normalized.ToLower())
                .Select(g => new GenreReadDto { GenreId = g.GenreId, Name = g.Name })
                .FirstOrDefaultAsync(ct);

            if (existing != null) return (existing, false);

            var entity = new Genre { Name = normalized };
            _db.Genres.Add(entity);
            await _db.SaveChangesAsync(ct);

            return (new GenreReadDto { GenreId = entity.GenreId, Name = entity.Name }, true);
        }
    }
}
