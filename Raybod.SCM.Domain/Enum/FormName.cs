using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    //ContractFormConfig
    public enum FormName
    {
        [Display(Name = "DocumentRevision")]
        DocumentRevision = 1,

        [Display(Name = "Transmittal")]
        Transmittal = 2,
        
        [Display(Name = "CommunicationComment")]
        CommunicationComment = 3,

        [Display(Name = "CommunicationTQ")]
        CommunicationTQ = 4,

        [Display(Name = "CommunicationNCR")]
        CommunicationNCR = 5,

        [Display(Name = "MRP")]
        MRP = 6,

        [Display(Name = "PR")]
        PR = 7,

        [Display(Name = "RFP")]
        RFP = 8,

        [Display(Name = "PRContract")]
        PRContract = 9,

        [Display(Name = "PO")]
        PO = 10,

        [Display(Name = "Pack")]
        Pack = 11,

        [Display(Name = "Receipt")]
        Receipt = 12,

        [Display(Name = "ReceiptReject")]
        ReceiptReject = 13,

        [Display(Name = "PendingToPayment")]
        PendingToPayment = 14,

        [Display(Name = "Invoice")]
        Invoice = 15,

        [Display(Name = "Payment")]
        Payment = 16,

        [Display(Name = "WarehouseOutput")]
        WarehouseOutput = 17,

        [Display(Name = "WarehouseDespatch")]
        WarehouseDespatch = 18
    }
}
