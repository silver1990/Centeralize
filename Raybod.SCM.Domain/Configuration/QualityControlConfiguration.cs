using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class QualityControlConfiguration : IEntityTypeConfiguration<QualityControl>
    {
        public void Configure(EntityTypeBuilder<QualityControl> builder)
        {
            builder.HasMany(a => a.QCAttachments)
                .WithOne(c => c.QualityControl)
                .HasForeignKey(x => x.QualityControlId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}