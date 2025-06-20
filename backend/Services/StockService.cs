using backend.Models;

namespace backend.Services
{
    public class StockService
    {
        /// <summary>
        /// Updates the availability flag based on the current quantity.
        /// </summary>
        public void UpdateAvailability(Stock stock)
        {
            stock.IsAvailable = stock.Quantity > 0;
        }

        /// <summary>
        /// Applies a quantity adjustment and updates availability accordingly.
        /// </summary>
        public void AdjustQuantity(Stock stock, int adjustment)
        {
            stock.Quantity += adjustment;
            UpdateAvailability(stock);
        }

        /// <summary>
        /// Decreases stock quantity by 1 and updates availability.
        /// </summary>
        public void Decrease(Stock stock)
        {
            stock.Quantity -= 1;
            UpdateAvailability(stock);
        }

        /// <summary>
        /// Increases stock quantity by 1 and updates availability.
        /// </summary>
        public void Increase(Stock stock)
        {
            stock.Quantity += 1;
            UpdateAvailability(stock);
        }
    }
}