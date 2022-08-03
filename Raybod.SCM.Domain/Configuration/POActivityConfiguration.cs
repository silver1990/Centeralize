using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class POActivityConfiguration : IEntityTypeConfiguration<POActivity>
    {
        public void Configure(EntityTypeBuilder<POActivity> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
