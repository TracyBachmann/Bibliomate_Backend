using System.Security.Claims;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    /// <summary>
    /// Controller for managing application users.
    /// All administration endpoints are restricted to the <c>Admin</c> role,
    /// while authenticated users may access and update their own profile via <c>/me</c> routes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly BiblioMateDbContext _context;
        private readonly UserActivityLogService _activityLog;

        /// <summary>
        /// Constructor with DbContext and ActivityLog service injection.
        /// </summary>
        public UsersController(BiblioMateDbContext context,
                               UserActivityLogService activityLog)
        {
            _context     = context;
            _activityLog = activityLog;
        }

        // GET: api/Users
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <remarks>Admin‐only endpoint.</remarks>
        /// <returns>A collection of <see cref="UserReadDto"/>.</returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserReadDto
                {
                    UserId = u.UserId,
                    Name   = u.Name,
                    Email  = u.Email,
                    Role   = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/{id}
        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>
        /// The requested <see cref="UserReadDto"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserReadDto
            {
                UserId = user.UserId,
                Name   = user.Name,
                Email  = user.Email,
                Role   = user.Role
            };
        }

        // POST: api/Users
        /// <summary>
        /// Creates a new user (via the admin panel).
        /// The supplied plain-text password is hashed automatically.
        /// </summary>
        /// <param name="dto">User object containing registration data.</param>
        /// <returns>
        /// <c>201 Created</c> with the created user’s data.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto dto)
        {
            var user = new User
            {
                Name             = dto.Name,
                Email            = dto.Email,
                Password         = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Address          = dto.Address ?? string.Empty,
                Phone            = dto.Phone ?? string.Empty,
                Role             = UserRoles.User,
                IsEmailConfirmed = true,
                IsApproved       = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log account creation
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = user.UserId,
                Action  = "CreateAccount",
                Details = $"Email={user.Email}"
            });

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new UserReadDto
            {
                UserId = user.UserId,
                Name   = user.Name,
                Email  = user.Email,
                Role   = user.Role
            });
        }

        // PUT: api/Users/{id}
        /// <summary>
        /// Updates an existing user’s basic information.
        /// Does <b>not</b> allow changing password or role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">DTO containing updated data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the user does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Name    = dto.Name;
            user.Email   = dto.Email;
            user.Address = dto.Address;
            user.Phone   = dto.Phone;

            await _context.SaveChangesAsync();

            // Log profile update
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = user.UserId,
                Action  = "UpdateUser",
                Details = $"Updated basic info for user {user.UserId}"
            });

            return NoContent();
        }

        // PUT: api/Users/me
        /// <summary>
        /// Updates the currently authenticated user’s personal data.
        /// </summary>
        /// <param name="dto">DTO containing updated profile information.</param>
        /// <returns><c>200 OK</c> with a success message.</returns>
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto dto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name    = dto.Name;
            user.Email   = dto.Email;
            user.Phone   = dto.Phone;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();

            // Log self profile update
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = user.UserId,
                Action  = "UpdateSelf",
                Details = "User updated own profile"
            });

            return Ok("Profile updated successfully.");
        }

        // GET: api/Users/me
        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <returns>The user’s profile data.</returns>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) return Unauthorized();

            int userId = int.Parse(claim.Value);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.Name,
                user.Email,
                user.Address,
                user.Phone,
                user.Role
            });
        }

        // PUT: api/Users/{id}/role
        /// <summary>
        /// Updates a user’s role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">DTO containing the new role.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>400 BadRequest</c> for invalid roles;  
        /// <c>404 NotFound</c> if the user does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRoleUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var validRoles = new[] { UserRoles.User, UserRoles.Librarian, UserRoles.Admin };
            if (!validRoles.Contains(dto.Role))
                return BadRequest("Invalid role.");

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            // Log role change
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = user.UserId,
                Action  = "UpdateRole",
                Details = $"Role changed to {dto.Role}"
            });

            return Ok(new { message = $"Role updated to {dto.Role}." });
        }

        // DELETE: api/Users/{id}
        /// <summary>
        /// Deletes a user account.
        /// Cannot delete your own account.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>
        /// <c>204 NoContent</c> when deletion succeeds;  
        /// <c>400 BadRequest</c> if attempting self-deletion;  
        /// <c>404 NotFound</c> if the user is not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == currentUserId)
                return BadRequest("You cannot delete your own account.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Log deletion
            await _activityLog.LogAsync(new UserActivityLogDocument
            {
                UserId  = currentUserId,
                Action  = "DeleteUser",
                Details = $"Deleted user {id}"
            });

            return NoContent();
        }
    }
}