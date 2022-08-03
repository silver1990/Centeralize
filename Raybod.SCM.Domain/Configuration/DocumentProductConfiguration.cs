using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class DocumentProductConfiguration : IEntityTypeConfiguration<DocumentProduct>
    {
        public void Configure(EntityTypeBuilder<DocumentProduct> builder)
        {
            builder.HasKey(c => new { c.ProductId, c.DocumentId });
            builder.HasOne(a => a.Product)
                .WithMany(a => a.DocumentProducts)
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
