using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ReceiptRejectSubjectConfiguration : IEntityTypeConfiguration<ReceiptRejectSubject>
    {
        public void Configure(EntityTypeBuilder<ReceiptRejectSubject> builder)
        {
            builder.HasMany(p => p.ReceiptRejectSubjectPartLists)
                .WithOne(e => e.ParentSubject)
                .HasForeignKey(x => x.ParentSubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();

        }

    }
}
