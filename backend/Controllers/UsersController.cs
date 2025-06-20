using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using backend.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using backend.Models.Enums;

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

        public UsersController(BiblioMateDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <remarks>Admin‐only endpoint.</remarks>
        /// <returns>A collection of <see cref="UserReadDTO"/>.</returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserReadDTO
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
        /// The requested <see cref="UserReadDTO"/> if found; otherwise <c>404 NotFound</c>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserReadDTO
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
        /// <param name="user">User object containing registration data.</param>
        /// <returns>
        /// <c>201 Created</c> with the created user’s data.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<UserReadDTO>> PostUser(User user)
        {
            user.Password         = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role             = UserRoles.User;
            user.IsEmailConfirmed = true;
            user.IsApproved       = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new UserReadDTO
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
        /// <param name="user">User object containing updated data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;  
        /// <c>400 BadRequest</c> if IDs mismatch;  
        /// <c>404 NotFound</c> if the user does not exist.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId)
                return BadRequest();

            var existing = await _context.Users.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name    = user.Name;
            existing.Email   = user.Email;
            existing.Address = user.Address;
            existing.Phone   = user.Phone;

            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDTO dto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name    = dto.Name;
            user.Email   = dto.Email;
            user.Phone   = dto.Phone;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var validRoles = new[] { UserRoles.User, UserRoles.Librarian, UserRoles.Admin };
            if (!validRoles.Contains(dto.Role))
                return BadRequest("Invalid role.");

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

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

            return NoContent();
        }
    }
}
