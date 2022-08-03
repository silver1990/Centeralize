using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RevisionActivityConfiguration : IEntityTypeConfiguration<RevisionActivity>
    {
        public void Configure(EntityTypeBuilder<RevisionActivity> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
