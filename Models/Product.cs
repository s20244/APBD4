using System.ComponentModel.DataAnnotations;

namespace WarehouseAPI.Models
{
    public class Product
    {
        [Key]
        public int IdProduct { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        public decimal Price { get; set; }
    }
}