using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class RFPCommentInqueryConfiguration : IEntityTypeConfiguration<RFPCommentInquery>
    {
        public void Configure(EntityTypeBuilder<RFPCommentInquery> builder)
        {
            builder.HasKey(a => new { a.RFPCommentId, a.RFPInqueryId });
        }

    }
}
