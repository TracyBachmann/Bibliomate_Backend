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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// An <see cref="IEnumerable{GenreReadDto}"/> containing all genres.
        /// </returns>
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
        /// <param name="id">Identifier of the genre to retrieve.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item>The <see cref="GenreReadDto"/> if found, otherwise null.</item>
        ///   <item>An <see cref="IActionResult"/> <c>NotFound</c> when missing, otherwise null.</item>
        /// </list>
        /// </returns>
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
        /// <param name="dto">Data transfer object containing genre creation data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A tuple containing the created <see cref="GenreReadDto"/> and a <see cref="CreatedAtActionResult"/>.
        /// </returns>
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
        /// <param name="id">Identifier of the genre to update.</param>
        /// <param name="dto">Data transfer object containing updated genre data.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the update was successful; <c>false</c> if the genre was not found.
        /// </returns>
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
        /// <param name="id">Identifier of the genre to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// <c>true</c> if the deletion was successful; <c>false</c> if the genre was not found.
        /// </returns>
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
    }
}