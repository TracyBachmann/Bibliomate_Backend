using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ShelfLevelsController"/>.
    /// Verifies CRUD endpoints, validation rules, and pagination behavior.
    /// </summary>
    public class ShelfLevelsControllerTest
    {
        private readonly Mock<IShelfLevelService> _svcMock;
        private readonly ShelfLevelsController    _controller;

        /// <summary>
        /// Initializes the test class with a mocked service
        /// and a fresh instance of <see cref="ShelfLevelsController"/>.
        /// </summary>
        public ShelfLevelsControllerTest()
        {
            _svcMock    = new Mock<IShelfLevelService>();
            _controller = new ShelfLevelsController(_svcMock.Object);
        }

        /// <summary>
        /// Retrieving all shelf levels should return 200 OK with the expected list.
        /// </summary>
        [Fact]
        public async Task GetShelfLevels_ReturnsOkWithList()
        {
            var list = new List<ShelfLevelReadDto>
            {
                new ShelfLevelReadDto { ShelfLevelId = 1, ShelfId = 10, LevelNumber = 1 },
                new ShelfLevelReadDto { ShelfLevelId = 2, ShelfId = 10, LevelNumber = 2 }
            };
            _svcMock
                .Setup(s => s.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetShelfLevels(page: 1, pageSize: 10, cancellationToken: CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving a shelf level that exists should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetShelfLevel_Exists_ReturnsOk()
        {
            var dto = new ShelfLevelReadDto { ShelfLevelId = 5, ShelfId = 20, LevelNumber = 3 };
            _svcMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var action = await _controller.GetShelfLevel(5, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Retrieving a non-existent shelf level should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetShelfLevel_NotFound_Returns404()
        {
            _svcMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ShelfLevelReadDto?)null);

            var action = await _controller.GetShelfLevel(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a shelf level should return 201 Created with the newly created DTO.
        /// </summary>
        [Fact]
        public async Task CreateShelfLevel_ReturnsCreated()
        {
            var createDto = new ShelfLevelCreateDto { ShelfId = 30, LevelNumber = 4 };
            var created   = new ShelfLevelReadDto { ShelfLevelId = 7, ShelfId = 30, LevelNumber = 4 };
            _svcMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var action = await _controller.CreateShelfLevel(createDto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ShelfLevelsController.GetShelfLevel), createdAt.ActionName);
            Assert.Equal(created.ShelfLevelId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with a mismatched ID should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_IdMismatch_ReturnsBadRequest()
        {
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 8, LevelNumber = 5 };

            var action = await _controller.UpdateShelfLevel(9, dto, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(action);
        }

        /// <summary>
        /// Updating a non-existent shelf level should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_NotFound_Returns404()
        {
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 10, LevelNumber = 6 };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.UpdateShelfLevel(10, dto, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// A successful update should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateShelfLevel_Success_ReturnsNoContent()
        {
            var dto = new ShelfLevelUpdateDto { ShelfLevelId = 11, LevelNumber = 7 };
            _svcMock
                .Setup(s => s.UpdateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.UpdateShelfLevel(11, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent shelf level should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteShelfLevel_NotFound_Returns404()
        {
            _svcMock
                .Setup(s => s.DeleteAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var action = await _controller.DeleteShelfLevel(15, CancellationToken.None);

            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successfully deleting a shelf level should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteShelfLevel_Success_ReturnsNoContent()
        {
            _svcMock
                .Setup(s => s.DeleteAsync(16, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var action = await _controller.DeleteShelfLevel(16, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
        }
    }
}

