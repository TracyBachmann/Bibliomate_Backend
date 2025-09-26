using System.Security.Claims;
using System.Text.Json;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="UsersController"/>.
    /// Covers admin-only endpoints, “/me” routes, role updates, deletion flows, and activity logging.
    /// </summary>
    public class UsersControllerTest
    {
        private readonly Mock<IUserService>            _serviceMock;
        private readonly Mock<IUserActivityLogService> _logMock;
        private readonly UsersController               _controller;

        public UsersControllerTest()
        {
            _serviceMock = new Mock<IUserService>();
            _logMock     = new Mock<IUserActivityLogService>();
            _controller  = new UsersController(_serviceMock.Object, _logMock.Object);
        }

        /// <summary>
        /// Helper to set <see cref="HttpContext.User"/> with given user ID and roles.
        /// </summary>
        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
                }
            };
        }

        // -------- GET /api/users (Admin) --------

        /// <summary>
        /// An admin retrieving all users should return 200 OK with the list.
        /// </summary>
        [Fact]
        public async Task GetUsers_AsAdmin_ReturnsOk()
        {
            SetUser(1, UserRoles.Admin);
            var list = new List<UserReadDto>
            {
                new UserReadDto { UserId = 1, Email = "a@x" },
                new UserReadDto { UserId = 2, Email = "b@y" }
            };
            _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var action = await _controller.GetUsers();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value!);
        }

        // -------- GET /api/users/{id} (Admin) --------

        /// <summary>
        /// An admin retrieving an existing user by ID should return 200 OK.
        /// </summary>
        [Fact]
        public async Task GetUser_Exists_ReturnsOk()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserReadDto { UserId = 5, Email = "u@z" };
            _serviceMock.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            var action = await _controller.GetUser(5);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value!);
        }

        /// <summary>
        /// An admin retrieving a missing user should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((UserReadDto?)null);

            var action = await _controller.GetUser(99);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        // -------- POST /api/users (Admin) --------

        /// <summary>
        /// An admin creating a new user should return 201 Created and log the action.
        /// </summary>
        [Fact]
        public async Task PostUser_AsAdmin_ReturnsCreatedAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            var createDto = new UserCreateDto { Email = "new@x" };
            var created   = new UserReadDto   { UserId = 10, Email = "new@x" };

            _serviceMock.Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(created);

            var action = await _controller.PostUser(createDto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(UsersController.GetUser), createdAt.ActionName);
            Assert.Equal(created.UserId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value!);

            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == created.UserId &&
                        doc.Action == "CreateAccount" &&
                        doc.Details!.Contains($"Email={created.Email}")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // -------- PUT /api/users/{id} (Admin) --------

        /// <summary>
        /// An admin updating a user should return 204 NoContent and log the action.
        /// </summary>
        [Fact]
        public async Task UpdateUser_AsAdmin_ReturnsNoContentAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserUpdateDto { Email = "upd@x" };
            _serviceMock.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var action = await _controller.UpdateUser(5, dto);

            Assert.IsType<NoContentResult>(action);
            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 5 &&
                        doc.Action == "UpdateUser" &&
                        doc.Details!.Contains("Updated basic info")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Updating a non-existent user should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserUpdateDto();
            _serviceMock.Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var action = await _controller.UpdateUser(5, dto);

            Assert.IsType<NotFoundResult>(action);
        }

        // -------- PUT /api/users/me (Auth) --------

        /// <summary>
        /// A user updating their own profile should return 200 OK and log the action.
        /// </summary>
        [Fact]
        public async Task UpdateCurrentUser_Self_ReturnsOkAndLogs()
        {
            SetUser(7);
            var dto = new UserUpdateDto { Email = "me@x" };
            _serviceMock.Setup(s => s.UpdateCurrentUserAsync(7, dto, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            var action = await _controller.UpdateCurrentUser(dto);

            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal("Profile updated successfully.", ok.Value!);
            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 7 &&
                        doc.Action == "UpdateSelf"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Updating current user when not found should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateCurrentUser_NotFound_Returns404()
        {
            SetUser(8);
            var dto = new UserUpdateDto();
            _serviceMock.Setup(s => s.UpdateCurrentUserAsync(8, dto, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            var action = await _controller.UpdateCurrentUser(dto);

            Assert.IsType<NotFoundResult>(action);
        }

        // -------- GET /api/users/me (Auth) --------

        /// <summary>
        /// A user retrieving their own profile should return 200 OK with user details.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_ReturnsOk()
        {
            SetUser(9);
            var dto = new UserReadDto { UserId = 9, Email = "self@x" };
            _serviceMock.Setup(s => s.GetCurrentUserAsync(9, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(dto);

            var action = await _controller.GetCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value!);
        }

        /// <summary>
        /// Retrieving current user when missing should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_NotFound_Returns404()
        {
            SetUser(10);
            _serviceMock.Setup(s => s.GetCurrentUserAsync(10, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((UserReadDto?)null);

            var action = await _controller.GetCurrentUser();

            Assert.IsType<NotFoundResult>(action.Result);
        }

        // -------- PUT /api/users/{id}/role (Admin) --------

        /// <summary>
        /// Updating a user’s role successfully should return 200 OK and log the action.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_Success_ReturnsOkAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new UserReadDto { UserId = 5 });

            var dto = new UserRoleUpdateDto { Role = UserRoles.Librarian };
            _serviceMock.Setup(s => s.UpdateRoleAsync(5, dto, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            var action = await _controller.UpdateUserRole(5, dto);

            var ok = Assert.IsType<OkObjectResult>(action);

            // Validate anonymous payload message
            var json = JsonSerializer.Serialize(ok.Value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var doc = JsonDocument.Parse(json);
            Assert.Equal($"Role updated to {dto.Role}.", doc.RootElement.GetProperty("message").GetString());

            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(d =>
                        d.UserId == 5 &&
                        d.Action == "UpdateRole" &&
                        d.Details!.Contains(dto.Role)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Updating role of a missing user should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.GetByIdAsync(6, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((UserReadDto?)null);

            var dto = new UserRoleUpdateDto { Role = UserRoles.User };
            var action = await _controller.UpdateUserRole(6, dto);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Updating role with an invalid value should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_Invalid_Returns400()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new UserReadDto { UserId = 7 });

            var dto = new UserRoleUpdateDto { Role = "INVALID" };
            _serviceMock.Setup(s => s.UpdateRoleAsync(7, dto, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            var action = await _controller.UpdateUserRole(7, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Invalid role.", bad.Value!);
        }

        // -------- DELETE /api/users/{id} (Admin) --------

        /// <summary>
        /// An admin attempting to delete their own account should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task DeleteUser_Self_ReturnsBadRequest()
        {
            SetUser(2, UserRoles.Admin);

            var action = await _controller.DeleteUser(2);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("You cannot delete your own account.", bad.Value!);
        }

        /// <summary>
        /// An admin deleting another user successfully should return 204 NoContent and log the action.
        /// </summary>
        [Fact]
        public async Task DeleteUser_Other_Success_ReturnsNoContentAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var action = await _controller.DeleteUser(3);

            Assert.IsType<NoContentResult>(action);

            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(d =>
                        d.UserId == 1 &&
                        d.Action == "DeleteUser" &&
                        d.Details!.Contains("Deleted user 3")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Deleting a missing user should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock.Setup(s => s.DeleteAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var action = await _controller.DeleteUser(4);

            Assert.IsType<NotFoundResult>(action);
        }

        // -------- DELETE /api/users/me (Auth) --------

        /// <summary>
        /// A user deleting their own account should return 204 NoContent and log the action.
        /// </summary>
        [Fact]
        public async Task DeleteCurrentUser_Success_ReturnsNoContentAndLogs()
        {
            SetUser(12, UserRoles.User);
            _serviceMock.Setup(s => s.DeleteAsync(12, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);

            var action = await _controller.DeleteCurrentUser();

            Assert.IsType<NoContentResult>(action);

            _logMock.Verify(l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(d =>
                        d.UserId == 12 &&
                        d.Action == "DeleteSelf"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Deleting current user when not found should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteCurrentUser_NotFound_Returns404()
        {
            SetUser(13, UserRoles.User);
            _serviceMock.Setup(s => s.DeleteAsync(13, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false);

            var action = await _controller.DeleteCurrentUser();

            Assert.IsType<NotFoundResult>(action);
        }

        // -------- GET /api/users/debug-token (Auth) --------

        /// <summary>
        /// DebugToken should return 200 OK with the full list of claims for the current user.
        /// </summary>
        [Fact]
        public void DebugToken_ReturnsOkWithClaims()
        {
            SetUser(42, UserRoles.User);

            // Add an extra claim to ensure it flows through
            var http = (DefaultHttpContext)_controller.ControllerContext.HttpContext!;
            var identity = (ClaimsIdentity)http.User.Identity!;
            identity.AddClaim(new Claim("custom", "value"));

            var action = _controller.DebugToken();

            var ok = Assert.IsType<OkObjectResult>(action);

            // Serialize anonymous objects to check content
            var json = JsonSerializer.Serialize(ok.Value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.EnumerateArray().ToList();

            Assert.Contains(items, el => el.GetProperty("type").GetString() == ClaimTypes.NameIdentifier
                                         && el.GetProperty("value").GetString() == "42");
            Assert.Contains(items, el => el.GetProperty("type").GetString() == ClaimTypes.Role
                                         && el.GetProperty("value").GetString() == UserRoles.User);
            Assert.Contains(items, el => el.GetProperty("type").GetString() == "custom"
                                         && el.GetProperty("value").GetString() == "value");
        }
    }
}
