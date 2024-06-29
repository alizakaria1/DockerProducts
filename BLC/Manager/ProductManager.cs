using DALC.IRepositories;
using Microsoft.Extensions.Configuration;
using Shared.Constants;
using Shared.Models;
using System.Transactions;

namespace BLC.Manager
{
    public class ProductManager
    {
        private IProductRepository _repository;
        private ICacheRepository _cacheRepository;
        private IFileRepository _fileRepository;

        public ProductManager(IProductRepository repository, ICacheRepository cacheRepository, IFileRepository fileRepository)
        {
            _repository = repository;
            _cacheRepository = cacheRepository;
            _fileRepository = fileRepository;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    product.UploadedFiles = new List<UploadedFiles>();

                    var addedProduct = await _repository.AddProductAsync(product);

                    var isRedisSynced = await _cacheRepository.InsertItemAndSyncRedisAsync(addedProduct, $"{CacheConstants.Product}:{addedProduct.Id}", CacheConstants.AllProducts);

                    scope.Complete();

                    return addedProduct;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var files = await _fileRepository.GetFiles(id);

                    if (files.Any() && files != null)
                    {
                        foreach (var file in files)
                        {
                            await _fileRepository.DeleteFile(file.FileId);
                            await _cacheRepository.DeleteItemAndSyncRedisAsync<UploadedFiles>($"{CacheConstants.File}:{file.FileId}", CacheConstants.AllFiles, file.FileId.ToString());
                        }
                    }

                    await _repository.DeleteProductAsync(id);

                    var isRedisSynced = await _cacheRepository.DeleteItemAndSyncRedisAsync<Product>($"{CacheConstants.Product}:{id}" ,CacheConstants.AllProducts, id.ToString());

                    scope.Complete();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
        {
            try
            {
                var products = await _cacheRepository.GetOrAddCachedObjectAsync(CacheConstants.AllProducts, () => _repository.GetAllProductsAsync());

                products.SelectMany(p => p.UploadedFiles)
               .ToList()
               .ForEach(file => file.Url = $"{ConfigVariables.fileUrl}{file.FileId}.{file.Extension}");

                Console.WriteLine($"this is the fileUrl : {ConfigVariables.fileUrl}");
                return products;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _cacheRepository.GetOrAddCachedObjectAsync($"{CacheConstants.Product}:{id}",CacheConstants.AllProducts, () => _repository.GetProductByIdAsync(id));

                foreach (var file in product.UploadedFiles)
                {
                    file.Url = $"{ConfigVariables.fileUrl}{file.FileId}.{file.Extension}";
                }
                Console.WriteLine($"this is the fileUrl : {ConfigVariables.fileUrl}");
                return product;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var updatedProduct = await _repository.UpdateProductAsync(product);

                    var isRedisSynced = await _cacheRepository.UpdateItemAndSyncRedisAsync($"{CacheConstants.Product}:{product.Id}", product, CacheConstants.AllProducts,product.Id.ToString());

                    scope.Complete();

                    return updatedProduct;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}
