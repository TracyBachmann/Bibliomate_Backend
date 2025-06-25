using backend.Data;
using backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="ITagService"/> using EF Core.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly BiblioMateDbContext _context;

        public TagService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TagReadDto>> GetAllAsync()
        {
            var tags = await _context.Tags.ToListAsync();
            return tags.Select(t => new TagReadDto
            {
                TagId = t.TagId,
                Name  = t.Name
            });
        }

        public async Task<TagReadDto?> GetByIdAsync(int id)
        {
            var t = await _context.Tags.FindAsync(id);
            if (t == null) return null;

            return new TagReadDto
            {
                TagId = t.TagId,
                Name  = t.Name
            };
        }

        public async Task<TagReadDto> CreateAsync(TagCreateDto dto)
        {
            var t = new Models.Tag { Name = dto.Name };
            _context.Tags.Add(t);
            await _context.SaveChangesAsync();
            return new TagReadDto
            {
                TagId = t.TagId,
                Name  = t.Name
            };
        }

        public async Task<bool> UpdateAsync(TagUpdateDto dto)
        {
            var t = await _context.Tags.FindAsync(dto.TagId);
            if (t == null) return false;
            t.Name = dto.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var t = await _context.Tags.FindAsync(id);
            if (t == null) return false;
            _context.Tags.Remove(t);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}