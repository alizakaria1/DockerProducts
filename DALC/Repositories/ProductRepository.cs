using DALC.Data;
using DALC.IRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace DALC.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private DataContext _context;

        public ProductRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            try
            {
                var addedProduct = await _context.AddAsync(product);

                await _context.SaveChangesAsync();

                return addedProduct.Entity;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                var productToDelete = await _context.Product.FirstOrDefaultAsync(x => x.Id == id);

                _context.Remove(productToDelete);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _context.Product.Include(f => f.UploadedFiles).ToListAsync();

                return products;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _context.Product.Include(f => f.UploadedFiles).FirstOrDefaultAsync(x => x.Id == id);

            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            try
            {
                var productToUpdate = await _context.Product.FirstOrDefaultAsync(x => x.Id == product.Id);

                productToUpdate.Rating = product.Rating;
                productToUpdate.RatingCount = product.RatingCount;
                productToUpdate.Name = product.Name;
                productToUpdate.Price = product.Price;

                await _context.SaveChangesAsync();

                return product;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
