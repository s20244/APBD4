using System.Threading.Tasks;
using WarehouseAPI.Models;

namespace WarehouseAPI.Services
{
    public interface IWarehouseService
    {
        Task<int> AddProductToWarehouse(ProductWarehouseRequest request);
    }
}
