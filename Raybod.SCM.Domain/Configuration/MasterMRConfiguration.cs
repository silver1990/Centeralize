using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class MasterMRConfiguration : IEntityTypeConfiguration<MasterMR>
    {
        public void Configure(EntityTypeBuilder<MasterMR> builder)
        {
            builder.HasOne(x => x.Product)
                .WithMany(x => x.MasterMRs)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.BomProduct)
                .WithMany(x => x.MasterBomProducts)
                .HasForeignKey(x => x.BomProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();

        }
    }
}
