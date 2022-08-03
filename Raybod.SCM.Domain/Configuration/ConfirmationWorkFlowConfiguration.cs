using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ConfirmationWorkFlowConfiguration : IEntityTypeConfiguration<ConfirmationWorkFlow>
    {
        public void Configure(EntityTypeBuilder<ConfirmationWorkFlow> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
