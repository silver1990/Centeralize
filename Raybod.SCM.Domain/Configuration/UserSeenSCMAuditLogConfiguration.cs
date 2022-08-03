using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class UserSeenSCMAuditLogConfiguration : IEntityTypeConfiguration<UserSeenScmAuditLog>
    {
        public void Configure(EntityTypeBuilder<UserSeenScmAuditLog> builder)
        {
            builder.HasKey(a => new { a.UserId, a.SCMAuditLogId});
        }
    }
}
