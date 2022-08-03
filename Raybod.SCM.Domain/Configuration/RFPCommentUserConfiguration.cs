using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPCommentUserConfiguration : IEntityTypeConfiguration<RFPCommentUser>
    {
        public void Configure(EntityTypeBuilder<RFPCommentUser> builder)
        {
            builder.HasKey(a => new { a.UserId, a.RFPCommentId });
        }
    }
}
