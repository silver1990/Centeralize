using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raybod.SCM.Domain.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(a => a.UserName).IsUnique();
            builder.Property(a => a.RowVersion).IsRowVersion();
            builder.Property(a => a.UserType).HasDefaultValue(1);
        }
    }
}
