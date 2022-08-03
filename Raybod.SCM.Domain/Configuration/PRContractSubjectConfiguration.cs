using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PRContractSubjectConfiguration : IEntityTypeConfiguration<PRContractSubject>
    {
        public void Configure(EntityTypeBuilder<PRContractSubject> builder)
        {
            builder.HasOne(a => a.PRContract)
                .WithMany(a => a.PRContractSubjects)
                .HasForeignKey(a => a.PRContractId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.RFPItem)
                .WithMany(x => x.PRContractSubjects)
                .HasForeignKey(x => x.RFPItemId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
