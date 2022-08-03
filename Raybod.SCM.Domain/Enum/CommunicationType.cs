using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    // DocumentCommunication and DocumnentTQNCR
    public enum CommunicationType
    {
        [Display(Name ="comment")]
        Comment = 1,
        [Display(Name = "TQ")]
        TQ = 2,
        [Display(Name = "NCR")]
        NCR = 3
    }
}
