using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="UsersController"/>.
    /// Verifies admin‐only endpoints, “/me” routes, and activity logging.
    /// </summary>
    public class UsersControllerTest
    {
        private readonly Mock<IUserService>           _serviceMock;
        private readonly Mock<UserActivityLogService> _logMock;
        private readonly UsersController              _controller;

        public UsersControllerTest()
        {
            _serviceMock = new Mock<IUserService>();
            _logMock     = new Mock<UserActivityLogService>();
            _controller  = new UsersController(_serviceMock.Object, _logMock.Object);
        }

        /// <summary>
        /// Helper to set HttpContext.User with given ID and roles.
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

        /// <summary>
        /// Admin fetching all users should return 200 OK with list.
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
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetUsers();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value!);
        }

        /// <summary>
        /// Admin fetching an existing user returns 200 OK.
        /// </summary>
        [Fact]
        public async Task GetUser_Exists_ReturnsOk()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserReadDto { UserId = 5, Email = "u@z" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetUser(5);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value!);
        }

        /// <summary>
        /// Admin fetching a missing user returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserReadDto?)null);

            var action = await _controller.GetUser(99);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Admin creating a user logs activity and returns 201 Created.
        /// </summary>
        [Fact]
        public async Task PostUser_AsAdmin_ReturnsCreatedAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            var createDto = new UserCreateDto { Email = "new@x" };
            var created   = new UserReadDto { UserId = 10, Email = "new@x" };

            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var action = await _controller.PostUser(createDto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(UsersController.GetUser), createdAt.ActionName);
            Assert.Equal(created.UserId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value!);

            _logMock.Verify(
                l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == created.UserId &&
                        doc.Action == "CreateAccount" &&
                        doc.Details!.Contains($"Email={created.Email}")
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Admin updating a user returns 204 NoContent and logs.
        /// </summary>
        [Fact]
        public async Task UpdateUser_AsAdmin_ReturnsNoContentAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserUpdateDto { Email = "upd@x" };
            _serviceMock
                .Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateUser(5, dto);

            Assert.IsType<NoContentResult>(action);

            _logMock.Verify(
                l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 5 &&
                        doc.Action == "UpdateUser" &&
                        doc.Details!.Contains("Updated basic info")
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Updating a non-existent user returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new UserUpdateDto();
            _serviceMock
                .Setup(s => s.UpdateAsync(5, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateUser(5, dto);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Authenticated user updating own profile returns 200 OK and logs.
        /// </summary>
        [Fact]
        public async Task UpdateCurrentUser_Self_ReturnsOkAndLogs()
        {
            SetUser(7);
            var dto = new UserUpdateDto { Email = "me@x" };
            _serviceMock
                .Setup(s => s.UpdateCurrentUserAsync(7, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateCurrentUser(dto);

            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.Equal("Profile updated successfully.", ok.Value!);

            _logMock.Verify(
                l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 7 &&
                        doc.Action == "UpdateSelf"
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Authenticated user updating own profile not found returns 404.
        /// </summary>
        [Fact]
        public async Task UpdateCurrentUser_NotFound_Returns404()
        {
            SetUser(8);
            var dto = new UserUpdateDto();
            _serviceMock
                .Setup(s => s.UpdateCurrentUserAsync(8, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateCurrentUser(dto);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Authenticated user retrieving own profile returns 200 OK.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_ReturnsOk()
        {
            SetUser(9);
            var dto = new UserReadDto { UserId = 9, Email = "self@x" };
            _serviceMock
                .Setup(s => s.GetCurrentUserAsync(9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetCurrentUser();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value!);
        }

        /// <summary>
        /// Authenticated user retrieving own profile not found returns 404.
        /// </summary>
        [Fact]
        public async Task GetCurrentUser_NotFound_Returns404()
        {
            SetUser(10);
            _serviceMock
                .Setup(s => s.GetCurrentUserAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserReadDto?)null);

            var action = await _controller.GetCurrentUser();

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Admin updating role for existing user returns 200 OK and logs.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_Success_ReturnsOkAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserReadDto { UserId = 5 });
            var dto = new UserRoleUpdateDto { Role = UserRoles.Librarian };
            _serviceMock
                .Setup(s => s.UpdateRoleAsync(5, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateUserRole(5, dto);

            var ok = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal($"Role updated to {dto.Role}.", (string)body.message);

            _logMock.Verify(
                l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 5 &&
                        doc.Action == "UpdateRole" &&
                        doc.Details!.Contains(dto.Role)
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Admin updating role for missing user returns 404.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(6, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserReadDto?)null);

            var dto = new UserRoleUpdateDto { Role = UserRoles.User };
            var action = await _controller.UpdateUserRole(6, dto);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Admin updating role with invalid data returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_Invalid_Returns400()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserReadDto { UserId = 7 });
            var dto = new UserRoleUpdateDto { Role = "INVALID" };
            _serviceMock
                .Setup(s => s.UpdateRoleAsync(7, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateUserRole(7, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Invalid role.", bad.Value!);
        }

        /// <summary>
        /// Admin deleting own account returns 400 BadRequest.
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
        /// Admin deleting another user returns 204 NoContent and logs.
        /// </summary>
        [Fact]
        public async Task DeleteUser_Other_Success_ReturnsNoContentAndLogs()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.DeleteUser(3);

            Assert.IsType<NoContentResult>(action);

            _logMock.Verify(
                l => l.LogAsync(
                    It.Is<UserActivityLogDocument>(doc =>
                        doc.UserId == 1 &&
                        doc.Action == "DeleteUser" &&
                        doc.Details!.Contains("Deleted user 3")
                    ),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Admin deleting missing user returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteUser_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            _serviceMock
                .Setup(s => s.DeleteAsync(4, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.DeleteUser(4);

            Assert.IsType<NotFoundResult>(action);
        }
    }
}