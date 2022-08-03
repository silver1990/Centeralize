using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PurchaseRequestConfiguration : IEntityTypeConfiguration<PurchaseRequest>
    {
        public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
        {
            builder.HasIndex(a => a.PRCode).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
