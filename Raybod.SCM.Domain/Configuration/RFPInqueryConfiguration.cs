using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPInqueryConfiguration : IEntityTypeConfiguration<RFPInquery>
    {
        public void Configure(EntityTypeBuilder<RFPInquery> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
