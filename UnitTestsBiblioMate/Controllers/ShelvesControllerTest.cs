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
        private readonly ShelvesController  _controller;

        public ShelvesControllerTest()
        {
            _svcMock    = new Mock<IShelfService>();
            _controller = new ShelvesController(_svcMock.Object);
        }

        /// <summary>
        /// Helper to set an authenticated user with optional roles.
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
        /// Getting all shelves returns 200 OK with the list.
        /// </summary>
        [Fact]
        public async Task GetShelves_ReturnsOkWithList()
        {
            // Arrange
            var list = new List<ShelfReadDto>
            {
                new ShelfReadDto { ShelfId = 1, ZoneId = 5, Name = "A" },
                new ShelfReadDto { ShelfId = 2, ZoneId = 5, Name = "B" }
            };
            _svcMock
                .Setup(s => s.GetAllAsync(10, 2, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetShelves(zoneId: 10, page: 2, pageSize: 5, cancellationToken: CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Getting a shelf by ID that exists returns 200 OK.
        /// </summary>
        [Fact]
        public async Task GetShelf_Exists_ReturnsOk()
        {
            // Arrange
            var dto = new ShelfReadDto { ShelfId = 7, ZoneId = 3, Name = "C" };
            _svcMock
                .Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetShelf(7, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Getting a shelf by ID that does not exist returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetShelf_NotFound_Returns404()
        {
            // Arrange
            _svcMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShelfReadDto?)null);

            // Act
            var action = await _controller.GetShelf(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a shelf returns 201 Created with location header.
        /// </summary>
        [Fact]
        public async Task CreateShelf_ReturnsCreated()
        {
            // Arrange
            SetUser(1, UserRoles.Librarian);
            var createDto = new ShelfCreateDto { ZoneId = 4, Name = "NewShelf" };
            var created   = new ShelfReadDto { ShelfId = 11, ZoneId = 4, Name = "NewShelf" };
            _svcMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateShelf(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ShelvesController.GetShelf), createdAt.ActionName);
            Assert.Equal(created.ShelfId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched IDs returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 5, Name = "X" };

            // Act
            var action = await _controller.UpdateShelf(6, dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Shelf ID in route and payload do not match.", bad.Value);
        }

        /// <summary>
        /// Updating a non-existent shelf returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_NotFound_Returns404()
        {
            // Arrange
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 8, Name = "Y" };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.UpdateShelf(8, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful update returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateShelf_Success_ReturnsNoContent()
        {
            // Arrange
            SetUser(1, UserRoles.Admin);
            var dto = new ShelfUpdateDto { ShelfId = 9, Name = "Z" };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.UpdateShelf(9, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent shelf returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteShelf_NotFound_Returns404()
        {
            // Arrange
            SetUser(1, UserRoles.Librarian);
            _svcMock
                .Setup(s => s.DeleteAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.DeleteShelf(20, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful delete returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteShelf_Success_ReturnsNoContent()
        {
            // Arrange
            SetUser(1, UserRoles.Librarian);
            _svcMock
                .Setup(s => s.DeleteAsync(21, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteShelf(21, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }
    }
}