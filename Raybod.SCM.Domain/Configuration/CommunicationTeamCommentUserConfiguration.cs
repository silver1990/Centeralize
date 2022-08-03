

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class CommunicationTeamCommentUserConfiguration : IEntityTypeConfiguration<CommunicationTeamCommentUser>
    {
        public void Configure(EntityTypeBuilder<CommunicationTeamCommentUser> builder)
        {
            builder.HasKey(a => new { a.UserId, a.CommunicationTeamCommentId });
        }
    }

}
