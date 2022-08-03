using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RevisionCommentUserConfiguration : IEntityTypeConfiguration<RevisionCommentUser>
    {
        public void Configure(EntityTypeBuilder<RevisionCommentUser> builder)
        {
            builder.HasKey(a => new { a.UserId, a.RevisionCommentId });
        }
    }
}
