using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PackConfiguration : IEntityTypeConfiguration<Pack>
    {
        public void Configure(EntityTypeBuilder<Pack> builder)
        {
            builder.HasKey(a => a.PackId);
            builder.HasIndex(a => a.PackCode).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();

            builder
              .HasMany(a => a.PackSubjects)
              .WithOne(a => a.Pack)
              .HasForeignKey(c => c.PackId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
