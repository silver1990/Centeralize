using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;
namespace Raybod.SCM.Domain.Configuration
{
    public class MrpConfiguration : IEntityTypeConfiguration<Mrp>
    {
        public void Configure(EntityTypeBuilder<Mrp> builder)
        {
            builder.HasIndex(a => a.MrpNumber).IsUnique(true);
            builder.HasOne(x => x.Contract)
                .WithMany(x => x.Mrps)
                .HasForeignKey(x => x.ContractCode)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
