using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PRContractConfiguration : IEntityTypeConfiguration<PRContract>
    {
        public void Configure(EntityTypeBuilder<PRContract> builder)
        {
            builder.HasIndex(a => a.PRContractCode).IsUnique(true);

            builder.HasMany(x => x.POs)
             .WithOne(x => x.PRContract)
             .HasForeignKey(x => x.PRContractId)
             .IsRequired(true)
             .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}