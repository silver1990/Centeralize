using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class CommunicationReplyConfiguration : IEntityTypeConfiguration<CommunicationReply>
    {
        public void Configure(EntityTypeBuilder<CommunicationReply> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
