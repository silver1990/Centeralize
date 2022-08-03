using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ReceiptSubjectConfiguration : IEntityTypeConfiguration<ReceiptSubject>
    {
        public void Configure(EntityTypeBuilder<ReceiptSubject> builder)
        {
            builder.HasMany(p => p.ReceiptSubjectPartLists)
                .WithOne(e => e.ParentSubject)
                .HasForeignKey(x => x.ParentSubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();

        }
    }
}
