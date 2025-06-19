using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;

namespace backend.Controllers
{
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
        /// Retrieves all users. Only accessible by Admins.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Select(user => new UserReadDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        /// <summary>
        /// Retrieves a user by ID. Admin access only.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDTO>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return new UserReadDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        // POST: api/Users
        /// <summary>
        /// Creates a new user (from Admin panel). Password is hashed automatically.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<UserReadDTO>> PostUser(User user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "User";
            user.IsEmailConfirmed = true;
            user.IsApproved = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new UserReadDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        // PUT: api/Users/5
        /// <summary>
        /// Updates an existing user's basic information. Admin only.
        /// Does not allow modifying password or role.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId)
                return BadRequest();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            // Protect sensitive fields
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Address = user.Address;
            existingUser.Phone = user.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Users/me
        /// <summary>
        /// Updates the current authenticated user's personal data.
        /// </summary>
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();
            return Ok("Mise à jour réussie.");
        }

        // GET: api/Users/me
        /// <summary>
        /// Retrieves the currently authenticated user's profile.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
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
        /// Updates the role of a specific user. Only Admins are allowed.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            var validRoles = new[] { "User", "Librarian", "Admin" };
            if (!validRoles.Contains(dto.Role))
                return BadRequest("Rôle invalide.");

            user.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Rôle mis à jour à {dto.Role}."
            });
        }

        // DELETE: api/Users/5
        /// <summary>
        /// Deletes a user by ID. Admin only. Cannot delete yourself.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (id == currentUserId)
                return BadRequest("Vous ne pouvez pas supprimer votre propre compte.");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}