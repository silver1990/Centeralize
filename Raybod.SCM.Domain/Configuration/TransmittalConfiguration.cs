using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class TransmittalConfiguration : IEntityTypeConfiguration<Transmittal>
    {
        public void Configure(EntityTypeBuilder<Transmittal> builder)
        {
            builder.HasIndex(a => a.TransmittalNumber).IsUnique(true);
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
