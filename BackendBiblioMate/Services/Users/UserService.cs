using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Implements <see cref="IUserService"/> using EF Core for CRUD operations on users.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/>.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public UserService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>Collection of <see cref="UserReadDto"/>.</returns>
        public async Task<IEnumerable<UserReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return users.Select(MapToDto);
        }

        /// <summary>
        /// Retrieves a user by their identifier.
        /// </summary>
        /// <param name="id">Identifier of the user.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// The <see cref="UserReadDto"/> if found; otherwise <c>null</c>.
        /// </returns>
        public async Task<UserReadDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // FindAsync est disponible **directement** sur DbSet<T>
            var user = await _context.Users
                .FindAsync(new object[]{ id }, cancellationToken)
                .AsTask();            // FindAsync retourne une ValueTask<EntityEntry>, on convertit en Task<T>
        
            if (user == null)
                return null;

            return MapToDto(user);
        }

        /// <summary>
        /// Creates a new user with the specified data.
        /// </summary>
        /// <param name="dto">Data for the new user.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>The created <see cref="UserReadDto"/>.</returns>
        public async Task<UserReadDto> CreateAsync(
            UserCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = new User
            {
                Name             = dto.Name,
                Email            = dto.Email,
                Password         = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address          = dto.Address ?? string.Empty,
                Phone            = dto.Phone ?? string.Empty,
                Role             = dto.Role,
                IsEmailConfirmed = true,
                IsApproved       = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(user);
        }

        /// <summary>
        /// Updates an existing user's basic profile information.
        /// </summary>
        /// <param name="id">Identifier of the user to update.</param>
        /// <param name="dto">Updated user data.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns><c>true</c> if update succeeded; <c>false</c> if user not found.</returns>
        public async Task<bool> UpdateAsync(
            int id,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FindAsync(new object[] { id }, cancellationToken);
            if (user is null) 
                return false;

            user.Name    = dto.Name;
            user.Email   = dto.Email;
            user.Address = dto.Address;
            user.Phone   = dto.Phone;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Updates the role of an existing user.
        /// </summary>
        /// <param name="id">Identifier of the user.</param>
        /// <param name="dto">Role update data.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns>
        /// <c>true</c> if role was valid and update succeeded; otherwise <c>false</c>.
        /// </returns>
        public async Task<bool> UpdateRoleAsync(
            int id,
            UserRoleUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var validRoles = new[] { UserRoles.User, UserRoles.Librarian, UserRoles.Admin };
            if (!validRoles.Contains(dto.Role))
                return false;

            var user = await _context.Users
                .FindAsync(new object[] { id }, cancellationToken);
            if (user is null)
                return false;

            user.Role = dto.Role;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Deletes a user by their identifier.
        /// </summary>
        /// <param name="id">Identifier of the user to delete.</param>
        /// <param name="cancellationToken">Token to monitor cancellation.</param>
        /// <returns><c>true</c> if deletion succeeded; <c>false</c> if user not found.</returns>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FindAsync(new object[] { id }, cancellationToken);
            if (user is null) 
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Retrieves the currently authenticated user.
        /// </summary>
        public Task<UserReadDto?> GetCurrentUserAsync(
            int currentUserId,
            CancellationToken cancellationToken = default)
            => GetByIdAsync(currentUserId, cancellationToken);

        /// <summary>
        /// Updates the currently authenticated user's profile.
        /// </summary>
        public Task<bool> UpdateCurrentUserAsync(
            int currentUserId,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default)
            => UpdateAsync(currentUserId, dto, cancellationToken);

        /// <summary>
        /// Maps a <see cref="User"/> entity to its <see cref="UserReadDto"/>.
        /// </summary>
        private static UserReadDto MapToDto(User u) => new()
        {
            UserId = u.UserId,
            Name   = u.Name,
            Email  = u.Email,
            Role   = u.Role
        };
    }
}