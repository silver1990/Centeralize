using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPConfiguration : IEntityTypeConfiguration<RFP>
    {
        public void Configure(EntityTypeBuilder<RFP> builder)
        {
            builder.HasIndex(a => a.RFPNumber).IsUnique(true);
            builder.HasMany(x => x.RFPInqueries)
             .WithOne(x => x.RFP)
             .HasForeignKey(x => x.RFPId)
             .IsRequired(true)
             .OnDelete(DeleteBehavior.Restrict);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
