using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
    {
        public void Configure(EntityTypeBuilder<Receipt> builder)
        {
            builder.HasIndex(a => a.ReceiptCode).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
