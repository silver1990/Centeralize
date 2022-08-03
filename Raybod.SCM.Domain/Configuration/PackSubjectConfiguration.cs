using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class PackSubjectConfiguration : IEntityTypeConfiguration<PackSubject>
    {
        public void Configure(EntityTypeBuilder<PackSubject> builder)
        {
          

            builder.Property(a => a.RowVersion).IsRowVersion();

        }
    }
}
