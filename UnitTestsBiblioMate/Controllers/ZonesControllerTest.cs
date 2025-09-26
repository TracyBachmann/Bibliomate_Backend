using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="ZonesController"/>.
    /// Validates pagination, CRUD endpoints, and error handling.
    /// </summary>
    public class ZonesControllerTest
    {
        private readonly Mock<IZoneService> _serviceMock;
        private readonly ZonesController    _controller;

        public ZonesControllerTest()
        {
            _serviceMock = new Mock<IZoneService>();
            _controller  = new ZonesController(_serviceMock.Object);
        }

        /// <summary>
        /// Retrieving all zones should return 200 OK with the list of zones.
        /// </summary>
        [Fact]
        public async Task GetZones_ReturnsOkWithList()
        {
            // Arrange
            var list = new List<ZoneReadDto>
            {
                new ZoneReadDto { ZoneId = 1, Name = "ZoneA" },
                new ZoneReadDto { ZoneId = 2, Name = "ZoneB" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(2, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetZones(page: 2, pageSize: 5, cancellationToken: CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Retrieving an existing zone by ID should return 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetZone_Exists_ReturnsOk()
        {
            // Arrange
            var dto = new ZoneReadDto { ZoneId = 5, Name = "ZoneX" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var action = await _controller.GetZone(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Retrieving a non-existent zone should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetZone_NotFound_Returns404()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ZoneReadDto?)null);

            // Act
            var action = await _controller.GetZone(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a new zone should return 201 Created with the created DTO.
        /// </summary>
        [Fact]
        public async Task CreateZone_ReturnsCreated()
        {
            // Arrange
            var createDto = new ZoneCreateDto { Name = "NewZone" };
            var created   = new ZoneReadDto { ZoneId = 10, Name = "NewZone" };
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var action = await _controller.CreateZone(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            Assert.Equal(nameof(ZonesController.GetZone), createdAt.ActionName);
            Assert.Equal(created.ZoneId, createdAt.RouteValues!["id"]);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Updating with mismatched IDs between route and payload should return 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateZone_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var dto = new ZoneUpdateDto { ZoneId = 5, Name = "X" };

            // Act
            var action = await _controller.UpdateZone(6, dto, CancellationToken.None);

            // Assert
            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Route ID and payload ZoneId do not match.", bad.Value);
        }

        /// <summary>
        /// Updating a non-existent zone should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateZone_NotFound_Returns404()
        {
            // Arrange
            var dto = new ZoneUpdateDto { ZoneId = 7, Name = "Y" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.UpdateZone(7, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// A successful update should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateZone_Success_ReturnsNoContent()
        {
            // Arrange
            var dto = new ZoneUpdateDto { ZoneId = 8, Name = "Z" };
            _serviceMock
                .Setup(s => s.UpdateAsync(8, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.UpdateZone(8, dto, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }

        /// <summary>
        /// Deleting a non-existent zone should return 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteZone_NotFound_Returns404()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(15, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var action = await _controller.DeleteZone(15, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successfully deleting an existing zone should return 204 NoContent.
        /// </summary>
        [Fact]
        public async Task DeleteZone_Success_ReturnsNoContent()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(16, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var action = await _controller.DeleteZone(16, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(action);
        }
    }
}
