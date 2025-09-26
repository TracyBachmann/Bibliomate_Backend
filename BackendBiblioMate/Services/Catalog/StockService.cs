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
        /// Initializes a new instance of the <see cref="StockService"/> class
        /// without a database context.
        /// Useful for unit tests where persistence is not required.
        /// </summary>
        public StockService() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StockService"/> class
        /// with an EF Core database context.
        /// </summary>
        /// <param name="context">EF Core database context.</param>
        public StockService(BiblioMateDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Updates the <see cref="Stock.IsAvailable"/> flag based on the current quantity.
        /// </summary>
        /// <param name="stock">The stock entry to update.</param>
        public void UpdateAvailability(Stock stock)
        {
            stock.IsAvailable = stock.Quantity > 0;
        }

        /// <summary>
        /// Adjusts the stock quantity by a given delta, ensuring it never drops below zero.
        /// Also updates the availability flag.  
        /// If a database context is provided, the change is persisted immediately.
        /// </summary>
        /// <param name="stock">The stock entry to modify.</param>
        /// <param name="delta">The amount to adjust (positive to increase, negative to decrease).</param>
        public void AdjustQuantity(Stock stock, int delta)
        {
            var newQty = stock.Quantity + delta;
            stock.Quantity = newQty < 0 ? 0 : newQty;
            stock.IsAvailable = stock.Quantity > 0;

            if (_context != null)
            {
                _context.Stocks.Update(stock);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Increases the stock quantity by 1 and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to increment.</param>
        public void Increase(Stock stock) => AdjustQuantity(stock, +1);

        /// <summary>
        /// Decreases the stock quantity by 1 and updates availability.
        /// </summary>
        /// <param name="stock">The stock entry to decrement.</param>
        public void Decrease(Stock stock) => AdjustQuantity(stock, -1);
    }
}
