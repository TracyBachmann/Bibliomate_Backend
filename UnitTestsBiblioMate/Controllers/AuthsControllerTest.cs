using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit test suite for <see cref="AuthsController"/>.
    /// Covers registration, login, email confirmation, password reset, 
    /// user approval, and resending confirmation emails.
    /// </summary>
    public class AuthsControllerTest
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthsController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>A mocked <see cref="IAuthService"/> service.</description></item>
        ///   <item><description>An <see cref="AuthsController"/> instance using the mocked service.</description></item>
        /// </list>
        /// </summary>
        public AuthsControllerTest()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller      = new AuthsController(_authServiceMock.Object);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.Register"/> returns
        /// HTTP 200 OK when registration succeeds.
        /// </summary>
        [Fact]
        public async Task Register_ShouldReturnOkOnSuccess()
        {
            var dto    = new RegisterDto { FirstName = "Jane", LastName = "Doe", Email = "jane@x.com", Password = "Pass123!" };
            var action = new OkObjectResult("ok");

            _authServiceMock
                .Setup(s => s.RegisterAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, action));

            var result = await _controller.Register(dto, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("ok", ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.Register"/> returns
        /// HTTP 400 BadRequest when registration fails.
        /// </summary>
        [Fact]
        public async Task Register_ShouldReturnBadRequestOnFailure()
        {
            var dto = new RegisterDto { FirstName = "Jane", LastName = "Doe", Email = "bad", Password = "weak" };
            var bad = new BadRequestObjectResult("invalid");

            _authServiceMock
                .Setup(s => s.RegisterAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, bad));

            var result = await _controller.Register(dto, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.Login"/> returns
        /// HTTP 200 OK with a token when login succeeds.
        /// </summary>
        [Fact]
        public async Task Login_ShouldReturnOkOnSuccess()
        {
            var dto = new LoginDto { Email = "u@x.com", Password = "pass" };
            var ok  = new OkObjectResult(new { token = "jwt" });

            _authServiceMock
                .Setup(s => s.LoginAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.Login(dto, CancellationToken.None);

            var obj = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(ok.Value, obj.Value);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.Login"/> returns
        /// HTTP 401 Unauthorized when login fails.
        /// </summary>
        [Fact]
        public async Task Login_ShouldReturnUnauthorizedOnFailure()
        {
            var dto = new LoginDto { Email = "u@x.com", Password = "wrong" };
            var unauthorized = new UnauthorizedResult();

            _authServiceMock
                .Setup(s => s.LoginAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, unauthorized));

            var result = await _controller.Login(dto, CancellationToken.None);

            Assert.IsType<UnauthorizedResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ConfirmEmail"/> returns
        /// HTTP 200 OK when email confirmation succeeds.
        /// </summary>
        [Fact]
        public async Task ConfirmEmail_ShouldReturnOkOnSuccess()
        {
            const string token = "token";
            var ok = new OkResult();

            _authServiceMock
                .Setup(s => s.ConfirmEmailAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.ConfirmEmail(token, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ConfirmEmail"/> returns
        /// HTTP 400 BadRequest when email confirmation fails.
        /// </summary>
        [Fact]
        public async Task ConfirmEmail_ShouldReturnBadRequestOnFailure()
        {
            const string token = "invalid";
            var bad = new BadRequestObjectResult("bad token");

            _authServiceMock
                .Setup(s => s.ConfirmEmailAsync(token, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, bad));

            var result = await _controller.ConfirmEmail(token, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.RequestPasswordReset"/> returns
        /// HTTP 200 OK when the reset email is successfully sent.
        /// </summary>
        [Fact]
        public async Task RequestPasswordReset_ShouldReturnOkOnSuccess()
        {
            var dto = new RequestPasswordResetDto { Email = "u@x.com" };
            var ok  = new OkObjectResult("sent");

            _authServiceMock
                .Setup(s => s.RequestPasswordResetAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.RequestPasswordReset(dto, CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.RequestPasswordReset"/> returns
        /// HTTP 404 NotFound when the email is not associated with a user.
        /// </summary>
        [Fact]
        public async Task RequestPasswordReset_ShouldReturnNotFoundOnFailure()
        {
            var dto = new RequestPasswordResetDto { Email = "missing@x.com" };
            var notFound = new NotFoundResult();

            _authServiceMock
                .Setup(s => s.RequestPasswordResetAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, notFound));

            var result = await _controller.RequestPasswordReset(dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ResetPassword"/> returns
        /// HTTP 400 BadRequest when reset fails (invalid token or password).
        /// </summary>
        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequestOnFailure()
        {
            var dto = new ResetPasswordDto { Token = "t", NewPassword = "short" };
            var bad = new BadRequestObjectResult("bad");

            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, bad));

            var result = await _controller.ResetPassword(dto, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ResetPassword"/> returns
        /// HTTP 200 OK when reset succeeds with a valid token and password.
        /// </summary>
        [Fact]
        public async Task ResetPassword_ShouldReturnOkOnSuccess()
        {
            var dto = new ResetPasswordDto { Token = "t", NewPassword = "GoodPass123!" };
            var ok  = new OkResult();

            _authServiceMock
                .Setup(s => s.ResetPasswordAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.ResetPassword(dto, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ApproveUser"/> returns
        /// HTTP 404 NotFound when the user does not exist.
        /// </summary>
        [Fact]
        public async Task ApproveUser_ShouldReturnNotFoundOnFailure()
        {
            const int id = 5;
            var notFound = new NotFoundResult();

            _authServiceMock
                .Setup(s => s.ApproveUserAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, notFound));

            var result = await _controller.ApproveUser(id, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ApproveUser"/> returns
        /// HTTP 200 OK when the user is successfully approved.
        /// </summary>
        [Fact]
        public async Task ApproveUser_ShouldReturnOkOnSuccess()
        {
            const int id = 5;
            var ok = new OkResult();

            _authServiceMock
                .Setup(s => s.ApproveUserAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.ApproveUser(id, CancellationToken.None);

            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="AuthsController.ResendConfirmation"/> returns
        /// HTTP 200 OK when the confirmation email is successfully resent.
        /// </summary>
        [Fact]
        public async Task ResendConfirmation_ShouldReturnOkOnSuccess()
        {
            var dto = new ResendEmailConfirmationDto { Email = "u@x.com" };
            var ok  = new OkObjectResult("resent");

            _authServiceMock
                .Setup(s => s.ResendConfirmationAsync(dto.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ok));

            var result = await _controller.ResendConfirmation(dto, CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}

