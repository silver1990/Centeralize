using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ReceiptRejectConfiguration : IEntityTypeConfiguration<ReceiptReject>
    {
        public void Configure(EntityTypeBuilder<ReceiptReject> builder)
        {
            builder.HasIndex(a => a.ReceiptRejectCode).IsUnique(true);
            builder.HasOne(a => a.Receipt)
                .WithMany(c => c.ReceiptRejects)
                .HasForeignKey(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }

    }
}
