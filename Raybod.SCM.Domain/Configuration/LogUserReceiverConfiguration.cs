using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class LogUserReceiverConfiguration : IEntityTypeConfiguration<LogUserReceiver>
    {
        public void Configure(EntityTypeBuilder<LogUserReceiver> builder)
        {
            builder.HasKey(a => new { a.UserId, a.SCMAuditLogId });
        }
    }
}
