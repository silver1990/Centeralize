using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class CommunicationQuestionConfiguration : IEntityTypeConfiguration<CommunicationQuestion>
    {
        public void Configure(EntityTypeBuilder<CommunicationQuestion> builder)
        {
            builder.HasMany(p => p.ChildQuestions)
                .WithOne(e => e.ParentQuestion)
                .HasForeignKey(x => x.ParentQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.CommunicationReply)
                .WithOne(e => e.CommunicationQuestion)
                .HasForeignKey<CommunicationReply>(x => x.CommunicationQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a=>a.RowVersion).IsRowVersion();
        }
    }
}
