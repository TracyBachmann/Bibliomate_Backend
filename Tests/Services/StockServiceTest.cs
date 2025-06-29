using backend.Models;
using backend.Services;

namespace Tests.Services
{
    public class StockServiceTest
    {
        private readonly StockService _service;

        public StockServiceTest()
        {
            _service = new StockService();
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(5, true)]
        public void UpdateAvailability_ShouldSetIsAvailable(int quantity, bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = quantity, IsAvailable = !expectedAvailable };

            // Act
            _service.UpdateAvailability(stock);

            // Assert
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        [Theory]
        [InlineData( 5,  3,  8, true)]
        [InlineData( 5, -3,  2, true)]
        [InlineData( 1, -1,  0, false)]
        [InlineData( 0,  0,  0, false)]
        public void AdjustQuantity_ShouldChangeQuantityAndAvailability(
            int initialQty,
            int adjustment,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty, IsAvailable = initialQty > 0 };

            // Act
            _service.AdjustQuantity(stock, adjustment);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        [Theory]
        [InlineData(1, 0, false)]
        [InlineData(2, 1, true)]
        public void Decrease_ShouldSubtractOneAndUpdateAvailability(
            int initialQty,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty, IsAvailable = initialQty > 0 };

            // Act
            _service.Decrease(stock);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }

        [Theory]
        [InlineData(0, 1, true)]
        [InlineData(5, 6, true)]
        public void Increase_ShouldAddOneAndUpdateAvailability(
            int initialQty,
            int expectedQty,
            bool expectedAvailable)
        {
            // Arrange
            var stock = new Stock { Quantity = initialQty, IsAvailable = initialQty > 0 };

            // Act
            _service.Increase(stock);

            // Assert
            Assert.Equal(expectedQty, stock.Quantity);
            Assert.Equal(expectedAvailable, stock.IsAvailable);
        }
    }
}
