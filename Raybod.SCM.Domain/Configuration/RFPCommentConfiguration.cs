using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPCommentConfiguration : IEntityTypeConfiguration<RFPComment>
    {
        public void Configure(EntityTypeBuilder<RFPComment> builder)
        {
            builder.HasMany(p => p.ReplayComments)
                .WithOne(e => e.ParentComment)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
