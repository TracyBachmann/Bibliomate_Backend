using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="AuthorsController"/>.
    /// Verifies behavior of all CRUD endpoints with mocked <see cref="IAuthorService"/>.
    /// </summary>
    public class AuthorsControllerTest
    {
        private readonly Mock<IAuthorService> _serviceMock;
        private readonly AuthorsController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public AuthorsControllerTest()
        {
            _serviceMock = new Mock<IAuthorService>();
            _controller = new AuthorsController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures that GetAuthors returns 200 OK with a list of authors.
        /// </summary>
        [Fact]
        public async Task GetAuthors_ShouldReturnOkWithAuthors()
        {
            // Arrange
            var list = new List<AuthorReadDto>
            {
                new AuthorReadDto { AuthorId = 1, Name = "A1" },
                new AuthorReadDto { AuthorId = 2, Name = "A2" }
            };
            _serviceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            // Act
            var result = await _controller.GetAuthors(CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(list, ok.Value);
        }

        /// <summary>
        /// Ensures that GetAuthor returns 200 OK when the author exists.
        /// </summary>
        [Fact]
        public async Task GetAuthor_ShouldReturnOkWhenFound()
        {
            // Arrange
            var dto = new AuthorReadDto { AuthorId = 1, Name = "Test" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dto, default(ActionResult)));

            // Act
            var result = await _controller.GetAuthor(1, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that GetAuthor returns 404 NotFound when the author does not exist.
        /// </summary>
        [Fact]
        public async Task GetAuthor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, new NotFoundResult()));

            // Act
            var result = await _controller.GetAuthor(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that CreateAuthor returns the service's action result.
        /// </summary>
        [Fact]
        public async Task CreateAuthor_ShouldReturnServiceResult()
        {
            // Arrange
            var input = new AuthorCreateDto { Name = "New" };
            var created = new AuthorReadDto { AuthorId = 3, Name = "New" };
            var action = new CreatedAtActionResult(
                actionName: nameof(AuthorsController.GetAuthor),
                controllerName: "Authors",
                routeValues: new { id = created.AuthorId },
                value: created);

            _serviceMock
                .Setup(s => s.CreateAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync((created, action));

            // Act
            var result = await _controller.CreateAuthor(input, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AuthorsController.GetAuthor), createdResult.ActionName);
        }

        /// <summary>
        /// Ensures that UpdateAuthor returns 204 NoContent when update succeeds.
        /// </summary>
        [Fact]
        public async Task UpdateAuthor_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            var input = new AuthorCreateDto { Name = "Updated" };
            _serviceMock
                .Setup(s => s.UpdateAsync(1, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateAuthor(1, input, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that UpdateAuthor returns 404 NotFound when the author does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateAuthor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            var input = new AuthorCreateDto { Name = "Nobody" };
            _serviceMock
                .Setup(s => s.UpdateAsync(99, input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateAuthor(99, input, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteAuthor returns 204 NoContent when deletion succeeds.
        /// </summary>
        [Fact]
        public async Task DeleteAuthor_ShouldReturnNoContentWhenSuccess()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAuthor(1, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that DeleteAuthor returns 404 NotFound when the author does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteAuthor_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteAuthor(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}