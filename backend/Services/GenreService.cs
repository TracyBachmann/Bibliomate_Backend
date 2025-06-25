using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IGenreService"/> by coordinating EF Core
    /// to perform CRUD on <see cref="Genre"/> entities.
    /// </summary>
    public class GenreService : IGenreService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Initializes a new instance of <see cref="GenreService"/>.
        /// </summary>
        /// <param name="db">The EF Core database context.</param>
        public GenreService(BiblioMateDbContext db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<GenreReadDto>> GetAllAsync()
        {
            return await _db.Genres
                .Select(g => new GenreReadDto
                {
                    GenreId = g.GenreId,
                    Name    = g.Name
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<GenreReadDto?> GetByIdAsync(int id)
        {
            var g = await _db.Genres.FindAsync(id);
            if (g == null) return null;

            return new GenreReadDto
            {
                GenreId = g.GenreId,
                Name    = g.Name
            };
        }

        /// <inheritdoc/>
        public async Task<GenreReadDto> CreateAsync(GenreCreateDto dto)
        {
            var entity = new Genre { Name = dto.Name };
            _db.Genres.Add(entity);
            await _db.SaveChangesAsync();

            return new GenreReadDto
            {
                GenreId = entity.GenreId,
                Name    = entity.Name
            };
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(int id, GenreCreateDto dto)
        {
            var entity = await _db.Genres.FindAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            await _db.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Genres.FindAsync(id);
            if (entity == null) return false;

            _db.Genres.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}