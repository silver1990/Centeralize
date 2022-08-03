using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class TeamWorkUserDocumentGroupConfiguration : IEntityTypeConfiguration<TeamWorkUserDocumentGroup>
    {
        public void Configure(EntityTypeBuilder<TeamWorkUserDocumentGroup> builder)
        {
            builder.HasKey(a => new { a.TeamWorkUserId, a.DocumentGroupId });
        }
    }
}