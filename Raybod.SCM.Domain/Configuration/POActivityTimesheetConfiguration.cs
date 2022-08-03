using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class POActivityTimesheetConfiguration : IEntityTypeConfiguration<POActivityTimesheet>
    {
        public void Configure(EntityTypeBuilder<POActivityTimesheet> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
