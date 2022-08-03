using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPItemsConfiguration : IEntityTypeConfiguration<RFPItems>
    {
        public void Configure(EntityTypeBuilder<RFPItems> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
