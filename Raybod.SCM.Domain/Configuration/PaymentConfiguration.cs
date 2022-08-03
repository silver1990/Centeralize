using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(a => a.PaymentId);
            builder.HasIndex(a => a.PaymentNumber).IsUnique(true);
            builder.Property(a=>a.RowVersion).IsRowVersion();
        }

    }
}
