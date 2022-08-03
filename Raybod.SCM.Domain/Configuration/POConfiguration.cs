using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class POConfiguration : IEntityTypeConfiguration<PO>
    {
        public void Configure(EntityTypeBuilder<PO> builder)
        {
            builder.HasIndex(a => a.POCode).IsUnique(true);
            builder.HasMany(a => a.POTermsOfPayments).WithOne(s => s.PO).HasForeignKey(c => c.POId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(a => a.POStatusLogs).WithOne(s => s.PO).HasForeignKey(c => c.POId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
