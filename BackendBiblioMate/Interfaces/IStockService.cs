using BackendBiblioMate.Models;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines domain operations for managing book stock levels and availability flags.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Sets the <see cref="Stock.IsAvailable"/> flag based on whether any copies remain.
        /// </summary>
        /// <param name="stock">The <see cref="Stock"/> entry to update.</param>
        void UpdateAvailability(Stock stock);

        /// <summary>
        /// Adjusts the stock <see cref="Stock.Quantity"/> by a given amount and recalculates availability.
        /// </summary>
        /// <param name="stock">The <see cref="Stock"/> entry to adjust.</param>
        /// <param name="adjustment">
        /// The amount to change the quantity by (positive to add, negative to remove).
        /// </param>
        void AdjustQuantity(Stock stock, int adjustment);

        /// <summary>
        /// Decreases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The <see cref="Stock"/> entry to decrement.</param>
        void Decrease(Stock stock);

        /// <summary>
        /// Increases the stock quantity by one and updates availability.
        /// </summary>
        /// <param name="stock">The <see cref="Stock"/> entry to increment.</param>
        void Increase(Stock stock);
    }
}