using Raybod.SCM.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.DataTransferObject.Mrp
{
    public class ChangeMrpStatusDto
    {    

        [StringLength(300, MinimumLength = 6, ErrorMessage = "حداقل مقدار برای فیلد {2} و حداکثر {1} می باشد.")]
        public string Details { get; set; }

        public MrpStatus MrpStatus { get; set; }
    }
}
