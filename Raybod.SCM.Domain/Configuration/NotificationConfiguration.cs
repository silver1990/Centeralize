using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");            
            builder.HasOne(a => a.PerformerUser).WithMany(a => a.PerformerNotifications).HasForeignKey(a => a.PerformerUserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
