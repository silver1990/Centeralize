using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{ 
    //no db
    public enum PRType
    {
        [Display(Name = "قرارداد")]
        Contract = 1,

        [Display(Name = "برنامه ریزی مواد")]
        Mrp = 2,

    }
    //PR
    public enum PRStatus
    {
        [Display(Name = "رد درخواست")]
        Reject = 0,

        [Display(Name = "صدور درخواست ")]
        Register = 1,

        [Display(Name = "تایید شد")]
        Confirm = 2,

        [Display(Name = "درخواست پیشنهاد فنی ارسال شد")]
        RfpSending = 3,

        [Display(Name = "TBE")]
        TBE = 4,

        [Display(Name = "CBE")]
        CBE = 5,

        [Display(Name = "انتخاب تامین کننده")]
        SupplierSelection = 6,

        [Display(Name = "قرارداد منعقد شد")]
        PRContractSigned = 7,

        [Display(Name = "منتظر تایید")]
        PendingForConfirm = 8,
    }
}
