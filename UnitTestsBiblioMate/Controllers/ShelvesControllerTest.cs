using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ShelvesController"/>.
    /// Verifies pagination, CRUD endpoints, and error handling.
    /// </summary>
    public class ShelvesControllerTest
    {
        private readonly Mock<IShelfService> _svcMock;
        private readonly ShelvesController    _controller;

        /// <summary>
        /// Initializes the test class with a mocked service
        /// and a fresh instance of <see cref="ShelvesController"/>.
        /// </summary>
        public ShelvesControllerTest()
        {
            _svcMock    = new Mock<IShelfService>();
            _controller = new ShelvesController(_svcMock.Object);
        }

        /// <summary>
        /// Sets the current HttpContext user with a given ID and optional roles.
        /// </summary>
        private void SetUser(int userId, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
                }
            };
        }

        /// <summary>
        /// Getting all shelves should return 200 OK with the expected list.
        /// </summary>
        [Fact]
        public async Task GetShelves_ReturnsOkWithList()
        {
            var list = new List<ShelfReadDto>
            {
                new ShelfReadDto { ShelfId = 1, ZoneId = 5, Name = "A" },
                new ShelfReadDto { ShelfId = 2, ZoneId = 5, Name = "B" }
            };
            _svcMock
                .Setup(s => s.GetAllAsync(10, 2, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetShelves(zoneId: 10, page: 2, pageSize: 5, cancellationToken: CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Getting a shelf by ID that exists should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetShelf_Exists_ReturnsOk()
        {
            var dto = new ShelfReadDto { ShelfId = 7, ZoneId = 3, Name = "C" };
            _svcMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetShelf(7, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Getting a shelf by ID that does not exist should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetShelf_NotFound_Returns404()
        {
            _svcMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShelfReadDto?)null);

            var action = await _controller.GetShelf(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a shelf should return 201 Created with the newly created DTO.
        /// </summary>
        [Fact]
        public async Task CreateShelf_ReturnsCreated()
        {
            SetUser(1, UserRoles.Librarian);
            var createDto = new ShelfCreateDto { ZoneId = 4, Name = "NewShelf" };
            var created   = new ShelfReadDto { ShelfId = 11, ZoneId = 4, Name = "NewShelf" };
            _svcMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var action = await _controller.CreateShelf(createDto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ShelvesController.GetShelf), createdAt.ActionName);
            Assert.Equal(created.ShelfId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched IDs should return 400 BadRequest
        /// with an error and details message.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_IdMismatch_ReturnsBadRequest()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 5, Name = "X" };

            var action = await _controller.UpdateShelf(6, dto, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(action);

            // Tolère un objet anonyme { error, details }, un string, ou ProblemDetails
            // 1) error
            var hasError =
                (bad.Value is string s1 && s1.Contains("mismatch", StringComparison.OrdinalIgnoreCase)) ||
                TryGetAnonString(bad.Value, "error", out var err) && err == "IdMismatch" ||
                bad.Value is ProblemDetails pd1 &&
                    ((pd1.Title?.Contains("mismatch", StringComparison.OrdinalIgnoreCase) ?? false) ||
                     (pd1.Detail?.Contains("mismatch", StringComparison.OrdinalIgnoreCase) ?? false));

            Assert.True(hasError, $"Unexpected BadRequest payload for error: {bad.Value}");

            // 2) details
            var hasDetails =
                (bad.Value is string s2 && s2.Contains("route", StringComparison.OrdinalIgnoreCase) &&
                                          s2.Contains("body",  StringComparison.OrdinalIgnoreCase)) ||
                TryGetAnonString(bad.Value, "details", out var det) &&
                    det == "Route ID and body ID do not match." ||
                bad.Value is ProblemDetails pd2 &&
                    (pd2.Detail?.Contains("Route ID and body ID do not match.", StringComparison.OrdinalIgnoreCase) ?? false);

            Assert.True(hasDetails, $"Unexpected BadRequest payload for details: {bad.Value}");
        }

        /// <summary>
        /// Updating a non-existent shelf should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 8, Name = "Y" };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateShelf(8, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// A successful update should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_Success_ReturnsNoContent()
        {
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 9, Name = "Z" };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateShelf(9, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent shelf should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteShelf_NotFound_Returns404()
        {
            SetUser(1, UserRoles.Librarian);
            _svcMock
                .Setup(s => s.DeleteAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.DeleteShelf(20, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successfully deleting a shelf should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteShelf_Success_ReturnsNoContent()
        {
            SetUser(1, UserRoles.Librarian);
            _svcMock
                .Setup(s => s.DeleteAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.DeleteShelf(21, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        // ---------- helpers (privés au test) ----------

        private static bool TryGetAnonString(object? value, string propName, out string str)
        {
            str = string.Empty;
            if (value is null) return false;
            var p = value.GetType().GetProperty(propName) ??
                    value.GetType().GetProperty(ToPascal(propName));
            if (p == null) return false;
            var v = p.GetValue(value) as string;
            if (v == null) return false;
            str = v;
            return true;
        }

        private static string ToPascal(string name)
            => string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name[1..];
    }
}
