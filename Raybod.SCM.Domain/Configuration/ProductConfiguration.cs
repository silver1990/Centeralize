using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raybod.SCM.Domain.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(a => a.ProductCode).IsUnique(true);
            builder.HasIndex(a => a.Description).IsUnique(true);
            builder.HasOne(p => p.ProductGroup)
                .WithMany(e => e.Products)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
                      
        }

    }
}
