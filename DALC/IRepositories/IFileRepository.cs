using Shared.Models;

namespace DALC.IRepositories
{
    public interface IFileRepository
    {
        Task<UploadedFiles> UploadFile(UploadedFiles file);
        Task<bool> DeleteFile(int fileId);
        Task<List<UploadedFiles>> GetFiles(int productId);
    }
}
