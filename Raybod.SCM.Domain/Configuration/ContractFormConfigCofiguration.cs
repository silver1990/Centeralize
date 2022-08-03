using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class ContractFormConfigCofiguration : IEntityTypeConfiguration<ContractFormConfig>
    {
        public void Configure(EntityTypeBuilder<ContractFormConfig> builder)
        {
            builder.HasKey(a => a.ContractFormConfigId);
        }

    }
}
