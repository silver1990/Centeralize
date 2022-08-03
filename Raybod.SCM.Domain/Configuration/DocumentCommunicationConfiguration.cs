using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class DocumentCommunicationConfiguration : IEntityTypeConfiguration<DocumentCommunication>
    {
        public void Configure(EntityTypeBuilder<DocumentCommunication> builder)
        {
            builder.HasIndex(a => a.CommunicationCode).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
