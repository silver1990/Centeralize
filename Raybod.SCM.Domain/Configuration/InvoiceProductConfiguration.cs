using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class InvoiceProductConfiguration : IEntityTypeConfiguration<InvoiceProduct>
    {
        public void Configure(EntityTypeBuilder<InvoiceProduct> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}