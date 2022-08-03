using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Raybod.SCM.Domain.Enum
{
    public enum EmailNotify
    {
        None=0,
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal register")]
        Transmittal =1,
        [Description("ثبت کامنت")]
        [Display(Name = "Comment register")]
        Comment =2,
        [Description("ثبت TQ")]
        [Display(Name = "TQ register")]
        TQ =3,
        [Description("ثبت NCR")]
        [Display(Name = "NCR register")]
        NCR =4,
        [Description("تایید ویرایش")]
        [Display(Name = "Revision confirmation")]
        RevisionConfirmation =5,
    }
}
