using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class DocumentTQNCRConfiguration : IEntityTypeConfiguration<DocumentTQNCR>
    {
        public void Configure(EntityTypeBuilder<DocumentTQNCR> builder)
        {
            builder.HasIndex(a => a.CommunicationCode).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
