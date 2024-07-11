using BLC.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private FileManager _fileManager;
        private readonly IConfiguration _configuration;

        public FilesController(FileManager fileManager, IConfiguration configuration)
        {
            _fileManager = fileManager;
            _configuration = configuration;
        }

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromQuery]int productId)
        {
            try
            {
                var file = HttpContext.Request.Form.Files[0];

                string fileExtension = Path.GetExtension(file.FileName).Replace(".", "");

                var fileSize = file.Length / 1024;

                var fileToUpload = new UploadedFiles
                {
                    ProductId = productId,
                    FileSize = fileSize,
                    Extension = fileExtension,
                    UploadDate = DateTime.Now
                };

                var uploadedFile = await _fileManager.UploadFile(fileToUpload);

                var path = _configuration.GetValue<string>("FilePath");

                var filePath = string.Format(@"{0}{1}.{2}", path, fileToUpload.FileId, fileExtension);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(uploadedFile);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpDelete("DeleteFile")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            await _fileManager.DeleteFile(fileId);

            return Ok();
        }
    }
}
