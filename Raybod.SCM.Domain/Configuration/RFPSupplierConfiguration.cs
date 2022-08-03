using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPSupplierConfiguration : IEntityTypeConfiguration<RFPSupplier>
    {
        public void Configure(EntityTypeBuilder<RFPSupplier> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
