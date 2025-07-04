using BackendBiblioMate.Models;
using BackendBiblioMate.Services.Catalog;

namespace UnitTestsBiblioMate.Services.Catalog
{
    /// <summary>
    /// Unit tests for <see cref="StockService"/> validating quantity adjustment
    /// and availability logic.
    /// </summary>
    public class StockServiceTest
    {
        private readonly StockService _service;

        /// <summary>
        /// Initializes a new test instance with the service under test.
        /// </summary>
        public StockServiceTest()
        {
            _service = new StockService();
        }

        /// <summary>
        /// Verifies that UpdateAvailability correctly sets <see cref="Stock.IsAvailable"/>
        /// based on whether <see cref="Stock.Quantity"/> is greater than zero.
        /// </summary>
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(5, true)]
        public void UpdateAvailability_ShouldReflectComputedIsAvailable(
            int quantity,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = quantity };

            // Act
            _service.UpdateAvailability(stock);

            // Assert
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// Verifies that AdjustQuantity changes the quantity by the given adjustment,
        /// does not go below zero, and updates <see cref="Stock.IsAvailable"/> accordingly.
        /// </summary>
        [Theory]
        [InlineData(5,  3, 8, true)]
        [InlineData(5, -3, 2, true)]
        [InlineData(1, -1, 0, false)]
        [InlineData(0,  0, 0, false)]
        public void AdjustQuantity_ShouldChangeQuantityAndReflectAvailability(
            int initialQty,
            int adjustment,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty };

            // Act
            _service.AdjustQuantity(stock, adjustment);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// Verifies that Decrease subtracts one from <see cref="Stock.Quantity"/>,
        /// does not go below zero, and updates <see cref="Stock.IsAvailable"/>.
        /// </summary>
        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        public void Decrease_ShouldSubtractOneAndReflectAvailability(
            int initialQty,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty };

            // Act
            _service.Decrease(stock);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        /// <summary>
        /// Verifies that Increase adds one to <see cref="Stock.Quantity"/>
        /// and always sets <see cref="Stock.IsAvailable"/> to true.
        /// </summary>
        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(5, 6, true)]
        public void Increase_ShouldAddOneAndReflectAvailability(
            int initialQty,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty };

            // Act
            _service.Increase(stock);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }
    }
}