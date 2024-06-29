namespace Shared.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public string Description { get; set; }

        public ICollection<UploadedFiles>? UploadedFiles { get; set; }
    }
}
