using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    /// <summary>
    /// Implements <see cref="IUserService"/> using EF Core.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly BiblioMateDbContext _context;

        public UserService(BiblioMateDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserReadDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => new UserReadDto
                {
                    UserId = u.UserId,
                    Name   = u.Name,
                    Email  = u.Email,
                    Role   = u.Role
                })
                .ToListAsync();
        }

        public async Task<UserReadDto?> GetByIdAsync(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return null;
            return new UserReadDto
            {
                UserId = u.UserId,
                Name   = u.Name,
                Email  = u.Email,
                Role   = u.Role
            };
        }

        public async Task<UserReadDto> CreateAsync(UserCreateDto dto)
        {
            var u = new User
            {
                Name             = dto.Name,
                Email            = dto.Email,
                Password         = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address          = dto.Address ?? string.Empty,
                Phone            = dto.Phone   ?? string.Empty,
                Role             = dto.Role,
                IsEmailConfirmed = true,
                IsApproved       = true
            };
            _context.Users.Add(u);
            await _context.SaveChangesAsync();
            return new UserReadDto
            {
                UserId = u.UserId,
                Name   = u.Name,
                Email  = u.Email,
                Role   = u.Role
            };
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return false;
            u.Name    = dto.Name;
            u.Email   = dto.Email;
            u.Address = dto.Address;
            u.Phone   = dto.Phone;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateRoleAsync(int id, UserRoleUpdateDto dto)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return false;
            var valid = new[] { UserRoles.User, UserRoles.Librarian, UserRoles.Admin };
            if (!valid.Contains(dto.Role)) return false;
            u.Role = dto.Role;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return false;
            _context.Users.Remove(u);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserReadDto?> GetCurrentUserAsync(int currentUserId)
            => await GetByIdAsync(currentUserId);

        public async Task<bool> UpdateCurrentUserAsync(int currentUserId, UserUpdateDto dto)
        {
            return await UpdateAsync(currentUserId, dto);
        }
    }
}