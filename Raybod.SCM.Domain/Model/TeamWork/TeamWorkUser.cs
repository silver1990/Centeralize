using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class TeamWorkUser
    {
        [Key]
        public int Id { get; set; }

        public int TeamWorkId { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(TeamWorkId))]
        public virtual TeamWork TeamWork { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public virtual ICollection<TeamWorkUserRole> TeamWorkUserRoles { get; set; }

        public virtual ICollection<TeamWorkUserProductGroup> TeamWorkUserProductGroups { get; set; }

        public virtual ICollection<TeamWorkUserDocumentGroup> TeamWorkUserDocumentGroups { get; set; }

        public virtual ICollection<TeamWorkUserOperationGroup> TeamWorkUserOperationGroups { get; set; }
        // [
        //     {
        //     contratCode="aj04",
        //     title=" some",
        //     permission=[
        //             {
        //             module="MRP",
        //             roleIds=[1,3,5,8]
        //             },
        //             {
        //             module="RFP",
        //             roleIds=[1,3,5,8]
        //             }
        //     ]},
        //     {
        //     contratCode="PK02",
        //     title="some",
        //     permission=[
        //         {
        //         module="PR",
        //         roleIds=[1,3,5,8]
        //         },
        //         {
        //         module="MDR",
        //         roleIds=[1,3,5,8]
        //         }
        //      ]},
        //]

    }
}
