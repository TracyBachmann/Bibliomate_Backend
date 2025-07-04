using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ShelfLevelsController"/>.
    /// Verifies CRUD endpoints and pagination behavior.
    /// </summary>
    public class ShelfLevelsControllerTest
    {
        private readonly Mock<IShelfLevelService> _svcMock;
        private readonly ShelfLevelsController   _controller;

        public ShelfLevelsControllerTest()
        {
            _svcMock    = new Mock<IShelfLevelService>();
            _controller = new ShelfLevelsController(_svcMock.Object);
        }

        /// <summary>
        /// Retrieving all shelf levels returns 200 OK with the list.
        /// </summary>
        [Fact]
        public async Task GetShelfLevels_ReturnsOkWithList()
        {
            // Arrange
            var list = new List<ShelfLevelReadDto>
            {
                new ShelfLevelReadDto { ShelfLevelId = 1, ShelfId = 10, LevelNumber = 1 },
                new ShelfLevelReadDto { ShelfLevelId = 2, ShelfId = 10, LevelNumber = 2 }
            };
            _svcMock
                .Setup(s => s.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetShelfLevels(page: 1, pageSize: 10, cancellationToken: CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving a specific shelf level that exists returns 200 OK.
        /// </summary>
        [Fact]
        public async Task GetShelfLevel_Exists_ReturnsOk()
        {
            // Arrange
            var dto = new ShelfLevelReadDto { ShelfLevelId = 5, ShelfId = 20, LevelNumber = 3 };
            _svcMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetShelfLevel(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Retrieving a non-existent shelf level returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetShelfLevel_NotFound_Returns404()
        {
            // Arrange
            _svcMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShelfLevelReadDto?)null);

            // Act
            var action = await _controller.GetShelfLevel(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a shelf level returns 201 Created with the DTO.
        /// </summary>
        [Fact]
        public async Task CreateShelfLevel_ReturnsCreated()
        {
            // Arrange
            var createDto = new ShelfLevelCreateDto { ShelfId = 30, LevelNumber = 4 };
            var created   = new ShelfLevelReadDto { ShelfLevelId = 7, ShelfId = 30, LevelNumber = 4 };
            _svcMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateShelfLevel(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ShelfLevelsController.GetShelfLevel), createdAt.ActionName);
            Assert.Equal(created.ShelfLevelId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched ID returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 8, LevelNumber = 5 };

            // Act
            var action = await _controller.UpdateShelfLevel(9, dto, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestResult>(action);
        }

        /// <summary>
        /// Updating a non-existent shelf level returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_NotFound_Returns404()
        {
            // Arrange
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 10, LevelNumber = 6 };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.UpdateShelfLevel(10, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful update returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_Success_ReturnsNoContent()
        {
            // Arrange
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 11, LevelNumber = 7 };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.UpdateShelfLevel(11, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent shelf level returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteShelfLevel_NotFound_Returns404()
        {
            // Arrange
            _svcMock
                .Setup(s => s.DeleteAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.DeleteShelfLevel(15, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful delete returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteShelfLevel_Success_ReturnsNoContent()
        {
            // Arrange
            _svcMock
                .Setup(s => s.DeleteAsync(16, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteShelfLevel(16, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }
    }
}