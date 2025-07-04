using BackendBiblioMate.Controllers;
using BackendBiblioMate.Data;
using BackendBiblioMate.DTOs;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="StocksController"/>.
    /// Uses an in-memory EF Core context and a mocked <see cref="IStockService"/>.
    /// </summary>
    public class StocksControllerTest : IDisposable
    {
        private readonly BiblioMateDbContext _context;
        private readonly Mock<IStockService> _stockServiceMock;
        private readonly StocksController    _controller;

        public StocksControllerTest()
        {
            // 1) Configure in-memory EF Core with EncryptionService
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var encryptionConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012"))
                })
                .Build();
            var encryptionService = new EncryptionService(encryptionConfig);

            _context = new BiblioMateDbContext(options, encryptionService);

            // Seed Books
            _context.Books.AddRange(
                new Book { BookId = 1, Title = "Book1" },
                new Book { BookId = 2, Title = "Book2" }
            );
            _context.SaveChanges();

            // Seed Stocks (IsAvailable is computed)
            _context.Stocks.AddRange(
                new Stock { StockId = 10, BookId = 1, Quantity = 5, Book = _context.Books.Find(1)! },
                new Stock { StockId = 20, BookId = 2, Quantity = 0, Book = _context.Books.Find(2)! }
            );
            _context.SaveChanges();

            _stockServiceMock = new Mock<IStockService>();
            _controller = new StocksController(_context, _stockServiceMock.Object);
        }

        public void Dispose() => _context.Dispose();

        /// <summary>
        /// Default retrieval should return all stocks, mapped to DTOs.
        /// </summary>
        [Fact]
        public async Task GetStocks_DefaultPagination_ReturnsAllMapped()
        {
            var action = await _controller.GetStocks(cancellationToken: CancellationToken.None);
            var ok     = Assert.IsType<OkObjectResult>(action.Result);

            // materialize into a List so we don't re-enumerate
            var list = Assert.IsAssignableFrom<IEnumerable<StockReadDto>>(ok.Value)
                             .ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, s => s.BookTitle == "Book1" && s.Quantity == 5 && s.IsAvailable);
            Assert.Contains(list, s => s.BookTitle == "Book2" && s.Quantity == 0 && !s.IsAvailable);
        }

        /// <summary>
        /// Retrieving an existing stock returns 200 OK with the DTO.
        /// </summary>
        [Fact]
        public async Task GetStock_Exists_ReturnsOk()
        {
            var action = await _controller.GetStock(10, CancellationToken.None);
            var ok     = Assert.IsType<OkObjectResult>(action.Result);
            var dto    = Assert.IsType<StockReadDto>(ok.Value);

            Assert.Equal(10, dto.StockId);
            Assert.Equal("Book1", dto.BookTitle);
            Assert.Equal(5, dto.Quantity);
            Assert.True(dto.IsAvailable);
        }

        /// <summary>
        /// Retrieving a missing stock returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task GetStock_NotFound_Returns404()
        {
            var action = await _controller.GetStock(999, CancellationToken.None);
            Assert.IsType<NotFoundResult>(action.Result);
        }

        /// <summary>
        /// Creating a duplicate stock returns 409 Conflict.
        /// </summary>
        [Fact]
        public async Task CreateStock_Conflict_Returns409()
        {
            var dto    = new StockCreateDto { BookId = 1, Quantity = 3 };
            var action = await _controller.CreateStock(dto, CancellationToken.None);

            var conflict = Assert.IsType<ConflictObjectResult>(action.Result);
            dynamic body = conflict.Value!;
            Assert.Equal("A stock entry already exists for that book.", (string)body.message);
        }

        /// <summary>
        /// Creating a new stock returns 201 Created and invokes service to set availability.
        /// </summary>
        [Fact]
        public async Task CreateStock_New_ReturnsCreated()
        {
            // Arrange: add BookId=3
            _context.Books.Add(new Book { BookId = 3, Title = "Book3" });
            _context.SaveChanges();

            var dto = new StockCreateDto { BookId = 3, Quantity = 7 };
            _stockServiceMock.Setup(s => s.UpdateAvailability(It.IsAny<Stock>()));

            // Act
            var action = await _controller.CreateStock(dto, CancellationToken.None);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            var result    = Assert.IsType<StockReadDto>(createdAt.Value);

            Assert.Equal(3, result.BookId);
            Assert.Equal("Book3", result.BookTitle);
            Assert.Equal(7, result.Quantity);
            Assert.True(result.IsAvailable);

            _stockServiceMock.Verify(s =>
                s.UpdateAvailability(It.Is<Stock>(st => st.BookId == 3 && st.Quantity == 7)),
                Times.Once);
        }

        /// <summary>
        /// Update with mismatched ID returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateStock_IdMismatch_Returns400()
        {
            var dto    = new StockUpdateDto { StockId = 10, BookId = 1, Quantity = 2, IsAvailable = true };
            var action = await _controller.UpdateStock(11, dto, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            Assert.Equal("Route ID and payload StockId do not match.", bad.Value);
        }

        /// <summary>
        /// Update of non-existent stock returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task UpdateStock_NotFound_Returns404()
        {
            var dto    = new StockUpdateDto { StockId = 99, BookId = 1, Quantity = 2, IsAvailable = true };
            var action = await _controller.UpdateStock(99, dto, CancellationToken.None);
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful update returns 204 NoContent.
        /// </summary>
        [Fact]
        public async Task UpdateStock_Success_ReturnsNoContent()
        {
            var dto    = new StockUpdateDto { StockId = 10, BookId = 1, Quantity = 8, IsAvailable = true };
            var action = await _controller.UpdateStock(10, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(action);
            var updated = await _context.Stocks.FindAsync(10);
            Assert.Equal(8, updated!.Quantity);
        }

        /// <summary>
        /// Adjustment of zero returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task AdjustStockQuantity_ZeroAdjustment_Returns400()
        {
            var dto    = new StockAdjustmentDto { Adjustment = 0 };
            var action = await _controller.AdjustStockQuantity(10, dto, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Adjustment cannot be zero.", (string)body.message);
        }

        /// <summary>
        /// Adjustment on missing stock returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task AdjustStockQuantity_NotFound_Returns404()
        {
            var dto    = new StockAdjustmentDto { Adjustment = 1 };
            var action = await _controller.AdjustStockQuantity(999, dto, CancellationToken.None);
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Negative resulting quantity returns 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task AdjustStockQuantity_NegativeResult_Returns400()
        {
            var dto    = new StockAdjustmentDto { Adjustment = -10 }; // current=5
            var action = await _controller.AdjustStockQuantity(10, dto, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(action);
            dynamic body = bad.Value!;
            Assert.Equal("Resulting quantity cannot be negative.", (string)body.message);
        }

        /// <summary>
        /// Successful adjustment returns 200 OK and updates via service.
        /// </summary>
        [Fact]
        public async Task AdjustStockQuantity_Success_ReturnsOk()
        {
            var dto = new StockAdjustmentDto { Adjustment = -2 };
            _stockServiceMock.Setup(s => s.AdjustQuantity(It.IsAny<Stock>(), dto.Adjustment));

            var action = await _controller.AdjustStockQuantity(10, dto, CancellationToken.None);

            var ok   = Assert.IsType<OkObjectResult>(action);
            dynamic body = ok.Value!;
            Assert.Equal("Stock updated successfully.", (string)body.message);
            Assert.Equal(3, (int)body.newQuantity);
            _stockServiceMock.Verify(s =>
                s.AdjustQuantity(It.Is<Stock>(st => st.StockId == 10), -2),
                Times.Once);
        }

        /// <summary>
        /// Deleting a missing stock returns 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteStock_NotFound_Returns404()
        {
            var action = await _controller.DeleteStock(999, CancellationToken.None);
            Assert.IsType<NotFoundResult>(action);
        }

        /// <summary>
        /// Successful delete returns 204 NoContent and removes from database.
        /// </summary>
        [Fact]
        public async Task DeleteStock_Success_ReturnsNoContent()
        {
            var action = await _controller.DeleteStock(20, CancellationToken.None);
            Assert.IsType<NoContentResult>(action);
            Assert.Null(await _context.Stocks.FindAsync(20));
        }
    }
}