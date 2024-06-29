using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace DALC.Data
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
                
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<UploadedFiles> UploadedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UploadedFiles>(entity =>
            {
                entity.HasKey(e => e.FileId);
                entity.Property(e => e.FileId).ValueGeneratedOnAdd();
                // Configure the foreign key relationship
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.UploadedFiles)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
