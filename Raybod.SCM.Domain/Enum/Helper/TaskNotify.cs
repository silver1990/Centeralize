using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Raybod.SCM.Domain.Enum
{
    public enum TaskNotify
    {
        None=0,
        [Description("نهایی سازی ویرایش")]
        [Display(Name = "Revision finalization")]
        AddRevision = 90,
        [Description("اصلاح ویرایش")]
        [Display(Name = "Revision modification")]
        RevisionReject = 102,
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal register")]
        AddTransmittal = 104,
        [Description("پاسخ کامنت")]
        [Display(Name = "Comment reply")]
        ReplyComComment =114,
        [Description("پاسخ NCR")]
        [Display(Name = "NCR reply")]
        ReplyNCRComment =116,
        [Description("پاسخ TQ")]
        [Display(Name = "TQ reply")]
        ReplyTQComment =115,
        [Description("عملیات درجریان")]
        [Display(Name = "Operation in progress")]
        StartOperation = 121,
        [Description("ثبت درخواست خرید")]
        [Display(Name = "Purchase request register")]
        AddPurchaseRequest = 14,
        [Description("ثبت درخواست پیشنهاد")]
        [Display(Name = "RFP register")]
        AddRFP = 19,
        [Description("ثبت پروپوزال فنی")]
        [Display(Name = "Technical proposal register")]
        AddTechProposal = 21,
        [Description("ثبت پروپوزال بازرگانی")]
        [Display(Name = "Commercial proposal register")]
        AddCommercialProposal = 22,
        [Description("ثبت ارزیابی پروپوزال فنی")]
        [Display(Name = "Evaluate technical proposal")]
        SetTechEvaluation = 28,
        [Description("ثبت ارزیابی پروپوزال بازرگانی")]
        [Display(Name = "Evaluate commercial proposal")]
        SetCommercialEvaluation = 29,
        [Description("انتخاب برنده درخواست پیشنهاد")]
        [Display(Name = "RFP winner")]
        SetRFPWinner = 25,
        [Description("ثبت در خواست پیشنهاد پیش فاکتور")]
        [Display(Name = "RFP proforma register")]
        AddRFPProForma = 129,
        [Description("ثبت قرارداد خرید")]
        [Display(Name = "Pr contract register")]
        AddPrContract = 51,
        [Description("ثبت سفارش خرید")]
        [Display(Name = "PO register")]
        AddPOPending = 56,
        [Description("سفارش در جریان")]
        [Display(Name = "PO inprogress")]
        AddPO = 57,
        [Description("بازرسی پکینگ سفارش")]
        [Display(Name = "PO packing QC")]
        AddPackQC = 65,
        [Description("ثبت حمل به گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing transport to source customs")]
        AcceptPackQC = 66,
        [Description("ثبت ترخیص گمرک مبدا")]
        [Display(Name = "PO packing  clearance from source customs")]
        C1Pending = 71,
        [Description("ثبت حمل به گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing transport to destination customs")]
        T2Pending = 74,
        [Description("ثبت ترخیص گمرک مقصد")]
        [Display(Name = "PO packing  clearance from destination customs")]
        C2Pending = 77,
        [Description("ثبت حمل به انبار خریدار پکینگ سفارش")]
        [Display(Name = "PO packing transport to buyer warehouse")]
        T3Pending = 80,
        [Description("ثبت پرداخت")]
        [Display(Name = "Payment register")]
        AddPayment = 89,
        [Description("ثبت رسید انبار")]
        [Display(Name = "Receipt register")]
        AddReceipt = 84,
        [Description("ثبت کنترل کیفیت انبار")]
        [Display(Name = "Receipt QC register")]
        AddReceiptQC = 85,
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddReceiptReject = 86,
        [Description("ثبت خروج از انبار")]
        [Display(Name = "Warehouse dispatch register")]
        AddWarehouseDispatch =147,
        [Description("ثبت فاکتور")]
        [Display(Name = "Invoice register")]
        AddInvoice =87,
    }
}
