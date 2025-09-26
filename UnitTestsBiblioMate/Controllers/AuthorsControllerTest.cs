using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit test suite for <see cref="AuthorsController"/>.
    /// Verifies CRUD and search operations using a mocked <see cref="IAuthorService"/>.
    /// </summary>
    public class AuthorsControllerTest
    {
        private readonly Mock<IAuthorService> _serviceMock;
        private readonly AuthorsController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>
        /// A mocked <see cref="IAuthorService"/> using Moq.
        /// </description></item>
        ///   <item><description>
        /// An instance of <see cref="AuthorsController"/> configured
        /// with the mocked dependency.
        /// </description></item>
        /// </list>
        /// </summary>
        public AuthorsControllerTest()
        {
            _serviceMock = new Mock<IAuthorService>();
            _controller  = new AuthorsController(_serviceMock.Object);
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.GetAuthors"/> returns
        /// HTTP 200 OK with a list of authors when authors exist.
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
            var returned = Assert.IsAssignableFrom<IEnumerable<AuthorReadDto>>(ok.Value);
            Assert.Collection(returned,
                item => Assert.Equal("A1", item.Name),
                item => Assert.Equal("A2", item.Name));
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.GetAuthor"/> returns
        /// HTTP 200 OK with the author when the author exists.
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
            var returned = Assert.IsType<AuthorReadDto>(ok.Value);
            Assert.Equal(dto.AuthorId, returned.AuthorId);
            Assert.Equal(dto.Name, returned.Name);
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.GetAuthor"/> returns
        /// HTTP 404 NotFound when the author does not exist.
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
        /// Ensures <see cref="AuthorsController.CreateAuthor"/> returns
        /// HTTP 201 CreatedAtAction with the created author.
        /// </summary>
        [Fact]
        public async Task CreateAuthor_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var input   = new AuthorCreateDto { Name = "New" };
            var created = new AuthorReadDto { AuthorId = 3, Name = "New" };
            var action  = new CreatedAtActionResult(
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
            var returned = Assert.IsType<AuthorReadDto>(createdResult.Value);
            Assert.Equal(created.AuthorId, returned.AuthorId);
            Assert.Equal(created.Name, returned.Name);
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.UpdateAuthor"/> returns
        /// HTTP 204 NoContent when the update succeeds.
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
        /// Ensures <see cref="AuthorsController.UpdateAuthor"/> returns
        /// HTTP 404 NotFound when the author does not exist.
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
        /// Ensures <see cref="AuthorsController.DeleteAuthor"/> returns
        /// HTTP 204 NoContent when deletion succeeds.
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
        /// Ensures <see cref="AuthorsController.DeleteAuthor"/> returns
        /// HTTP 404 NotFound when the author does not exist.
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

        /// <summary>
        /// Ensures <see cref="AuthorsController.SearchAuthors"/> returns
        /// HTTP 200 OK with matching results.
        /// </summary>
        [Fact]
        public async Task SearchAuthors_ShouldReturnOkWithResults()
        {
            // Arrange
            var results = new List<AuthorReadDto>
            {
                new AuthorReadDto { AuthorId = 1, Name = "John" }
            };
            _serviceMock
                .Setup(s => s.SearchAsync("John", 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            // Act
            var result = await _controller.SearchAuthors("John", 20, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsAssignableFrom<IEnumerable<AuthorReadDto>>(ok.Value);
            Assert.Single(returned);
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.EnsureAuthor"/> returns
        /// HTTP 201 Created when a new author is created.
        /// </summary>
        [Fact]
        public async Task EnsureAuthor_ShouldReturnCreatedWhenNew()
        {
            // Arrange
            var dto  = new AuthorCreateDto { Name = "Fresh" };
            var read = new AuthorReadDto { AuthorId = 5, Name = "Fresh" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, true));

            // Act
            var result = await _controller.EnsureAuthor(dto, CancellationToken.None);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<AuthorReadDto>(created.Value);
            Assert.Equal(read.AuthorId, returned.AuthorId);
        }

        /// <summary>
        /// Ensures <see cref="AuthorsController.EnsureAuthor"/> returns
        /// HTTP 200 OK when the author already exists.
        /// </summary>
        [Fact]
        public async Task EnsureAuthor_ShouldReturnOkWhenExists()
        {
            // Arrange
            var dto  = new AuthorCreateDto { Name = "Existing" };
            var read = new AuthorReadDto { AuthorId = 10, Name = "Existing" };

            _serviceMock
                .Setup(s => s.EnsureAsync(dto.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((read, false));

            // Act
            var result = await _controller.EnsureAuthor(dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<AuthorReadDto>(ok.Value);
            Assert.Equal(read.AuthorId, returned.AuthorId);
        }
    }
}
