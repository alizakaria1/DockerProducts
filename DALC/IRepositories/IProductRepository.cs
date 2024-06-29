using Shared.Models;

namespace DALC.IRepositories
{
    public interface IProductRepository
    {
        Task<Product> AddProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> UpdateProductAsync(Product product);
    }
}
