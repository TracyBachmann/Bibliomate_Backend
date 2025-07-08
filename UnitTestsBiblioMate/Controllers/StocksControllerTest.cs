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
        private readonly StocksController _controller;

        public StocksControllerTest()
        {
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

            // Seed Stocks
            _context.Stocks.AddRange(
                new Stock { StockId = 10, BookId = 1, Quantity = 5, Book = _context.Books.Find(1)! },
                new Stock { StockId = 20, BookId = 2, Quantity = 0, Book = _context.Books.Find(2)! }
            );
            _context.SaveChanges();

            _stockServiceMock = new Mock<IStockService>();
            _controller = new StocksController(_context, _stockServiceMock.Object);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task GetStocks_DefaultPagination_ReturnsAllMapped()
        {
            var action = await _controller.GetStocks(cancellationToken: CancellationToken.None);
            var ok     = Assert.IsType<OkObjectResult>(action.Result);

            var list = Assert.IsAssignableFrom<IEnumerable<StockReadDto>>(ok.Value!).ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, s => s.BookTitle == "Book1" && s.Quantity == 5);
            Assert.Contains(list, s => s.BookTitle == "Book2" && s.Quantity == 0);
        }

        [Fact]
        public async Task GetStock_Exists_ReturnsOk()
        {
            var action = await _controller.GetStock(10, cancellationToken: CancellationToken.None);
            var ok     = Assert.IsType<OkObjectResult>(action.Result);
            var dto    = Assert.IsType<StockReadDto>(ok.Value!);

            Assert.Equal(10, dto.StockId);
            Assert.Equal("Book1", dto.BookTitle);
            Assert.Equal(5, dto.Quantity);
        }

        [Fact]
        public async Task GetStock_NotFound_Returns404()
        {
            var action = await _controller.GetStock(999, cancellationToken: CancellationToken.None);
            Assert.IsType<NotFoundResult>(action.Result);
        }

        [Fact]
        public async Task CreateStock_Conflict_Returns409()
        {
            var dto    = new StockCreateDto { BookId = 1, Quantity = 3 };
            var action = await _controller.CreateStock(dto, cancellationToken: CancellationToken.None);

            var conflict = Assert.IsType<ConflictObjectResult>(action.Result);
            var text     = conflict.Value?.ToString() ?? "";
            Assert.Contains("A stock entry already exists for that book.", text);
        }

        [Fact]
        public async Task CreateStock_New_ReturnsCreated()
        {
            _context.Books.Add(new Book { BookId = 3, Title = "Book3" });
            _context.SaveChanges();

            var dto = new StockCreateDto { BookId = 3, Quantity = 7 };
            _stockServiceMock.Setup(s => s.UpdateAvailability(It.IsAny<Stock>()));

            var action = await _controller.CreateStock(dto, cancellationToken: CancellationToken.None);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action.Result);
            var result    = Assert.IsType<StockReadDto>(createdAt.Value!);

            Assert.Equal(3, result.BookId);
            Assert.Equal("Book3", result.BookTitle);
            Assert.Equal(7, result.Quantity);
        }

        [Fact]
        public async Task UpdateStock_IdMismatch_Returns400()
        {
            var dto    = new StockUpdateDto { StockId = 10, BookId = 1, Quantity = 2, IsAvailable = true };
            var action = await _controller.UpdateStock(11, dto, cancellationToken: CancellationToken.None);

            var bad  = Assert.IsType<BadRequestObjectResult>(action);
            var text = bad.Value?.ToString() ?? "";
            Assert.Contains("Route ID and payload StockId do not match.", text);
        }

        [Fact]
        public async Task UpdateStock_NotFound_Returns404()
        {
            var dto    = new StockUpdateDto { StockId = 99, BookId = 1, Quantity = 2, IsAvailable = true };
            var action = await _controller.UpdateStock(99, dto, cancellationToken: CancellationToken.None);
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task UpdateStock_Success_ReturnsNoContent()
        {
            var dto    = new StockUpdateDto { StockId = 10, BookId = 1, Quantity = 8, IsAvailable = true };
            var action = await _controller.UpdateStock(10, dto, cancellationToken: CancellationToken.None);
            Assert.IsType<NoContentResult>(action);
            var updated = await _context.Stocks.FindAsync(10);
            Assert.Equal(8, updated!.Quantity);
        }

        [Fact]
        public async Task AdjustStockQuantity_ZeroAdjustment_Returns400()
        {
            var dto    = new StockAdjustmentDto { Adjustment = 0 };
            var action = await _controller.AdjustStockQuantity(10, dto, cancellationToken: CancellationToken.None);

            var bad  = Assert.IsType<BadRequestObjectResult>(action.Result);
            var text = bad.Value?.ToString() ?? "";
            Assert.Contains("Adjustment cannot be zero.", text);
        }

        [Fact]
        public async Task AdjustStockQuantity_NotFound_Returns404()
        {
            var dto    = new StockAdjustmentDto { Adjustment = 1 };
            var action = await _controller.AdjustStockQuantity(999, dto, cancellationToken: CancellationToken.None);
            Assert.IsType<NotFoundResult>(action.Result);
        }

        [Fact]
        public async Task AdjustStockQuantity_NegativeResult_Returns400()
        {
            var dto    = new StockAdjustmentDto { Adjustment = -10 };
            var action = await _controller.AdjustStockQuantity(10, dto, cancellationToken: CancellationToken.None);

            var bad  = Assert.IsType<BadRequestObjectResult>(action.Result);
            var text = bad.Value?.ToString() ?? "";
            Assert.Contains("Resulting quantity cannot be negative.", text);
        }

        /// <summary>
        /// Successful adjustment returns 200 OK and updates via service.
        /// </summary>
        [Fact]
        public async Task AdjustStockQuantity_Success_ReturnsOk()
        {
            var dto = new StockAdjustmentDto { Adjustment = -2 };

            // When AdjustQuantity is called, actually decrement the quantity
            _stockServiceMock
                .Setup(s => s.AdjustQuantity(It.IsAny<Stock>(), dto.Adjustment))
                .Callback<Stock, int>((st, adj) => st.Quantity += adj);

            var action = await _controller.AdjustStockQuantity(
                id: 10,
                dto: dto,
                cancellationToken: CancellationToken.None
            );

            var okResult = Assert.IsType<OkObjectResult>(action.Result);
            var anon      = okResult.Value!;
            var anonType  = anon.GetType();

            var msgProp = anonType.GetProperty("message");
            Assert.NotNull(msgProp);
            var message = (string)msgProp.GetValue(anon)!;
            Assert.Equal("Stock updated successfully.", message);

            var qtyProp = anonType.GetProperty("newQuantity");
            Assert.NotNull(qtyProp);
            var newQty = (int)qtyProp.GetValue(anon)!;
            Assert.Equal(3, newQty);

            _stockServiceMock.Verify(s =>
                s.AdjustQuantity(It.Is<Stock>(st => st.StockId == 10), -2),
                Times.Once);
        }

        [Fact]
        public async Task DeleteStock_NotFound_Returns404()
        {
            var action = await _controller.DeleteStock(999, cancellationToken: CancellationToken.None);
            Assert.IsType<NotFoundResult>(action);
        }

        [Fact]
        public async Task DeleteStock_Success_ReturnsNoContent()
        {
            var action = await _controller.DeleteStock(
                id: 20,
                cancellationToken: CancellationToken.None
            );
            Assert.IsType<NoContentResult>(action);
            Assert.Null(await _context.Stocks.FindAsync(20));
        }
    }
}