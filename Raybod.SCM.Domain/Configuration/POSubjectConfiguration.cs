using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration
{
    public class POSubjectConfiguration : IEntityTypeConfiguration<POSubject>
    {
        public void Configure(EntityTypeBuilder<POSubject> builder)
        {


            builder.HasOne(a => a.MrpItem)
                .WithMany(a => a.POSubjects)
                .HasForeignKey(a => a.MrpItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
