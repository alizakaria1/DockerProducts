using DALC.IRepositories;
using Shared.Constants;
using Shared.Models;
using System.Transactions;

namespace BLC.Manager
{
    public class FileManager
    {
        private IFileRepository _fileRepository;
        private ICacheRepository _cacheRepository;
        private IProductRepository _productRepository;

        public FileManager(IFileRepository fileRepository, ICacheRepository cacheRepository, IProductRepository productRepository)
        {
            _fileRepository = fileRepository;
            _cacheRepository = cacheRepository;
            _productRepository = productRepository;
        }

        public async Task<UploadedFiles> UploadFile(UploadedFiles file)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var uploadedFile = await _fileRepository.UploadFile(file);

                    var product = await _cacheRepository.GetOrAddCachedObjectAsync($"{CacheConstants.Product}:{uploadedFile.ProductId}", CacheConstants.AllProducts, () => _productRepository.GetProductByIdAsync(uploadedFile.ProductId));

                    if(product.UploadedFiles == null)
                    {
                        product.UploadedFiles = new List<UploadedFiles>();
                    }

                    product.UploadedFiles.Add(uploadedFile);

                    var syncFiles = await _cacheRepository.InsertItemAndSyncRedisAsync(uploadedFile, $"{CacheConstants.File}:{uploadedFile.FileId}", CacheConstants.AllFiles);

                    var syncProducts = await _cacheRepository.UpdateItemAndSyncRedisAsync($"{CacheConstants.Product}:{uploadedFile.ProductId}", product, CacheConstants.AllProducts, uploadedFile.ProductId.ToString());

                    scope.Complete();

                    return uploadedFile;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task DeleteFile(int fileId)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var deletedFile = await _fileRepository.DeleteFile(fileId);

                    await _cacheRepository.DeleteItemAndSyncRedisAsync<UploadedFiles>($"{CacheConstants.File}:{fileId}", CacheConstants.AllFiles, fileId.ToString());

                    var file = string.Format("{0}{1}.{2}", ConfigVariables.filePath, deletedFile.FileId, deletedFile.Extension);

                    File.Delete(file);
                }
                catch (Exception)
                {

                    throw;
                } 
            }
        }
    }
}
