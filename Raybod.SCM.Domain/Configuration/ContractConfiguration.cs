using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;
using System;

namespace Raybod.SCM.Domain.Configuration
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasKey(c => c.ContractCode);
            builder.HasMany(x => x.AddendumContracts).WithOne(x => x.ParnetContract).HasForeignKey(x => x.ParnetContractCode).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.Property(a=>a.RowVersion).IsRowVersion();
        }
    }
}
