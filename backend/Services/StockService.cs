using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Provides domain logic for managing book stock levels and availability flags.
    /// </summary>
    public class StockService
    {
        /// <summary>
        /// Sets the <see cref="Stock.IsAvailable"/> flag based on whether any copies remain.
        /// </summary>
        /// <param name="stock">The stock entry to update.</param>
        public void UpdateAvailability(Stock stock)
        {
            stock.IsAvailable = stock.Quantity > 0;
        }

        /// <summary>
        /// Adjusts the stock quantity by a given amount and recalculates availability.
        /// </summary>
        /// <param name="stock">The stock entry to adjust.</param>
        /// <param name="adjustment">
        /// The amount to change the quantity by (positive to add, negative to remove).
        /// </param>
        public void AdjustQuantity(Stock stock, int adjustment)
        {
            stock.Quantity += adjustment;
            UpdateAvailability(stock);
        }

        /// <summary>
        /// Decreases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to decrement.</param>
        public void Decrease(Stock stock)
        {
            stock.Quantity -= 1;
            UpdateAvailability(stock);
        }

        /// <summary>
        /// Increases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to increment.</param>
        public void Increase(Stock stock)
        {
            stock.Quantity += 1;
            UpdateAvailability(stock);
        }
    }
}