using System.Security.Claims;
using backend.DTOs;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IUserService _svc;
        private readonly UserActivityLogService _log;

        public UsersController(
            IUserService svc,
            UserActivityLogService log)
        {
            _svc = svc;
            _log = log;
        }

        // GET: api/Users
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <remarks>Admin‐only endpoint.</remarks>
        /// <returns>
        /// <c>200 OK</c> with a collection of <see cref="UserReadDto"/>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
            => Ok(await _svc.GetAllAsync());

        // GET: api/Users/{id}
        /// <summary>
        /// Retrieves a user by its identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>
        /// <c>200 OK</c> with <see cref="UserReadDto"/> if found;
        /// <c>404 NotFound</c> otherwise.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            var dto = await _svc.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // POST: api/Users
        /// <summary>
        /// Creates a new user (via the admin panel).
        /// </summary>
        /// <param name="dto">User registration data.</param>
        /// <returns>
        /// <c>201 Created</c> with the created <see cref="UserReadDto"/>.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = created.UserId,
                Action  = "CreateAccount",
                Details = $"Email={created.Email}"
            });
            return CreatedAtAction(nameof(GetUser), new { id = created.UserId }, created);
        }

        // PUT: api/Users/{id}
        /// <summary>
        /// Updates basic info of an existing user.
        /// Does <b>not</b> allow changing password or role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">Updated data.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto dto)
        {
            var ok = await _svc.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = id,
                Action  = "UpdateUser",
                Details = $"Updated basic info for user {id}"
            });
            return NoContent();
        }

        // PUT: api/Users/me
        /// <summary>
        /// Updates the currently authenticated user’s personal data.
        /// </summary>
        /// <param name="dto">The updated profile data.</param>
        /// <returns>
        /// <c>200 OK</c> on success;  
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateDto dto)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var ok = await _svc.UpdateCurrentUserAsync(me, dto);
            if (!ok) return NotFound();
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = me,
                Action  = "UpdateSelf",
                Details = "User updated own profile"
            });
            return Ok("Profile updated successfully.");
        }

        // GET: api/Users/me
        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with the user’s profile data;
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserReadDto>> GetCurrentUser()
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var dto = await _svc.GetCurrentUserAsync(me);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // PUT: api/Users/{id}/role
        /// <summary>
        /// Updates a user’s role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="dto">New role data.</param>
        /// <returns>
        /// <c>200 OK</c> on success;
        /// <c>400 BadRequest</c> for invalid role;
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, UserRoleUpdateDto dto)
        {
            var ok = await _svc.UpdateRoleAsync(id, dto);
            if (!ok) return id == 0 ? BadRequest("Invalid role.") : NotFound();
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = id,
                Action  = "UpdateRole",
                Details = $"Role changed to {dto.Role}"
            });
            return Ok(new { message = $"Role updated to {dto.Role}." });
        }

        // DELETE: api/Users/{id}
        /// <summary>
        /// Deletes a user account (admin only).
        /// Cannot delete your own account.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>
        /// <c>204 NoContent</c> on success;
        /// <c>400 BadRequest</c> if self-deletion attempt;
        /// <c>404 NotFound</c> if user not found.
        /// </returns>
        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            int me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == me) return BadRequest("You cannot delete your own account.");
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            await _log.LogAsync(new Models.Mongo.UserActivityLogDocument
            {
                UserId  = me,
                Action  = "DeleteUser",
                Details = $"Deleted user {id}"
            });
            return NoContent();
        }
    }
}