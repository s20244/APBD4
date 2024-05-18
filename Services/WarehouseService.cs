using System;
using System.Threading.Tasks;
using WarehouseAPI.Repositories;
using WarehouseAPI.Models;

namespace WarehouseAPI.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<int> AddProductToWarehouse(ProductWarehouseRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Wartości muszą być większe niż 0.");
            }

            var productExists = await _warehouseRepository.CheckIfExists("Product", request.IdProduct);
            var warehouseExists = await _warehouseRepository.CheckIfExists("Warehouse", request.IdWarehouse);

            if (!productExists || !warehouseExists)
            {
                throw new InvalidOperationException("Nie znaleziono produktu i/lub magazynu.");
            }

            var orderId = await _warehouseRepository.GetOrderId(request.IdProduct, request.Amount, request.CreatedAt);
            if (orderId == null)
            {
                throw new InvalidOperationException("Nie znaleziono zamówienia.");
            }

            var isOrderFulfilled = await _warehouseRepository.CheckIfOrderFulfilled(orderId.Value);
            if (isOrderFulfilled)
            {
                throw new InvalidOperationException("Zamówienie zostało już zrealizowane.");
            }

            await _warehouseRepository.UpdateOrderFulfilledAt(orderId.Value);

            var productPrice = await _warehouseRepository.GetProductPrice(request.IdProduct);
            var totalPrice = productPrice * request.Amount;

            var productWarehouseId = await _warehouseRepository.InsertProductWarehouse(request.IdWarehouse, request.IdProduct, orderId.Value, request.Amount, totalPrice);

            return productWarehouseId;
        }
    }
}
