using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class DocumentRevisionConfiguration : IEntityTypeConfiguration<DocumentRevision>
    {
        public void Configure(EntityTypeBuilder<DocumentRevision> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
