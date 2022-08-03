using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raybod.SCM.Domain.Configuration
{
    public class ProductGroupConfiguration : IEntityTypeConfiguration<ProductGroup>
    {
        public void Configure(EntityTypeBuilder<ProductGroup> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasIndex(a => a.ProductGroupCode).IsUnique(true);
        }

    }
}

