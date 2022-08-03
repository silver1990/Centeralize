using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raybod.SCM.Domain.Configuration
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            //builder.HasIndex(a => a.DisplayName).IsUnique().IsClustered(false);
            builder.HasIndex(a => a.Name).IsUnique().IsClustered(false);
            builder.Property(e => e.Name).HasColumnType("varchar(50)");
        }
    }
}
