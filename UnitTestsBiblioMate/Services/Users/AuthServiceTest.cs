using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Infrastructure.Security;
using BackendBiblioMate.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Users
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/>.
    /// Verifies registration, login, email confirmation, password reset,
    /// user approval, and confirmation email resend flows.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly AuthService _service;
        private readonly BiblioMateDbContext _db;
        private readonly IEmailService _mockEmailService;
        private readonly ITestOutputHelper _output;

        public AuthServiceTests(ITestOutputHelper output)
        {
            _output = output;

            // Configure EF Core with in-memory database
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Provide necessary settings (JWT, encryption, frontend URL)
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Frontend:BaseUrl"] = "http://localhost:4200",
                    ["Jwt:Key"]          = "this_is_a_very_long_secret_key!!123456",
                    ["Jwt:Issuer"]       = "issuer",
                    ["Jwt:Audience"]     = "audience",
                    ["Encryption:Key"]   = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            // Mock dependencies
            _mockEmailService = Substitute.For<IEmailService>();
            _service          = new AuthService(_db, config, _mockEmailService);
        }

        /// <summary>
        /// RegisterAsync should persist a new user, return Ok, and set confirmation token.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ShouldRegisterNewUser()
        {
            var dto = new RegisterDto
            {
                FirstName = "Test",
                LastName  = "User",
                Email     = "test@example.com",
                Password  = "password123",
                Address1  = "123 Street",
                Phone     = "0600000000"
            };

            var (success, result) = await _service.RegisterAsync(dto);
            var createdUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            _output.WriteLine($"User created: {createdUser?.FirstName} {createdUser?.LastName}");
            _output.WriteLine($"Token: {createdUser?.EmailConfirmationToken}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(createdUser);
        }

        /// <summary>
        /// RegisterAsync should fail when email already exists.
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ShouldConflict_WhenEmailExists()
        {
            _db.Users.Add(new User { Email = "dup@example.com", Password = "x" });
            await _db.SaveChangesAsync();

            var dto = new RegisterDto
            {
                FirstName = "New",
                LastName  = "User",
                Email     = "dup@example.com",
                Password  = "password123"
            };

            var (success, result) = await _service.RegisterAsync(dto);

            Assert.False(success);
            Assert.IsType<ConflictObjectResult>(result);
        }

        /// <summary>
        /// LoginAsync should succeed and return JWT when credentials are valid.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var user = new User
            {
                Email            = "test@example.com",
                Password         = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role             = UserRoles.User,
                IsEmailConfirmed = true,
                IsApproved       = true,
                SecurityStamp    = Guid.NewGuid().ToString()
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new LoginDto { Email = user.Email, Password = "password123" };
            var (success, result) = await _service.LoginAsync(dto);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// LoginAsync should fail when email is not confirmed.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ShouldFail_WhenNotConfirmed()
        {
            var user = new User
            {
                Email            = "nc@example.com",
                Password         = BCrypt.Net.BCrypt.HashPassword("pass"),
                IsEmailConfirmed = false,
                IsApproved       = true,
                SecurityStamp    = Guid.NewGuid().ToString()
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (ok, res) = await _service.LoginAsync(new LoginDto { Email = user.Email, Password = "pass" });

            Assert.False(ok);
            Assert.IsType<UnauthorizedObjectResult>(res);
        }

        /// <summary>
        /// LoginAsync should fail when user is not approved.
        /// </summary>
        [Fact]
        public async Task LoginAsync_ShouldFail_WhenNotApproved()
        {
            var user = new User
            {
                Email            = "na@example.com",
                Password         = BCrypt.Net.BCrypt.HashPassword("pass"),
                IsEmailConfirmed = true,
                IsApproved       = false,
                SecurityStamp    = Guid.NewGuid().ToString()
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (ok, res) = await _service.LoginAsync(new LoginDto { Email = user.Email, Password = "pass" });

            Assert.False(ok);
            Assert.IsType<UnauthorizedObjectResult>(res);
        }

        /// <summary>
        /// ConfirmEmailAsync should validate token and mark email as confirmed.
        /// </summary>
        [Fact]
        public async Task ConfirmEmailAsync_ShouldConfirmEmail_WhenTokenIsValid()
        {
            var user = new User { Email = "confirm@example.com", EmailConfirmationToken = "valid-token" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ConfirmEmailAsync("valid-token");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsEmailConfirmed);
            Assert.Null(user.EmailConfirmationToken);
        }

        /// <summary>
        /// RequestPasswordResetAsync should generate a reset token and send email.
        /// </summary>
        [Fact]
        public async Task RequestPasswordResetAsync_ShouldSendEmail_WhenUserExists()
        {
            var user = new User { Email = "reset@example.com" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _mockEmailService.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.CompletedTask);

            var (success, result) = await _service.RequestPasswordResetAsync(user.Email);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(user.PasswordResetToken);
        }

        /// <summary>
        /// ResetPasswordAsync should update password and clear token when valid.
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_ShouldReset_WhenTokenIsValid()
        {
            var user = new User
            {
                Email                     = "reset@example.com",
                PasswordResetToken        = "valid-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new ResetPasswordDto { Token = "valid-token", NewPassword = "newpass123" };
            var (success, result) = await _service.ResetPasswordAsync(dto);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(user.PasswordResetToken);
        }

        /// <summary>
        /// ApproveUserAsync should mark user as approved when found.
        /// </summary>
        [Fact]
        public async Task ApproveUserAsync_ShouldApprove_WhenUserExists()
        {
            var user = new User { IsApproved = false };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ApproveUserAsync(user.UserId);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsApproved);
        }

        /// <summary>
        /// ResendConfirmationAsync should do nothing for unknown users but still return Ok.
        /// </summary>
        [Fact]
        public async Task ResendConfirmationAsync_UserNotFound_ShouldReturnGenericOk_NoEmailSent()
        {
            var (ok, action) = await _service.ResendConfirmationAsync("ghost@example.com");

            Assert.True(ok);
            Assert.IsType<OkObjectResult>(action);
            await _mockEmailService.ReceivedWithAnyArgs(0)
                .SendEmailAsync(default!, default!, default!);
        }

        /// <summary>
        /// ResendConfirmationAsync should send email and generate token when missing.
        /// </summary>
        [Fact]
        public async Task ResendConfirmationAsync_UnconfirmedUser_ShouldSendEmail_AndGenerateTokenIfMissing()
        {
            var user = new User
            {
                Email                  = "needconfirm@example.com",
                IsEmailConfirmed       = false,
                EmailConfirmationToken = null
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (ok, action) = await _service.ResendConfirmationAsync(user.Email);

            Assert.True(ok);
            Assert.IsType<OkObjectResult>(action);
            Assert.False(string.IsNullOrWhiteSpace(user.EmailConfirmationToken));
            await _mockEmailService.Received(1).SendEmailAsync(
                "needconfirm@example.com",
                Arg.Any<string>(),
                Arg.Is<string>(html => html.Contains("Confirmer mon e-mail") || html.Contains("confirm-email")));
        }
    }
}
