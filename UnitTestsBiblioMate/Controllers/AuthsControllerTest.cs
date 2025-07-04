using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="AuthsController"/>.
    /// Verifies endpoints for registration, login, confirmation, password reset, and approval.
    /// </summary>
    public class AuthsControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthsController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public AuthsControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthsController(_authServiceMock.Object);
        }

        /// <summary>
        /// Ensures that Register returns the service result.
        /// </summary>
        [Fact]
        public async Task Register_ShouldReturnServiceResult()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Name = "User",
                Email = "u@x.com",
                Password = "Pass123!",
                Address = "123 St",
                Phone = "0000000000"
            };
            var action = new CreatedResult("/dummy", null);
            _authServiceMock
                .Setup(s => s.RegisterAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, action));

            // Act
            var result = await _controller.Register(dto, CancellationToken.None);

            // Assert
            Assert.IsType<CreatedResult>(result);
        }

        /// <summary>
        /// Ensures that Login returns the service result.
        /// </summary>
        [Fact]
        public async Task Login_ShouldReturnServiceResult()
        {
            // Arrange
            var dto = new LoginDto { Email = "u@x.com", Password = "pass" };
            var ok = new OkObjectResult(new { Token = "jwt" });
            _authServiceMock
                .Setup(s => s.LoginAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, ok));

            // Act
            var result = await _controller.Login(dto, CancellationToken.None);

            // Assert
            var obj = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(ok.Value, obj.Value);
        }

        /// <summary>
        /// Ensures that ConfirmEmail returns the service result.
        /// </summary>
        [Fact]
        public async Task ConfirmEmail_ShouldReturnServiceResult()
        {
            // Arrange
            const string token = "token";
            var ok = new OkResult();
            _authServiceMock
                .Setup(s => s.ConfirmEmailAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, ok));

            // Act
            var result = await _controller.ConfirmEmail(token, CancellationToken.None);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Ensures that RequestPasswordReset returns the service result.
        /// </summary>
        [Fact]
        public async Task RequestPasswordReset_ShouldReturnServiceResult()
        {
            // Arrange
            var dto = new RequestPasswordResetDto { Email = "u@x.com" };
            var notFound = new NotFoundResult();
            _authServiceMock
                .Setup(s => s.RequestPasswordResetAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, notFound));

            // Act
            var result = await _controller.RequestPasswordReset(dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that ResetPassword returns the service result.
        /// </summary>
        [Fact]
        public async Task ResetPassword_ShouldReturnServiceResult()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                Token = "t",
                NewPassword = "NewPass123!"
            };
            var bad = new BadRequestResult();
            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, bad));

            // Act
            var result = await _controller.ResetPassword(dto, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Ensures that ApproveUser returns the service result.
        /// </summary>
        [Fact]
        public async Task ApproveUser_ShouldReturnServiceResult()
        {
            // Arrange
            const int id = 5;
            var forbid = new ForbidResult();
            _authServiceMock
                .Setup(s => s.ApproveUserAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((default, forbid));

            // Act
            var result = await _controller.ApproveUser(id, CancellationToken.None);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}