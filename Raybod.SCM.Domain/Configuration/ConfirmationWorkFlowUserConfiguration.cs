using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ConfirmationWorkFlowUserConfiguration : IEntityTypeConfiguration<ConfirmationWorkFlowUser>
    {
        public void Configure(EntityTypeBuilder<ConfirmationWorkFlowUser> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
