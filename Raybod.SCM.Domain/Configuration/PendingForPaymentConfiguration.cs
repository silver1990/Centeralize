using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PendingForPaymentConfiguration : IEntityTypeConfiguration<PendingForPayment>
    {
        public void Configure(EntityTypeBuilder<PendingForPayment> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.PendingForPaymentNumber).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }

    }
}
