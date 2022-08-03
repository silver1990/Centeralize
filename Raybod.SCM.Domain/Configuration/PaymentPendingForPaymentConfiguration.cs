using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PaymentPendingForPaymentConfiguration : IEntityTypeConfiguration<PaymentPendingForPayment>
    {
        public void Configure(EntityTypeBuilder<PaymentPendingForPayment> builder)
        {
            builder.HasKey(c => new { c.PaymentId, c.PendingForPaymentId });
        }
    }
}
