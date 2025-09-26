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
    /// Covers CRUD operations, paging with ETag handling,
    /// searching with or without authentication, and genre retrieval.
    /// </summary>
    public class BooksControllerTest
    {
        private readonly Mock<IBookService> _serviceMock;
        private readonly BooksController _controller;

        /// <summary>
        /// Initializes the test environment with:
        /// <list type="bullet">
        ///   <item><description>A mocked <see cref="IBookService"/> service.</description></item>
        ///   <item><description>An instance of <see cref="BooksController"/> using the mock.</description></item>
        ///   <item><description>A default <see cref="HttpContext"/> for simulating requests.</description></item>
        /// </list>
        /// </summary>
        public BooksControllerTest()
        {
            _serviceMock = new Mock<IBookService>();
            _controller  = new BooksController(_serviceMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        /// <summary>
        /// Ensures <see cref="BooksController.GetBooks"/> returns HTTP 200 OK
        /// with a paged result and sets the ETag header when service provides data.
        /// </summary>
        [Fact]
        public async Task GetBooks_ShouldReturnOkWithPageAndEtag()
        {
            var items   = new List<BookReadDto> { new BookReadDto { BookId = 1, Title = "T" } };
            var pageDto = PagedResult<BookReadDto>.Create(items, pageNumber: 1, pageSize: 20, totalCount: items.Count);
            const string eTag = "etag";

            _serviceMock
                .Setup(s => s.GetPagedAsync(1, 20, "Title", true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((pageDto, eTag, default(ActionResult)));

            var result = await _controller.GetBooks(1, 20, "Title", true, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pageDto, ok.Value);
            Assert.Equal(eTag, _controller.Response.Headers["ETag"]);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.GetBooks"/> returns HTTP 304 NotModified
        /// when the provided ETag matches and no new data needs to be sent.
        /// </summary>
        [Fact]
        public async Task GetBooks_ShouldReturnNotModifiedWhenEtagMatches()
        {
            var notModified = new StatusCodeResult(StatusCodes.Status304NotModified);

            _serviceMock
                .Setup(s => s.GetPagedAsync(2, 10, "Title", false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((null!, null!, notModified));

            var result = await _controller.GetBooks(2, 10, "Title", false, CancellationToken.None);

            Assert.Same(notModified, result);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.GetBook"/> returns HTTP 200 OK
        /// when a book with the given ID exists.
        /// </summary>
        [Fact]
        public async Task GetBook_ShouldReturnOkWhenFound()
        {
            var dto = new BookReadDto { BookId = 5, Title = "B" };
            _serviceMock
                .Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await _controller.GetBook(5, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dto, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.GetBook"/> returns HTTP 404 NotFound
        /// when the requested book does not exist.
        /// </summary>
        [Fact]
        public async Task GetBook_ShouldReturnNotFoundWhenNotExists()
        {
            _serviceMock
                .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(BookReadDto));

            var result = await _controller.GetBook(99, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.CreateBook"/> returns HTTP 201 Created
        /// with a <see cref="CreatedAtActionResult"/> referencing <see cref="BooksController.GetBook"/>.
        /// </summary>
        [Fact]
        public async Task CreateBook_ShouldReturnCreatedAtAction()
        {
            var createDto = new BookCreateDto { Title = "New" };
            var created   = new BookReadDto { BookId = 3, Title = "New" };

            _serviceMock
                .Setup(s => s.CreateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var result = await _controller.CreateBook(createDto, CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(BooksController.GetBook), createdAt.ActionName);
            Assert.Equal(created, createdAt.Value);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.UpdateBook"/> returns HTTP 204 NoContent
        /// when the update succeeds, or HTTP 404 NotFound when the book does not exist.
        /// </summary>
        [Theory]
        [InlineData(true, typeof(NoContentResult))]
        [InlineData(false, typeof(NotFoundResult))]
        public async Task UpdateBook_ShouldReturnExpectedResult(bool serviceResult, Type expected)
        {
            var dto = new BookUpdateDto { Title = "U" };

            _serviceMock
                .Setup(s => s.UpdateAsync(7, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            var result = await _controller.UpdateBook(7, dto, CancellationToken.None);

            Assert.IsType(expected, result);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.DeleteBook"/> returns HTTP 204 NoContent
        /// when the deletion succeeds, or HTTP 404 NotFound when the book does not exist.
        /// </summary>
        [Theory]
        [InlineData(true, typeof(NoContentResult))]
        [InlineData(false, typeof(NotFoundResult))]
        public async Task DeleteBook_ShouldReturnExpectedResult(bool serviceResult, Type expected)
        {
            _serviceMock
                .Setup(s => s.DeleteAsync(8, It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceResult);

            var result = await _controller.DeleteBook(8, CancellationToken.None);

            Assert.IsType(expected, result);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.SearchBooks"/> returns HTTP 200 OK
        /// with results when the user is authenticated, passing the user ID to the service.
        /// </summary>
        [Fact]
        public async Task SearchBooks_ShouldReturnOkWithResults()
        {
            var dto     = new BookSearchDto();
            var userId  = 42;
            var results = new List<BookReadDto> { new BookReadDto { BookId = 9, Title = "S" } };

            _serviceMock
                .Setup(s => s.SearchAsync(dto, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"))
                }
            };

            var result = await _controller.SearchBooks(dto, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(results, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.SearchBooks"/> returns HTTP 200 OK
        /// with results when the user is anonymous (no user ID provided).
        /// </summary>
        [Fact]
        public async Task SearchBooks_ShouldReturnOkWithResults_WhenAnonymous()
        {
            var dto     = new BookSearchDto();
            var results = new List<BookReadDto> { new BookReadDto { BookId = 10, Title = "AnonBook" } };

            _serviceMock
                .Setup(s => s.SearchAsync(dto, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(results);

            var result = await _controller.SearchBooks(dto, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(results, ok.Value);
        }

        /// <summary>
        /// Ensures <see cref="BooksController.GetGenres"/> returns HTTP 200 OK
        /// with the list of available genres.
        /// </summary>
        [Fact]
        public async Task GetGenres_ShouldReturnOkWithGenres()
        {
            var genres = new List<string> { "Drama", "Sci-Fi" };

            _serviceMock
                .Setup(s => s.GetAllGenresAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(genres);

            var result = await _controller.GetGenres(CancellationToken.None);

            var ok       = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value);

            Assert.Equal(genres, returned);
        }
    }
}
