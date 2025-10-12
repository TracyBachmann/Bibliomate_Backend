using Microsoft.EntityFrameworkCore;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;

namespace BackendBiblioMate.Services.Users
{
    /// <summary>
    /// Default implementation of <see cref="IUserService"/> that provides
    /// CRUD operations and role management for <see cref="User"/> entities,
    /// using EF Core as the persistence layer.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly BiblioMateDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
        public UserService(BiblioMateDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves all users with their associated genres.
        /// </summary>
        public async Task<IEnumerable<UserReadDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserGenres)
                .ToListAsync(cancellationToken);

            return users.Select(MapToDto);
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves a user by their identifier, including favorite genres.
        /// </summary>
        public async Task<UserReadDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .Include(u => u.UserGenres)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);

            return user is null ? null : MapToDto(user);
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new user with basic details and hashed password.
        /// </summary>
        public async Task<UserReadDto> CreateAsync(
            UserCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = new User
            {
                FirstName        = dto.FirstName,
                LastName         = dto.LastName,
                Email            = dto.Email,
                Password         = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address1         = dto.Address1 ?? string.Empty,
                Address2         = dto.Address2,
                Phone            = dto.Phone ?? string.Empty,
                Role             = dto.Role,
                IsEmailConfirmed = true,
                IsApproved       = true,
                DateOfBirth      = dto.DateOfBirth
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(user);
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates an existing user's profile information.
        /// </summary>
        public async Task<bool> UpdateAsync(
            int id,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
                return false;

            user.FirstName   = dto.FirstName;
            user.LastName    = dto.LastName;
            user.Email       = dto.Email;
            user.Address1    = dto.Address1 ?? string.Empty;
            user.Address2    = dto.Address2 ?? string.Empty;
            user.Phone       = dto.Phone ?? string.Empty;
            user.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Updates the role of a user if the new role is valid.
        /// </summary>
        public async Task<bool> UpdateRoleAsync(
            int id,
            UserRoleUpdateDto dto,
            CancellationToken cancellationToken = default)
        {
            var validRoles = new[] { UserRoles.User, UserRoles.Librarian, UserRoles.Admin };
            if (!validRoles.Contains(dto.Role))
                return false;

            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
                return false;

            user.Role = dto.Role;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes a user by their identifier.
        /// </summary>
        public async Task<bool> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves the currently logged-in user's details.
        /// </summary>
        public Task<UserReadDto?> GetCurrentUserAsync(
            int currentUserId,
            CancellationToken cancellationToken = default)
            => GetByIdAsync(currentUserId, cancellationToken);

        /// <inheritdoc />
        /// <summary>
        /// Updates the currently logged-in user's profile information.
        /// </summary>
        public Task<bool> UpdateCurrentUserAsync(
            int currentUserId,
            UserUpdateDto dto,
            CancellationToken cancellationToken = default)
            => UpdateAsync(currentUserId, dto, cancellationToken);

        /// <summary>
        /// Maps a <see cref="User"/> entity to a <see cref="UserReadDto"/>.
        /// </summary>
        private static UserReadDto MapToDto(User u) => new()
        {
            UserId           = u.UserId,
            FirstName        = u.FirstName,
            LastName         = u.LastName,
            Email            = u.Email,
            Role             = u.Role,
            Address1         = u.Address1,
            Address2         = u.Address2,
            Phone            = u.Phone,
            DateOfBirth      = u.DateOfBirth,
            ProfileImagePath = u.ProfileImagePath,
            FavoriteGenreIds = u.UserGenres.Select(g => g.GenreId).ToArray(),
            IsEmailConfirmed = u.IsEmailConfirmed,
            IsApproved       = u.IsApproved,
        };
    }
}
