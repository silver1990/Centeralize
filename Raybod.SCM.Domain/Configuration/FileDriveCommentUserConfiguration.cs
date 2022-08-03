using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class FileDriveCommentUserConfiguration : IEntityTypeConfiguration<FileDriveCommentUser>
    {
        public void Configure(EntityTypeBuilder<FileDriveCommentUser> builder)
        {
            builder.HasKey(a => new { a.UserId, a.CommentId });
        }
    }
}
