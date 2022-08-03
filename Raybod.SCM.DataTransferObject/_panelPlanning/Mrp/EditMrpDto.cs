using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class EditMrpDto : BaseMrpDto
    {
        public long Id { get; set; }

        //[Display(Name = "شماره MRP")]
        //public string MrpNumber { get; set; }
    }
}
