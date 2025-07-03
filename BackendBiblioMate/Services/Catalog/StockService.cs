using BackendBiblioMate.Data;
using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models;

namespace BackendBiblioMate.Services.Catalog
{
    /// <summary>
    /// Provides operations for managing stock quantities and availability.
    /// </summary>
    public class StockService : IStockService
    {
        private readonly BiblioMateDbContext? _context;

        /// <summary>
        /// Initializes a new instance of <see cref="StockService"/> without a database context.
        /// Useful for unit tests where no EF operations are required.
        /// </summary>
        public StockService() { }

        /// <summary>
        /// Initializes a new instance of <see cref="StockService"/> with an EF Core context.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public StockService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Updates the stock's availability flag.
        /// No-op if availability is a computed property.
        /// </summary>
        /// <param name="stock">The stock entry to refresh.</param>
        public void UpdateAvailability(Stock stock)
        {
            // No-op: IsAvailable is computed (Quantity > 0).
        }

        /// <summary>
        /// Adjusts the stock quantity by a given delta, ensuring it does not drop below zero.
        /// Persists the change if a database context is available.
        /// </summary>
        /// <param name="stock">The stock entry to modify.</param>
        /// <param name="delta">The amount to add (or subtract if negative).</param>
        public void AdjustQuantity(Stock stock, int delta)
        {
            int newQty = stock.Quantity + delta;
            stock.Quantity = newQty < 0 ? 0 : newQty;

            if (_context != null)
            {
                _context.Stocks.Update(stock);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Increases the stock quantity by 1.
        /// </summary>
        /// <param name="stock">The stock entry to modify.</param>
        public void Increase(Stock stock) => AdjustQuantity(stock, +1);

        /// <summary>
        /// Decreases the stock quantity by 1.
        /// </summary>
        /// <param name="stock">The stock entry to modify.</param>
        public void Decrease(Stock stock) => AdjustQuantity(stock, -1);
    }
}