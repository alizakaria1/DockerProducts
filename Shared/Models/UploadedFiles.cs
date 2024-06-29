using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    public class UploadedFiles
    {
        public int FileId { get; set; }
        public int ProductId { get; set; }
        public string Extension { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }

        // Navigation property for the foreign key relationship
        public Product? Product { get; set; }

        [NotMapped]
        public string Url { get; set; }
    }
}
