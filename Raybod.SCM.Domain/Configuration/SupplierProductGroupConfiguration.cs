using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class SupplierProductGroupConfiguration : IEntityTypeConfiguration<SupplierProductGroup>
    {
        public void Configure(EntityTypeBuilder<SupplierProductGroup> builder)
        {
            builder.HasKey(a => new { a.SupplierId, a.ProductGroupId });
        }
    }
}
