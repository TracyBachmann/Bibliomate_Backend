using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Models;
using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Services.Users;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace UnitTestsBiblioMate.Services.Users
{
    /// <summary>
    /// Unit tests for <see cref="UserService"/>.
    /// Validates Create, Read, Update, and Delete operations on users.
    /// </summary>
    public class UserServiceTests
    {
        private readonly UserService _service;
        private readonly BiblioMateDbContext _db;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes an in-memory database and the <see cref="UserService"/>.
        /// </summary>
        public UserServiceTests(ITestOutputHelper output)
        {
            _output = output;

            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // Provide an encryption key for the DbContext constructor
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                        "12345678901234567890123456789012"))
                })
                .Build();

            var encryptionService = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryptionService);

            _service = new UserService(_db);
        }

        /// <summary>
        /// Ensures that CreateAsync adds a new user to the database.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldAddUser()
        {
            _output.WriteLine("=== CreateAsync: START ===");

            // NB: UserCreateDto reste inchangé côté backend (Name/Address)
            var dto = new UserCreateDto
            {
                Name     = "Jean Dupont",
                Email    = "jean.dupont@example.com",
                Password = "Secure123!",
                Address  = "10 rue de la Paix",
                Phone    = "0612345678",
                Role     = UserRoles.User
            };

            var result = await _service.CreateAsync(dto);

            _output.WriteLine($"Created UserId: {result.UserId}, Email: {result.Email}, Role: {result.Role}");

            Assert.NotNull(result);
            Assert.Equal(dto.Email, result.Email);
            Assert.Equal(dto.Role, result.Role);
            Assert.True(await _db.Users.AnyAsync(u => u.UserId == result.UserId));

            _output.WriteLine("=== CreateAsync: END ===");
        }

        /// <summary>
        /// Ensures that GetAllAsync returns all users in the database.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            _output.WriteLine("=== GetAllAsync: START ===");

            _db.Users.Add(MakeUser("User", "A", "a@example.com", UserRoles.User));
            _db.Users.Add(MakeUser("User", "B", "b@example.com", UserRoles.Admin));
            await _db.SaveChangesAsync();

            var users = (await _service.GetAllAsync()).ToList();

            _output.WriteLine($"Found Users Count: {users.Count}");

            Assert.Equal(2, users.Count);

            _output.WriteLine("=== GetAllAsync: END ===");
        }

        /// <summary>
        /// Ensures that GetByIdAsync returns the correct user when it exists.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
        {
            _output.WriteLine("=== GetByIdAsync (exists): START ===");

            var user = MakeUser("Specific", "User", "specific@example.com", UserRoles.Librarian);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var result = await _service.GetByIdAsync(user.UserId);

            _output.WriteLine($"Found User: {result?.Email}");

            Assert.NotNull(result);
            Assert.Equal(user.Email, result!.Email);

            _output.WriteLine("=== GetByIdAsync (exists): END ===");
        }

        /// <summary>
        /// Ensures that GetByIdAsync returns null when the user does not exist.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            _output.WriteLine("=== GetByIdAsync (not exists): START ===");

            var result = await _service.GetByIdAsync(999);

            _output.WriteLine($"Result: {result}");

            Assert.Null(result);

            _output.WriteLine("=== GetByIdAsync (not exists): END ===");
        }

        /// <summary>
        /// Ensures that UpdateAsync modifies an existing user.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldModifyUser_WhenExists()
        {
            _output.WriteLine("=== UpdateAsync (success): START ===");

            var user = MakeUser("Old", "Name", "old@example.com", UserRoles.User);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new UserUpdateDto
            {
                Name    = "Updated Name",                // UserService split -> FirstName=Updated, LastName=Name
                Email   = "updated@example.com",
                Address = "123 rue de la Liberté",       // -> Address1
                Phone   = "0708091011"
            };

            var success = await _service.UpdateAsync(user.UserId, dto);

            _output.WriteLine($"Success: {success}");

            var updated = await _db.Users.FindAsync(user.UserId);

            Assert.True(success);
            Assert.Equal("Updated", updated!.FirstName);
            Assert.Equal("Name", updated.LastName);
            Assert.Equal(dto.Email, updated.Email);
            Assert.Equal(dto.Address, updated.Address1);
            Assert.Equal(dto.Phone, updated.Phone);

            _output.WriteLine("=== UpdateAsync (success): END ===");
        }

        /// <summary>
        /// Ensures that UpdateAsync returns false when the user does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== UpdateAsync (fail): START ===");

            var dto = new UserUpdateDto
            {
                Name    = "Doesn't matter",
                Email   = "nope@example.com",
                Address = "Somewhere",
                Phone   = "000"
            };

            var success = await _service.UpdateAsync(999, dto);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== UpdateAsync (fail): END ===");
        }

        /// <summary>
        /// Ensures that DeleteAsync removes an existing user.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser_WhenExists()
        {
            _output.WriteLine("=== DeleteAsync (success): START ===");

            var user = MakeUser("To", "Delete", "delete@example.com", UserRoles.User);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var success = await _service.DeleteAsync(user.UserId);

            _output.WriteLine($"Success: {success}");

            Assert.True(success);
            Assert.False(await _db.Users.AnyAsync(u => u.UserId == user.UserId));

            _output.WriteLine("=== DeleteAsync (success): END ===");
        }

        /// <summary>
        /// Ensures that DeleteAsync returns false when the user does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
        {
            _output.WriteLine("=== DeleteAsync (fail): START ===");

            var success = await _service.DeleteAsync(999);

            _output.WriteLine($"Success: {success}");

            Assert.False(success);

            _output.WriteLine("=== DeleteAsync (fail): END ===");
        }

        // -------------------- helpers --------------------

        private static User MakeUser(string first, string last, string email, string role) => new User
        {
            FirstName = first,
            LastName  = last,
            Email     = email,
            Password  = "hashed",
            Address1  = "123 St",
            Phone     = "0612345678",
            Role      = role,
            IsEmailConfirmed = true,
            IsApproved       = true,
            SecurityStamp    = Guid.NewGuid().ToString()
        };
    }
}
