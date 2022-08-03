using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class FileDriveCommentConfiguration : IEntityTypeConfiguration<FileDriveComment>
    {
        public void Configure(EntityTypeBuilder<FileDriveComment> builder)
        {
            builder.HasMany(p => p.ReplayComments)
                .WithOne(e => e.ParentComment)
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
  
}
