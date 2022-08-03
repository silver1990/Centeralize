using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.HasIndex(a => a.SupplierCode).IsUnique(true);

            builder.HasMany(a => a.FinancialAccounts)
                .WithOne(c => c.Supplier)
                .HasForeignKey(a => a.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Invoices)
              .WithOne(c => c.Supplier)
              .HasForeignKey(a => a.SupplierId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
