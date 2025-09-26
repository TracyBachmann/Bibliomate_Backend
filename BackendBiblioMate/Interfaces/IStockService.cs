using BackendBiblioMate.Models;

namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines domain operations for managing book stock levels
    /// and the availability status of a given <see cref="Stock"/>.
    /// A stock entry represents the number of physical copies
    /// available for a specific book in the library.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Recalculates and sets the <see cref="Stock.IsAvailable"/> flag
        /// based on the current <see cref="Stock.Quantity"/>.
        /// </summary>
        /// <param name="stock">
        /// The <see cref="Stock"/> entry to update.
        /// </param>
        /// <remarks>
        /// - If <c>Quantity &gt; 0</c>, <c>IsAvailable</c> is set to <c>true</c>.  
        /// - Otherwise, <c>IsAvailable</c> is set to <c>false</c>.
        /// </remarks>
        void UpdateAvailability(Stock stock);

        /// <summary>
        /// Adjusts the stock <see cref="Stock.Quantity"/> by a given amount
        /// and updates <see cref="Stock.IsAvailable"/> accordingly.
        /// </summary>
        /// <param name="stock">
        /// The <see cref="Stock"/> entry to adjust.
        /// </param>
        /// <param name="adjustment">
        /// The amount to change the quantity by.  
        /// Use a positive value to add copies, or a negative value to remove copies.  
        /// </param>
        /// <remarks>
        /// Quantity is prevented from going below zero.
        /// </remarks>
        void AdjustQuantity(Stock stock, int adjustment);

        /// <summary>
        /// Decreases the <see cref="Stock.Quantity"/> by one and updates availability.
        /// </summary>
        /// <param name="stock">
        /// The <see cref="Stock"/> entry to decrement.
        /// </param>
        /// <remarks>
        /// If <c>Quantity</c> is already zero, the method should prevent it from going negative.
        /// </remarks>
        void Decrease(Stock stock);

        /// <summary>
        /// Increases the <see cref="Stock.Quantity"/> by one and updates availability.
        /// </summary>
        /// <param name="stock">
        /// The <see cref="Stock"/> entry to increment.
        /// </param>
        void Increase(Stock stock);
    }
}
