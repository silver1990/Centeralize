

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class POCommentUserConfiguration : IEntityTypeConfiguration<POCommentUser>
    {
        public void Configure(EntityTypeBuilder<POCommentUser> builder)
        {
            builder.HasKey(a => new { a.UserId, a.POCommentId });
        }
    }

}
