using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="GenresController"/>.
    /// Verifies CRUD, search, and ensure endpoints with a mocked <see cref="IGenreService"/>.
    /// </summary>
    public class GenresControllerTest
    {
        private readonly Mock<IGenreService> _serviceMock;
        private readonly GenresController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>A mocked <see cref="IGenreService"/>.</description></item>
        ///   <item><description>An instance of <see cref="GenresController"/> using the mock.</description></item>
        /// </list>
        /// </summary>
        public GenresControllerTest()
        {
            _serviceMock = new Mock<IGenreService>();
            _controller  = new GenresController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.GetGenres"/> returns HTTP 200 OK
        /// with the list of genres when service provides data.
        /// </summary>
        [Fact]
        public async Task GetGenres_ShouldReturnOkWithGenres()
        {
            var list = new List<GenreReadDto>
            {
                new GenreReadDto { GenreId = 1, Name = "G1" },
                new GenreReadDto { GenreId = 2, Name = "G2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var action = await _controller.GetGenres(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.GetGenre"/> returns HTTP 200 OK
        /// when the requested genre exists.
        /// </summary>
        [Fact]
        public async Task GetGenre_ShouldReturnOkWhenFound()
        {
            var dto = new GenreReadDto { GenreId = 5, Name = "Found" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dto, default(ActionResult)));

            var result = await _controller.GetGenre(5, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.GetGenre"/> returns HTTP 404 NotFound
        /// when the genre does not exist.
        /// </summary>
        [Fact]
        public async Task GetGenre_ShouldReturnNotFoundWhenNotExists()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new NotFoundResult()));

            var result = await _controller.GetGenre(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.CreateGenre"/> returns a
        /// <see cref="CreatedAtActionResult"/> with the new genre when created successfully.
        /// </summary>
        [Fact]
        public async Task CreateGenre_ShouldReturnServiceResult()
        {
            var input   = new GenreCreateDto { Name = "NewGenre" };
            var created = new GenreReadDto { GenreId = 10, Name = "NewGenre" };
            var action  = new CreatedAtActionResult(
                actionName: nameof(GenresController.GetGenre),
                controllerName: "Genres",
                routeValues: new { id = created.GenreId },
                value: created);

            _serviceMock
                .Setup(s => s.CreateAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync((created, action));

            var result = await _controller.CreateGenre(input, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdAt.Value);
            Assert.Equal(nameof(GenresController.GetGenre), createdAt.ActionName);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.UpdateGenre"/> returns HTTP 204 NoContent
        /// when update is successful.
        /// </summary>
        [Fact]
        public async Task UpdateGenre_ShouldReturnNoContentWhenSuccess()
        {
            var input = new GenreUpdateDto { Name = "Updated" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateGenre(7, input, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.UpdateGenre"/> returns HTTP 404 NotFound
        /// when the genre does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateGenre_ShouldReturnNotFoundWhenNotExists()
        {
            var input = new GenreUpdateDto { Name = "Nobody" };
            _serviceMock
                .Setup(s => s.UpdateAsync(99, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateGenre(99, input, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.DeleteGenre"/> returns HTTP 204 NoContent
        /// when deletion is successful.
        /// </summary>
        [Fact]
        public async Task DeleteGenre_ShouldReturnNoContentWhenSuccess()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteGenre(3, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.DeleteGenre"/> returns HTTP 404 NotFound
        /// when the genre does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteGenre_ShouldReturnNotFoundWhenNotExists()
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.DeleteGenre(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.Search"/> returns HTTP 200 OK
        /// with matching results when service finds genres.
        /// </summary>
        [Fact]
        public async Task Search_ShouldReturnOkWithResults()
        {
            var results = new List<GenreReadDto>
            {
                new GenreReadDto { GenreId = 1, Name = "Fantasy" }
            };
            _serviceMock
                .Setup(s => s.SearchAsync("Fantasy", 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            var action = await _controller.Search("Fantasy", 20, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.Equal(results, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.Ensure"/> returns HTTP 201 Created
        /// when a new genre is created.
        /// </summary>
        [Fact]
        public async Task Ensure_ShouldReturnCreatedWhenNew()
        {
            var dto  = new GenreCreateDto { Name = "Fresh" };
            var read = new GenreReadDto { GenreId = 11, Name = "Fresh" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, true));

            var result = await _controller.Ensure(dto, CancellationToken.None);

            var created  = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<GenreReadDto>(created.Value);
            Assert.Equal(read.GenreId, returned.GenreId);
        }

        /// <summary>
        /// Ensures <see cref="GenresController.Ensure"/> returns HTTP 200 OK
        /// when the genre already exists.
        /// </summary>
        [Fact]
        public async Task Ensure_ShouldReturnOkWhenExists()
        {
            var dto  = new GenreCreateDto { Name = "Existing" };
            var read = new GenreReadDto { GenreId = 12, Name = "Existing" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, false));

            var result = await _controller.Ensure(dto, CancellationToken.None);

            var ok       = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<GenreReadDto>(ok.Value);
            Assert.Equal(read.GenreId, returned.GenreId);
        }
    }
}
