using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RevisionCommentConfiguration : IEntityTypeConfiguration<RevisionComment>
    {
        public void Configure(EntityTypeBuilder<RevisionComment> builder)
        {
            builder.HasMany(p => p.ReplayComments)
                .WithOne(e => e.ParentComment)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
