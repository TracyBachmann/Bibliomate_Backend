using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="LocationsController"/>.
    /// Covers endpoints for floors, aisles, shelves, levels, and ensure functionality.
    /// </summary>
    public class LocationsControllerTest
    {
        private readonly Mock<ILocationService> _serviceMock;
        private readonly LocationsController _controller;

        /// <summary>
        /// Initializes a new instance of <see cref="LocationsControllerTest"/>
        /// with a mocked <see cref="ILocationService"/>.
        /// </summary>
        public LocationsControllerTest()
        {
            _serviceMock = new Mock<ILocationService>();
            _controller  = new LocationsController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetFloors"/> 
        /// returns 200 OK with a list of floors when data exists.
        /// </summary>
        [Fact]
        public async Task GetFloors_ShouldReturnOkWithFloors()
        {
            var floors = new List<FloorReadDto> { new FloorReadDto(1), new FloorReadDto(2) };
            _serviceMock
                .Setup(s => s.GetFloorsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(floors);

            var result = await _controller.GetFloors(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(floors, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetAisles"/> 
        /// returns 200 OK when aisles are found for a floor.
        /// </summary>
        [Fact]
        public async Task GetAisles_ShouldReturnOk_WhenFound()
        {
            var aisles = new List<AisleReadDto> { new AisleReadDto("A1") };
            _serviceMock
                .Setup(s => s.GetAislesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(aisles);

            var result = await _controller.GetAisles(1, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(aisles, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetAisles"/> 
        /// returns 404 NotFound when no aisles are found.
        /// </summary>
        [Fact]
        public async Task GetAisles_ShouldReturnNotFound_WhenEmpty()
        {
            _serviceMock
                .Setup(s => s.GetAislesAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AisleReadDto>());

            var result = await _controller.GetAisles(1, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetShelves"/> 
        /// returns 200 OK when shelves exist for a floor and aisle.
        /// </summary>
        [Fact]
        public async Task GetShelves_ShouldReturnOk_WhenFound()
        {
            var shelves = new List<ShelfMiniReadDto> { new ShelfMiniReadDto(1, "S1") };
            _serviceMock
                .Setup(s => s.GetShelvesAsync(1, "A1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(shelves);

            var result = await _controller.GetShelves(1, "A1", CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(shelves, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetShelves"/> 
        /// returns 404 NotFound when no shelves are found.
        /// </summary>
        [Fact]
        public async Task GetShelves_ShouldReturnNotFound_WhenEmpty()
        {
            _serviceMock
                .Setup(s => s.GetShelvesAsync(1, "A1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShelfMiniReadDto>());

            var result = await _controller.GetShelves(1, "A1", CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetLevels"/> 
        /// returns 200 OK when levels exist for a shelf.
        /// </summary>
        [Fact]
        public async Task GetLevels_ShouldReturnOk_WhenFound()
        {
            var levels = new List<LevelReadDto> { new LevelReadDto(1) };
            _serviceMock
                .Setup(s => s.GetLevelsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(levels);

            var result = await _controller.GetLevels(10, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(levels, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.GetLevels"/> 
        /// returns 404 NotFound when no levels are found.
        /// </summary>
        [Fact]
        public async Task GetLevels_ShouldReturnNotFound_WhenEmpty()
        {
            _serviceMock
                .Setup(s => s.GetLevelsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LevelReadDto>());

            var result = await _controller.GetLevels(10, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.Ensure"/> 
        /// returns 400 BadRequest when the input DTO is null.
        /// </summary>
        [Fact]
        public async Task Ensure_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.Ensure(null, CancellationToken.None);

            Assert.IsType<BadRequestResult>(result.Result);
        }

        /// <summary>
        /// Ensures <see cref="LocationsController.Ensure"/> 
        /// returns 200 OK with the ensured location when input is valid.
        /// </summary>
        [Fact]
        public async Task Ensure_ShouldReturnOk_WhenDtoValid()
        {
            var dto = new LocationEnsureDto
            {
                FloorNumber = 1,
                AisleCode   = "A1",
                ShelfName   = "S1",
                LevelNumber = 1
            };
            
            var ensured = new LocationReadDto();

            _serviceMock
                .Setup(s => s.EnsureAsync(
                    It.Is<LocationEnsureDto>(x =>
                        x.FloorNumber == 1 &&
                        x.AisleCode   == "A1" &&
                        x.ShelfName   == "S1" &&
                        x.LevelNumber == 1),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ensured);

            var result = await _controller.Ensure(dto, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(ensured, ok.Value);
        }
    }
}


