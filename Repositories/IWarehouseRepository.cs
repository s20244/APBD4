namespace WarehouseAPI.Repositories
{
    public interface IWarehouseRepository
    {
        Task<bool> CheckIfExists(string tableName, int id);
        Task<int?> GetOrderId(int productId, int amount, DateTime createdAt);
        Task<bool> CheckIfOrderFulfilled(int orderId);
        Task UpdateOrderFulfilledAt(int orderId);
        Task<decimal> GetProductPrice(int productId);
        Task<int> InsertProductWarehouse(int warehouseId, int productId, int orderId, int amount, decimal price);
    }
}
