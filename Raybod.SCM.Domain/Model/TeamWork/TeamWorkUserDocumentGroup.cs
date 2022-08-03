using System.ComponentModel.DataAnnotations.Schema;

namespace Raybod.SCM.Domain.Model
{
    public class TeamWorkUserDocumentGroup
    {
        public int Id { get; set; }

        public int DocumentGroupId { get; set; }

        public int TeamWorkUserId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "varchar(60)")]
        public string ContractCode { get; set; }

        [ForeignKey(nameof(TeamWorkUserId))]
        public virtual TeamWorkUser TeamWorkUser { get; set; }

        [ForeignKey(nameof(DocumentGroupId))]
        public virtual DocumentGroup DocumentGroup { get; set; }
    }
}
