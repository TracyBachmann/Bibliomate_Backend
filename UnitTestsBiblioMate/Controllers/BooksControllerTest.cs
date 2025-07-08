using System.Security.Claims;
using BackendBiblioMate.Controllers;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Helpers;
using BackendBiblioMate.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="BooksController"/>.
    /// Verifies CRUD, paging with ETag, and search endpoints.
    /// </summary>
    public class BooksControllerTest
    {
        private readonly Mock<IBookService> _serviceMock;
        private readonly BooksController _controller;

        /// <summary>
        /// Initializes mocks and controller for testing.
        /// </summary>
        public BooksControllerTest()
        {
            _serviceMock = new Mock<IBookService>();
            _controller = new BooksController(_serviceMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        /// <summary>
        /// Ensures that GetBooks returns 200 OK with page and ETag header when data is fresh.
        /// </summary>
        [Fact]
        public async Task GetBooks_ShouldReturnOkWithPageAndEtag()
        {
            // Arrange
            var items = new List<BookReadDto> { new BookReadDto { BookId = 1, Title = "T" } };
            var pageDto = PagedResult<BookReadDto>.Create(items, pageNumber: 1, pageSize: 20, totalCount: items.Count);
            const string eTag = "etag";
            _serviceMock
                .Setup(s => s.GetPagedAsync(1, 20, "Title", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((pageDto, eTag, default(ActionResult)));

            // Act
            var result = await _controller.GetBooks(1, 20, "Title", true, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageDto, ok.Value);
            Assert.Equal(eTag, _controller.Response.Headers["ETag"]);
        }

        /// <summary>
        /// Ensures that GetBooks returns 304 NotModified when ETag matches.
        /// </summary>
        [Fact]
        public async Task GetBooks_ShouldReturnNotModifiedWhenEtagMatches()
        {
            // Arrange
            var notModified = new StatusCodeResult(StatusCodes.Status304NotModified);
            _serviceMock
                .Setup(s => s.GetPagedAsync(2, 10, "Title", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null!, null!, notModified));

            // Act
            var result = await _controller.GetBooks(2, 10, "Title", false, CancellationToken.None);

            // Assert
            Assert.Same(notModified, result);
        }

        /// <summary>
        /// Ensures that GetBook returns 200 OK when found.
        /// </summary>
        [Fact]
        public async Task GetBook_ShouldReturnOkWhenFound()
        {
            // Arrange
            var dto = new BookReadDto { BookId = 5, Title = "B" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.GetBook(5, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures that GetBook returns 404 NotFound when not found.
        /// </summary>
        [Fact]
        public async Task GetBook_ShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(BookReadDto));

            // Act
            var result = await _controller.GetBook(99, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures that CreateBook returns 201 CreatedAtAction.
        /// </summary>
        [Fact]
        public async Task CreateBook_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new BookCreateDto { Title = "New" };
            var created = new BookReadDto { BookId = 3, Title = "New" };
            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var result = await _controller.CreateBook(createDto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(BooksController.GetBook), createdAt.ActionName);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Ensures that UpdateBook returns 204 NoContent on success, 404 when not exists.
        /// </summary>
        [Theory]
        [InlineData(true, typeof(NoContentResult))]
        [InlineData(false, typeof(NotFoundResult))]
        public async Task UpdateBook_ShouldReturnExpectedResult(bool serviceResult, Type expected)
        {
            // Arrange
            var dto = new BookUpdateDto { Title = "U" };
            _serviceMock
                .Setup(s => s.UpdateAsync(7, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.UpdateBook(7, dto, CancellationToken.None);

            // Assert
            Assert.IsType(expected, result);
        }

        /// <summary>
        /// Ensures that DeleteBook returns 204 NoContent on success, 404 when not exists.
        /// </summary>
        [Theory]
        [InlineData(true, typeof(NoContentResult))]
        [InlineData(false, typeof(NotFoundResult))]
        public async Task DeleteBook_ShouldReturnExpectedResult(bool serviceResult, Type expected)
        {
            // Arrange
            _serviceMock
                .Setup(s => s.DeleteAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _controller.DeleteBook(8, CancellationToken.None);

            // Assert
            Assert.IsType(expected, result);
        }

        /// <summary>
        /// Ensures that SearchBooks returns 200 OK with results and respects authentication.
        /// </summary>
        [Fact]
        public async Task SearchBooks_ShouldReturnOkWithResults()
        {
            // Arrange
            var dto = new BookSearchDto();
            var userId = 42;
            var results = new List<BookReadDto> { new BookReadDto { BookId = 9, Title = "S" } };
            _serviceMock
                .Setup(s => s.SearchAsync(dto, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);
            // simulate authenticated user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"))
                }
            };

            // Act
            var result = await _controller.SearchBooks(dto, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(results, ok.Value);
        }
    }
}