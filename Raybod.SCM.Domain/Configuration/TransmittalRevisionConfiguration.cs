using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class TransmittalRevisionConfiguration : IEntityTypeConfiguration<TransmittalRevision>
    {
        public void Configure(EntityTypeBuilder<TransmittalRevision> builder)
        {
            builder.HasKey(a => new { a.DocumentRevisionId, a.TransmittalId });

            builder.HasOne(a => a.Transmittal)
                .WithMany(b => b.TransmittalRevisions)
                .HasForeignKey(v => v.TransmittalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
