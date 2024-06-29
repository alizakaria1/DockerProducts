using DALC.Data;
using DALC.IRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace DALC.Repositories
{
    public class FileRepository : IFileRepository
    {
        private DataContext _context;

        public FileRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> DeleteFile(int fileId)
        {
            try
            {
                var file = await _context.UploadedFiles.FirstOrDefaultAsync(x => x.FileId == fileId);

                var result = _context.Remove(file);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<UploadedFiles>> GetFiles(int productId)
        {
            var files = await _context.UploadedFiles.Where(x => x.ProductId == productId).ToListAsync();

            return files;
        }

        public async Task<UploadedFiles> UploadFile(UploadedFiles file)
        {
            var uploadedFile = await _context.UploadedFiles.AddAsync(file);

            await _context.SaveChangesAsync();

            return uploadedFile.Entity;
        }
    }
}
