using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class LogisticConfiguration : IEntityTypeConfiguration<Logistic>
    {
        public void Configure(EntityTypeBuilder<Logistic> builder)
        {
            builder.Property(a => a.RowVersion).IsRowVersion();
        }
    }
}
