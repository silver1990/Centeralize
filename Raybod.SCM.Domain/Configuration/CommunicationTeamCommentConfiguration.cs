using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class CommunicationTeamCommentConfiguration : IEntityTypeConfiguration<CommunicationTeamComment>
    {
        public void Configure(EntityTypeBuilder<CommunicationTeamComment> builder)
        {
            builder.HasMany(p => p.ReplayComments)
                .WithOne(e => e.ParentComment)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
