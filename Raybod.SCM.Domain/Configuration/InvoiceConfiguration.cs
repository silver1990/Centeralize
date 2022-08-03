using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.InvoiceNumber).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}