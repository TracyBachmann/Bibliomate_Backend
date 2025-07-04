using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="GenresController"/>.
    /// Verifies behavior of all CRUD endpoints with a mocked <see cref="IGenreService"/>.
    /// </summary>
    public class GenresControllerTest
    {
        private readonly Mock<IGenreService> _serviceMock;
        private readonly GenresController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public GenresControllerTest()
        {
            _serviceMock = new Mock<IGenreService>();
            _controller = new GenresController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures that GetGenres returns 200 OK with a list of genres.
        /// </summary>
        [Fact]
        public async Task GetGenres_ShouldReturnOkWithGenres()
        {
            // Arrange
            var list = new List<GenreReadDto>
            {
                new GenreReadDto { GenreId = 1, Name = "G1" },
                new GenreReadDto { GenreId = 2, Name = "G2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var action = await _controller.GetGenres(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures that GetGenre returns 200 OK when the genre exists.
        /// </summary>
        [Fact]
        public async Task GetGenre_ShouldReturnOkWhenFound()
        {
            // Arrange
            var dto = new GenreReadDto { GenreId = 5, Name = "Found" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dto, default(ActionResult)));

            // Act
            var result = await _controller.GetGenre(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that GetGenre returns 404 NotFound when the genre does not exist.
        /// </summary>
        [Fact]
        public async Task GetGenre_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new NotFoundResult()));

            // Act
            var result = await _controller.GetGenre(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that CreateGenre returns the service’s action result.
        /// </summary>
        [Fact]
        public async Task CreateGenre_ShouldReturnServiceResult()
        {
            // Arrange
            var input = new GenreCreateDto { Name = "NewGenre" };
            var created = new GenreReadDto { GenreId = 10, Name = "NewGenre" };
            var action = new CreatedAtActionResult(
                actionName: nameof(GenresController.GetGenre),
                controllerName: "Genres",
                routeValues: new { id = created.GenreId },
                value: created);

            _serviceMock
                .Setup(s => s.CreateAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync((created, action));

            // Act
            var result = await _controller.CreateGenre(input, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdAt.Value);
            Assert.Equal(nameof(GenresController.GetGenre), createdAt.ActionName);
        }

        /// <summary>
        /// Ensures that UpdateGenre returns 204 NoContent when update succeeds.</summary>
        [Fact]
        public async Task UpdateGenre_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            var input = new GenreUpdateDto { Name = "Updated" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateGenre(7, input, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that UpdateGenre returns 404 NotFound when the genre does not exist.</summary>
        [Fact]
        public async Task UpdateGenre_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            var input = new GenreUpdateDto { Name = "Nobody" };
            _serviceMock
                .Setup(s => s.UpdateAsync(99, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateGenre(99, input, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteGenre returns 204 NoContent when deletion succeeds.</summary>
        [Fact]
        public async Task DeleteGenre_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteGenre(3, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteGenre returns 404 NotFound when the genre does not exist.</summary>
        [Fact]
        public async Task DeleteGenre_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteGenre(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}