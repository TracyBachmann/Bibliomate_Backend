using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Defines domain operations for managing book stock levels and availability flags.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Sets the <see cref="Stock.IsAvailable"/> flag based on whether any copies remain.
        /// </summary>
        /// <param name="stock">The stock entry to update.</param>
        void UpdateAvailability(Stock stock);

        /// <summary>
        /// Adjusts the stock quantity by a given amount and recalculates availability.
        /// </summary>
        /// <param name="stock">The stock entry to adjust.</param>
        /// <param name="adjustment">
        /// The amount to change the quantity by (positive to add, negative to remove).
        /// </param>
        void AdjustQuantity(Stock stock, int adjustment);

        /// <summary>
        /// Decreases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to decrement.</param>
        void Decrease(Stock stock);

        /// <summary>
        /// Increases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to increment.</param>
        void Increase(Stock stock);
    }
}