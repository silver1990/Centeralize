using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class TeamWorkUserProductGroupConfiguration : IEntityTypeConfiguration<TeamWorkUserProductGroup>
    {
        public void Configure(EntityTypeBuilder<TeamWorkUserProductGroup> builder)
        {
            builder.HasKey(a => new { a.TeamWorkUserId, a.ProductGroupId });
        }
    }
}
