using Raybod.SCM.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Raybod.SCM.Domain.Configuration
{
    public class ContractAttachmentConfiguration : IEntityTypeConfiguration<ContractAttachment>
    {
        public void Configure(EntityTypeBuilder<ContractAttachment> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasOne(x => x.Contract).WithMany(x => x.Attachments).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        }

    }
}
