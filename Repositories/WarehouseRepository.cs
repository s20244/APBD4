using Microsoft.Data.SqlClient;

namespace WarehouseAPI.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WarehouseRepository> _logger;

        public WarehouseRepository(IConfiguration configuration, ILogger<WarehouseRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<bool> CheckIfExists(string tableName, int id)
        {
            var query = $"SELECT COUNT(*) FROM {tableName} WHERE Id{tableName} = @Id";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                await connection.OpenAsync();
                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task<int?> GetOrderId(int productId, int amount, DateTime createdAt)
        {
            var query = @"SELECT TOP 1 IdOrder FROM [Order]
                          WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt <= @CreatedAt";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdProduct", productId);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                if (result == null)
                {
                    _logger.LogWarning($"Nie znaleziono zamówienia na produkt o ID: {productId}, o ilości: {amount}, stworzonego: {createdAt}");
                }
                return result != null ? (int?)result : null;
            }
        }

        public async Task<bool> CheckIfOrderFulfilled(int orderId)
        {
            var query = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdOrder", orderId);
                await connection.OpenAsync();
                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }

        public async Task UpdateOrderFulfilledAt(int orderId)
        {
            var query = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                var gmtPlus2Time = DateTime.UtcNow.AddHours(2);
                command.Parameters.AddWithValue("@FulfilledAt", gmtPlus2Time);
                command.Parameters.AddWithValue("@IdOrder", orderId);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<decimal> GetProductPrice(int productId)
        {
            var query = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@IdProduct", productId);
                await connection.OpenAsync();
                return (decimal)await command.ExecuteScalarAsync();
            }
        }

        public async Task<int> InsertProductWarehouse(int warehouseId, int productId, int orderId, int amount, decimal price)
        {
            var query = @"INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                          VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                          SELECT SCOPE_IDENTITY();";
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                var gmtPlus2Time = DateTime.UtcNow.AddHours(2);
                command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
                command.Parameters.AddWithValue("@IdProduct", productId);
                command.Parameters.AddWithValue("@IdOrder", orderId);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Price", price);
                command.Parameters.AddWithValue("@CreatedAt", gmtPlus2Time);
                await connection.OpenAsync();
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }
    }
}
