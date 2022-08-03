using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Raybod.SCM.Domain.Model;

namespace Raybod.SCM.Domain.Configuration 
{
    public class CriteriaConfiguration /*: IEntityTypeConfiguration<Criteria>*/
    {
        //public void Configure(EntityTypeBuilder<Criteria> builder)
        //{
        //    builder.HasOne(x => x.CriteriaTip).WithMany(x => x.Criterias).HasForeignKey(x => x.CriteriaTipId).OnDelete(DeleteBehavior.Restrict);
        //}
    }
}
