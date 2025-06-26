using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Models.Enums;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Text;
using Xunit.Abstractions;

namespace Tests.Services
{
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
                    ["Jwt:Key"] = "this_is_a_very_long_secret_key!!123456",
                    ["Jwt:Issuer"] = "issuer",
                    ["Jwt:Audience"] = "audience",
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
            _output.WriteLine("=== RegisterAsync: START ===");

            var dto = new RegisterDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "password123",
                Address = "123 Street",
                Phone = "0600000000"
            };

            var (success, result) = await _service.RegisterAsync(dto);
            var createdUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            _output.WriteLine($"User created: {createdUser?.Name}, Email: {createdUser?.Email}");
            _output.WriteLine($"EmailConfirmed: {createdUser?.IsEmailConfirmed}");
            _output.WriteLine($"Token: {createdUser?.EmailConfirmationToken}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(createdUser);

            _output.WriteLine("=== RegisterAsync: END ===");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            _output.WriteLine("=== LoginAsync: START ===");

            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Address = "123 Street",
                Phone = "0600000000",
                Role = UserRoles.User,
                IsEmailConfirmed = true,
                IsApproved = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new LoginDto { Email = user.Email, Password = "password123" };
            var (success, result) = await _service.LoginAsync(dto);

            _output.WriteLine($"Login result: {success}, Response: {result}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);

            _output.WriteLine("=== LoginAsync: END ===");
        }

        [Fact]
        public async Task ConfirmEmailAsync_ShouldConfirmEmail_WhenTokenIsValid()
        {
            _output.WriteLine("=== ConfirmEmailAsync: START ===");

            var user = new User
            {
                Email = "confirm@example.com",
                EmailConfirmationToken = "valid-token"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ConfirmEmailAsync("valid-token");

            _output.WriteLine($"Confirm result: {success}, IsConfirmed: {user.IsEmailConfirmed}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsEmailConfirmed);
            Assert.Null(user.EmailConfirmationToken);

            _output.WriteLine("=== ConfirmEmailAsync: END ===");
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ShouldSendEmail_WhenUserExists()
        {
            _output.WriteLine("=== RequestPasswordResetAsync: START ===");

            var user = new User { Email = "reset@example.com" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _mockEmailService.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.CompletedTask);

            var (success, result) = await _service.RequestPasswordResetAsync(user.Email);

            _output.WriteLine($"Reset token: {user.PasswordResetToken}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(user.PasswordResetToken);

            _output.WriteLine("=== RequestPasswordResetAsync: END ===");
        }

        [Fact]
        public async Task ResetPasswordAsync_ShouldReset_WhenTokenIsValid()
        {
            _output.WriteLine("=== ResetPasswordAsync: START ===");

            var user = new User
            {
                Email = "reset@example.com",
                PasswordResetToken = "valid-token",
                PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new ResetPasswordDto { Token = "valid-token", NewPassword = "newpass123" };
            var (success, result) = await _service.ResetPasswordAsync(dto);

            _output.WriteLine($"Password reset: success={success}, token after reset={user.PasswordResetToken}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(user.PasswordResetToken);

            _output.WriteLine("=== ResetPasswordAsync: END ===");
        }

        [Fact]
        public async Task ApproveUserAsync_ShouldApprove_WhenUserExists()
        {
            _output.WriteLine("=== ApproveUserAsync: START ===");

            var user = new User { Name = "User", IsApproved = false };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var (success, result) = await _service.ApproveUserAsync(user.UserId);

            _output.WriteLine($"Approval result: {success}, Approved: {user.IsApproved}");

            Assert.True(success);
            Assert.IsType<OkObjectResult>(result);
            Assert.True(user.IsApproved);

            _output.WriteLine("=== ApproveUserAsync: END ===");
        }
    }
}