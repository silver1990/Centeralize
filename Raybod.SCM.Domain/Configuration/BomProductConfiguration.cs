using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class BomProductConfiguration : IEntityTypeConfiguration<BomProduct>
    {
        public void Configure(EntityTypeBuilder<BomProduct> builder)
        {
          
        }
    }
}
