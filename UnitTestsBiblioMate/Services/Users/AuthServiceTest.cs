using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Users;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Users
{
    /// <summary>
    /// Unit tests for <see cref="AuthService"/>.
    /// Verifies registration, login, email confirmation, password reset, and user approval flows.
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

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Frontend:BaseUrl"] = "http://localhost:4200",
                    ["Jwt:Key"]       = "this_is_a_very_long_secret_key!!123456",
                    ["Jwt:Issuer"]    = "issuer",
                    ["Jwt:Audience"]  = "audience",
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            _mockEmailService = Substitute.For<IEmailService>();
            _service = new AuthService(_db, config, _mockEmailService);
        }

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

            _output.WriteLine($"User created: {createdUser?.FirstName} {createdUser?.LastName}, Email: {createdUser?.Email}");
            _output.WriteLine($"EmailConfirmed: {createdUser?.IsEmailConfirmed}");
            _output.WriteLine($"Token: {createdUser?.EmailConfirmationToken}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(createdUser);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var user = new User
            {
                FirstName       = "Test",
                LastName        = "User",
                Email           = "test@example.com",
                Password        = BCrypt.Net.BCrypt.HashPassword("password123"),
                Address1        = "123 Street",
                Phone           = "0600000000",
                Role            = UserRoles.User,
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

        [Fact]
        public async Task ConfirmEmailAsync_ShouldConfirmEmail_WhenTokenIsValid()
        {
            var user = new User
            {
                FirstName              = "Confirm",
                LastName               = "User",
                Email                  = "confirm@example.com",
                EmailConfirmationToken = "valid-token"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ConfirmEmailAsync("valid-token");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsEmailConfirmed);
            Assert.Null(user.EmailConfirmationToken);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ShouldSendEmail_WhenUserExists()
        {
            var user = new User
            {
                FirstName = "Reset",
                LastName  = "User",
                Email     = "reset@example.com"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _mockEmailService
                .SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.CompletedTask);

            var (success, result) = await _service.RequestPasswordResetAsync(user.Email);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(user.PasswordResetToken);
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReset_WhenTokenIsValid()
        {
            var user = new User
            {
                FirstName                = "Reset",
                LastName                 = "User",
                Email                    = "reset@example.com",
                PasswordResetToken       = "valid-token",
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

        [Fact]
        public async Task ApproveUserAsync_ShouldApprove_WhenUserExists()
        {
            var user = new User
            {
                FirstName  = "Approve",
                LastName   = "User",
                IsApproved = false
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ApproveUserAsync(user.UserId);

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsApproved);
        }
    }
}
