using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class TeamWorkUserOperationGroupConfiguration : IEntityTypeConfiguration<TeamWorkUserOperationGroup>
    {
        public void Configure(EntityTypeBuilder<TeamWorkUserOperationGroup> builder)
        {
            builder.HasKey(a => new { a.TeamWorkUserId, a.OperationGroupId });
        }
    }
}
