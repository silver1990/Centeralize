using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPAttachmentConfiguration : IEntityTypeConfiguration<RFPAttachment>
    {
        public void Configure(EntityTypeBuilder<RFPAttachment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.RFP)
                .WithMany(d => d.RFPAttachments)
                .HasForeignKey(f => f.RFPId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(a => a.RFPSupplierState)
            //    .WithMany(d => d.RFPSupplierStateAttachments)
            //    .HasForeignKey(f => f.RFPSupplierStateId)
            //    .IsRequired(false)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
