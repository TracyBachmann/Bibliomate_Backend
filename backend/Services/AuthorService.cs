using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IAuthorService"/> to perform CRUD operations on authors.
    /// </summary>
    public class AuthorService : IAuthorService
    {
        private readonly BiblioMateDbContext _db;

        /// <summary>
        /// Creates a new instance of <see cref="AuthorService"/>.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        public AuthorService(BiblioMateDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AuthorReadDto>> GetAllAsync()
        {
            return await _db.Authors
                .Select(a => new AuthorReadDto
                {
                    AuthorId = a.AuthorId,
                    Name     = a.Name
                })
                .ToListAsync();
        }

        public async Task<(AuthorReadDto? Dto, IActionResult? ErrorResult)> GetByIdAsync(int id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null)
                return (null, new NotFoundResult());

            var dto = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name     = author.Name
            };
            return (dto, null);
        }

        public async Task<(AuthorReadDto Dto, CreatedAtActionResult Result)> CreateAsync(AuthorCreateDto dto)
        {
            var author = new Author { Name = dto.Name };
            _db.Authors.Add(author);
            await _db.SaveChangesAsync();

            var read = new AuthorReadDto
            {
                AuthorId = author.AuthorId,
                Name     = author.Name
            };

            var result = new CreatedAtActionResult(
                actionName: nameof(Controllers.AuthorsController.GetAuthor),
                controllerName: "Authors",
                routeValues: new { id = author.AuthorId },
                value: read
            );

            return (read, result);
        }

        public async Task<bool> UpdateAsync(int id, AuthorCreateDto dto)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null)
                return false;

            author.Name = dto.Name;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var author = await _db.Authors.FindAsync(id);
            if (author == null)
                return false;

            _db.Authors.Remove(author);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
