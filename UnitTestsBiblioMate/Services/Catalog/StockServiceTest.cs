using System.Text;
using BackendBiblioMate.Data;
using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;
using BackendBiblioMate.Services.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="StockService"/>.
    /// Covers both:
    /// - Stateless operations (working directly on in-memory Stock objects without persistence).
    /// - Stateful operations with EF Core InMemory provider (changes are persisted to the database).
    /// </summary>
    public class StockServiceTest : IDisposable
    {
        private readonly StockService _statelessService;

        // EF-backed setup for persistence validation
        private readonly BiblioMateDbContext _db;
        private readonly StockService _statefulService;

        /// <summary>
        /// Initializes both a stateless StockService and an EF Core InMemory-backed StockService.
        /// </summary>
        public StockServiceTest()
        {
            // Stateless service (no persistence, operates only on in-memory Stock instances)
            _statelessService = new StockService();

            // ----- EF Core InMemory context for persistence tests -----
            var options = new DbContextOptionsBuilder<BiblioMateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test class instance
                .Options;

            // Provide a valid encryption key for DbContext
            var base64Key = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("12345678901234567890123456789012")
            );
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:Key"] = base64Key
                })
                .Build();

            var encryption = new EncryptionService(config);
            _db = new BiblioMateDbContext(options, encryption);

            // Stateful service uses EF persistence
            _statefulService = new StockService(_db);
        }

        public void Dispose() => _db.Dispose();

        // =====================================================================
        // Stateless tests (no DbContext, operates only on in-memory objects)
        // =====================================================================

        /// <summary>
        /// UpdateAvailability should compute <c>IsAvailable</c> based on quantity.
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(5, true)]
        public void UpdateAvailability_ShouldReflectComputedIsAvailable(int quantity, bool expectedAvailable)
        {
            var stock = new Stock { Quantity = quantity };

            _statelessService.UpdateAvailability(stock);

            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// AdjustQuantity should correctly update Quantity and recompute availability.
        /// Includes increasing, decreasing, and edge cases at zero.
        /// </summary>
        [Theory]
        [InlineData(5,  3, 8, true)]
        [InlineData(5, -3, 2, true)]
        [InlineData(1, -1, 0, false)]
        [InlineData(0,  0, 0, false)]
        public void AdjustQuantity_ShouldChangeQuantityAndReflectAvailability(
            int initialQty, int adjustment, int expectedQty, bool expectedAvailable)
        {
            var stock = new Stock { Quantity = initialQty };

            _statelessService.AdjustQuantity(stock, adjustment);

            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// Decrease should subtract exactly one unit from quantity
        /// and update availability accordingly.
        /// </summary>
        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        public void Decrease_ShouldSubtractOneAndReflectAvailability(
            int initialQty, int expectedQty, bool expectedAvailable)
        {
            var stock = new Stock { Quantity = initialQty };

            _statelessService.Decrease(stock);

            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// Increase should add exactly one unit to quantity
        /// and update availability accordingly.
        /// </summary>
        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(5, 6, true)]
        public void Increase_ShouldAddOneAndReflectAvailability(
            int initialQty, int expectedQty, bool expectedAvailable)
        {
            var stock = new Stock { Quantity = initialQty };

            _statelessService.Increase(stock);

            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        // =====================================================================
        // Stateful tests (with EF Core InMemory persistence)
        // =====================================================================

        /// <summary>
        /// AdjustQuantity should persist changes to the database.
        /// Example: decreasing to zero should also mark IsAvailable as false.
        /// </summary>
        [Fact]
        public void AdjustQuantity_WithContext_PersistsChangeAndAvailability()
        {
            var stock = new Stock { BookId = 1, Quantity = 2, IsAvailable = true };
            _db.Stocks.Add(stock);
            _db.SaveChanges();

            _statefulService.AdjustQuantity(stock, -2);

            var reloaded = _db.Stocks.Single(s => s.StockId == stock.StockId);
            Assert.Equal(0, reloaded.Quantity);
            Assert.False(reloaded.IsAvailable);
        }

        /// <summary>
        /// AdjustQuantity should never allow Quantity to go below zero
        /// when persisted in the database.
        /// </summary>
        [Fact]
        public void AdjustQuantity_WithContext_DoesNotGoBelowZero()
        {
            var stock = new Stock { BookId = 2, Quantity = 1, IsAvailable = true };
            _db.Stocks.Add(stock);
            _db.SaveChanges();

            _statefulService.AdjustQuantity(stock, -5);

            var reloaded = _db.Stocks.Single(s => s.StockId == stock.StockId);
            Assert.Equal(0, reloaded.Quantity);
            Assert.False(reloaded.IsAvailable);
        }

        /// <summary>
        /// Increase should increment Quantity by one and persist the updated availability.
        /// </summary>
        [Fact]
        public void Increase_WithContext_IncrementsAndPersists()
        {
            var stock = new Stock { BookId = 3, Quantity = 0, IsAvailable = false };
            _db.Stocks.Add(stock);
            _db.SaveChanges();

            _statefulService.Increase(stock);

            var reloaded = _db.Stocks.Single(s => s.StockId == stock.StockId);
            Assert.Equal(1, reloaded.Quantity);
            Assert.True(reloaded.IsAvailable);
        }

        /// <summary>
        /// Decrease should subtract one from Quantity and persist the change.
        /// </summary>
        [Fact]
        public void Decrease_WithContext_DecrementsAndPersists()
        {
            var stock = new Stock { BookId = 4, Quantity = 2, IsAvailable = true };
            _db.Stocks.Add(stock);
            _db.SaveChanges();

            _statefulService.Decrease(stock);

            var reloaded = _db.Stocks.Single(s => s.StockId == stock.StockId);
            Assert.Equal(1, reloaded.Quantity);
            Assert.True(reloaded.IsAvailable);
        }

        /// <summary>
        /// UpdateAvailability persists immédiatement la nouvelle disponibilité
        /// lorsque le service est connecté à un DbContext (comportement actuel).
        /// </summary>
        [Fact]
        public void UpdateAvailability_WithContext_AutoPersists()
        {
            var stock = new Stock { BookId = 5, Quantity = 0, IsAvailable = true };
            _db.Stocks.Add(stock);
            _db.SaveChanges();

            // Appel du service : la dispo doit être recalculée et enregistrée
            _statefulService.UpdateAvailability(stock);

            // Vérifie que la valeur en BDD est déjà mise à jour sans SaveChanges manuel ici
            var immediate = _db.Stocks.AsNoTracking().Single(s => s.StockId == stock.StockId);
            Assert.False(immediate.IsAvailable);

            // Idempotence après un SaveChanges explicite
            _db.Stocks.Update(stock);
            _db.SaveChanges();

            var afterSave = _db.Stocks.AsNoTracking().Single(s => s.StockId == stock.StockId);
            Assert.False(afterSave.IsAvailable);
        }
    }
}
