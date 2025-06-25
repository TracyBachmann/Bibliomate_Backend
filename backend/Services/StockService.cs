using backend.Models;

namespace backend.Services
{
    /// <summary>
    /// Provides domain logic for managing book stock levels and availability flags.
    /// </summary>
    public class StockService : IStockService
    {
        public void UpdateAvailability(Stock stock)
            => stock.IsAvailable = stock.Quantity > 0;

        public void AdjustQuantity(Stock stock, int adjustment)
        {
            stock.Quantity += adjustment;
            UpdateAvailability(stock);
        }

        public void Decrease(Stock stock)
        {
            stock.Quantity -= 1;
            UpdateAvailability(stock);
        }

        public void Increase(Stock stock)
        {
            stock.Quantity += 1;
            UpdateAvailability(stock);
        }
    }
}