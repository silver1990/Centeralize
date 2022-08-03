using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class SCMAuditLogConfiguration : IEntityTypeConfiguration<SCMAuditLog>
    {
        public void Configure(EntityTypeBuilder<SCMAuditLog> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");            
        }
    }
}
