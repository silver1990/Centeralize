
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Raybod.SCM.Domain.Enum
{
    public enum NotifEvent
    {
        /// <summary>
        /// هیچ
        /// </summary>
        None = 0,
        /// <summary>
        /// ثبت قرارداد فروش
        /// </summary>
        [Description("ثبت پروژه")]
        [Display(Name = "Project register")]
        AddContract = 1,

        /// <summary>
        /// ویرایش قرارداد
        /// </summary>
        [Description("ویرایش پروژه")]
        [Display(Name = "Project edit")]
        EditContract = 2,

        /// <summary>
        /// افزودن فایل پیوست
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        DeleteContract = 3,

        /// <summary>
        /// ثبت مدرک مهندسی
        /// </summary>
        [Description("ثبت مدرک")]
        [Display(Name = "Document register")]
        AddDocument = 4,

        /// <summary>
        /// ویرایش مدرک مهندسی
        /// </summary>
        [Description("ویرایش مدرک")]
        [Display(Name = "Document edit")]
        EditDocument = 5,

        /// <summary>
        /// غیر فعاغل کردن مدرک
        /// </summary>
        [Description("غیر فعال کردن مدرک")]
        [Display(Name = "Document deactive")]
        DeActiveDocument = 6,

        /// <summary>
        /// فعال کردن مدرک
        /// </summary>
        [Description("فعال کردن مدرک")]
        [Display(Name = "Document active")]
        ActiveDocument = 7,

        /// <summary>
        /// ثبت درخت محصول
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddBOM = 8,

        /// <summary>
        /// ویرایش درخت محصول
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        EditBOM = 9,

        /// <summary>
        /// حذف Bom
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        DeleteBOM = 10,

        /// <summary>
        /// ثبت برنامه ریزی
        /// </summary>
        [Description("ثبت برنامه تامین")]
        [Display(Name = "MRP Register")]
        AddMRP = 11,

        /// <summary>
        /// ویرایش برنامه ریزی
        /// </summary>
        [Description("ویرایش برنامه تامین")]
        [Display(Name = "MRP Edit")]
        EditMRP = 12,

        /// <summary>
        /// حذف برنامه ریزی
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        DeleteMRP = 13,

        /// <summary>
        /// ثبت درخواست خرید
        /// </summary>
        [Description("ثبت درخواست خرید")]
        [Display(Name = "Purchase request Register")]
        AddPurchaseRequest = 14,

        /// <summary>
        /// ویرایش درخواست خرید
        /// </summary>
        [Description("ویرایش درخواست خرید")]
        [Display(Name = "Purchase request edit")]
        EditPurchaseRequest = 15,

        /// <summary>
        /// تایید درخوایت خرید
        /// </summary>
        [Description("تایید درخواست خرید")]
        [Display(Name = "Purchase request confirm")]
        ConfirmPurchaseRequest = 16,

        /// <summary>
        /// رد درخواست خرید
        /// </summary>
        [Description("عدم تایید درخواست خرید")]
        [Display(Name = "Purchase request reject")]
        RejectPurchaseRequest = 17,

        /// <summary>
        /// حذف درخواست خرید
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        DeletePurchaseRequest = 18,

        /// <summary>
        /// ثبت RFP
        /// </summary>
        [Description("ثبت درخواست پیشنهاد")]
        [Display(Name = "RFP register")]
        AddRFP = 19,

        /// <summary>
        /// ویرایش RFP
        /// </summary>
        [Description("ویرایش درخواست پیشنهاد")]
        [Display(Name = "RFP edit")]
        EditRFP = 20,

        /// <summary>
        /// ثبت پیشنهاد فنی
        /// </summary>
        [Description("ثبت پروپوزال فنی")]
        [Display(Name = "Technical proposal register")]
        AddTechProposal = 21,

        /// <summary>
        /// ثبت پیشنهاد بازرگانی
        /// </summary>
        [Description("ثبت پروپوزال بازرگانی")]
        [Display(Name = "Commercial proposal register")]
        AddCommercialProposal = 22,

        /// <summary>
        /// انتخاب برنده RFP
        /// </summary>
        [Description("انتخاب برنده درخواست پیشنهاد")]
        [Display(Name = "RFP winner")]
        SetRFPWinner = 25,

        /// <summary>
        /// ثبت کامنت فنی RFP
        /// </summary>
        [Description("پرسش و پاسخ پروپوزال فنی")]
        [Display(Name = "RFP teachnical proposal comment-reply")]
        AddTechRFPComment = 26,

        /// <summary>
        /// ثبت کامنت بازرگانی فنی
        /// </summary>
        [Description("پرسش و پاسخ پروپوزال بازرگانی")]
        [Display(Name = "RFP commercial proposal comment-reply")]
        AddCommercialRFPComment = 27,

        /// <summary>
        /// افزودن ارزیابی فنی
        /// </summary>
        [Description("ثبت ارزیابی پروپوزال فنی")]
        [Display(Name = "Evaluate technical proposal")]
        SetTechEvaluation = 28,

        /// <summary>
        /// ثبت ارزیابی بازرگانی
        /// </summary>
        [Description("ثبت پروپوزال بازرگانی")]
        [Display(Name = "Commercial proposal register")]
        SetCommercialEvaluation = 29,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddTechRFPCommentMention = 30,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddCommercialRFPCommentMention = 31,

        /// <summary>
        /// ثبت قرارداد خرید
        /// </summary>
        [Description("ثبت قرارداد خرید")]
        [Display(Name = "Pr contract register")]
        AddPrContract = 51,

        /// <summary>
        /// تایید قرارداد خرید
        /// </summary>
        [Description("تایید قرارداد خرید")]
        [Display(Name = "Pr contract confirm")]
        ConfirmPRContract = 52,

        /// <summary>
        /// ویرایش قرارداد خرید
        /// </summary>
        [Description("ویرایش قرارداد خرید")]
        [Display(Name = "Pr contract edit")]
        EditPRContract = 53,

        /// <summary>
        /// قرارداد منقضی گردید
        /// </summary>
        //[Description("ثبت ترانسمیتال")]
        //[Display(Name = "Transmittal Register")]
        //ExpirePRContract = 54,

        /// <summary>
        /// تکمیل قرارداد خرید
        /// </summary>
        //[Description("ثبت ترانسمیتال")]
        //[Display(Name = "Transmittal Register")]
        //CompeletePRContract = 55,

        /// <summary>
        /// سفارش خرید درانتظار ثبت
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddPOPending = 56,

        /// <summary>
        /// ثبت سفارش خرید
        /// </summary>
        [Description("ثبت سفارش خرید")]
        [Display(Name = "PO register")]
        AddPO = 57,

        /// <summary>
        /// یادآوری سفارش خرید
        /// </summary>
        //[Description("ثبت ترانسمیتال")]
        //[Display(Name = "Transmittal Register")]
        //PORemined = 58,

        /// <summary>
        /// ثبت فعالیت  در آماده سازی 
        /// </summary>
        [Description("ثبت فعالیت سفارش")]
        [Display(Name = "PO activity register")]
        AddPOActivity = 59,

        /// <summary>
        /// ویرایش اطلاعات اولیه فعالیت 
        /// </summary>
        [Description("ویرایش فعالیت سفارش")]
        [Display(Name = "PO activity edit")]
        EditPOActivity = 60,

        /// <summary>
        /// حذف فعالیت
        /// </summary>
        [Description("حذف فعالیت سفارش")]
        [Display(Name = "PO activity delete")]
        DeletePOActivity = 61,

        /// <summary>
        /// فعالیت به پایان رسید
        /// </summary>
        [Description("انجام فعالیت سفارش")]
        [Display(Name = "PO activity done")]
        POActivityDone = 62,

        /// <summary>
        /// ثبت تایم شیت
        /// </summary>
        [Description("ثبت زمان کاری فعالیت سفارش")]
        [Display(Name = "PO activity timesheet register")]
        AddPOTimeSheet = 63,

        /// <summary>
        /// حذف تایم شیت
        /// </summary>
        [Description("حذف زمان کاری فعالیت سفارش")]
        [Display(Name = "PO activity timesheet delete")]
        DeletePOTimeSheet = 50,

        /// <summary>
        /// ثبت بسته بندی
        /// </summary>
        [Description("ثبت پکینگ سفارش")]
        [Display(Name = "PO packing register")]
        AddPack = 64,

        /// <summary>
        /// ثبت qc
        /// </summary>       
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddPackQC = 65,

        [Description("تایید بازرسی پکینگ سفارش")]
        [Display(Name = "PO packing QC confirm")]
        AcceptPackQC = 66,

        /// <summary>
        /// رد با qc
        /// </summary>
        [Description("عدم تایید بازرسی پکینگ سفارش")]
        [Display(Name = "PO packing QC reject")]
        RejectPackQC = 67,

        /// <summary>
        /// تحویل پک بصورت کامل
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        DeliveredPackCompleted = 68,

        /// <summary>
        /// شروع حمل
        /// </summary>
        [Description("حمل به گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing transport to source customs")]
        T1Inprogress = 69,

        [Description("رسیدن به گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing arrived to source customs")]
        T1Compeleted = 70,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        C1Pending = 71,

        [Description("درجریان ترخیص از گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing clearance from source customs in progress")]
        C1Inprogress = 72,

        [Description(" ترخیص از گمرک مبدا پکینگ سفارش")]
        [Display(Name = "PO packing clearance from source customs")]
        C1ICompeleted = 73,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        T2Pending = 74,

        [Description("حمل به گمرک مقصد پکینگ سفارش")]
        [Display(Name = "PO packing transport to destination customs")]
        T2Inprogress = 75,

        [Description("رسیدن به گمرک مقصد پکینگ سفارش")]
        [Display(Name = "PO packing arrived to destination customs")]
        T2Compeleted = 76,


        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        C2Pending = 77,

        [Description("درجریان ترخیص از گمرک مقصد پکینگ سفارش")]
        [Display(Name = "PO packing clearance from destination customs in progress")]
        C2Inprogress = 78,

        [Description("ترخیص از گمرک مقصد پکینگ سفارش")]
        [Display(Name = "PO packing clearance from destination customs")]
        C2ICompeleted = 79,


        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        T3Pending = 80,

        [Description("حمل به انبار خریدار پکینگ سفارش")]
        [Display(Name = "PO packing transport to buyer warehouse")]
        T3Inprogress = 81,

        [Description("رسیدن به انبار خریدار پکینگ سفارش")]
        [Display(Name = "PO packing arrived to buyer warehouse")]
        T3Compeleted = 82,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        PendingDelivered = 83,

        /// <summary>
        /// ثبت رسید انبار
        /// </summary>
        [Description("ثبت رسید انبار")]
        [Display(Name = "Receipt register")]
        AddReceipt = 84,

        /// <summary>
        /// ثبت کنترل کیفی رسید
        /// </summary>
        [Description("ثبت کنترل کیفیت انبار")]
        [Display(Name = "Receipt QC register")]
        AddReceiptQC = 85,

        /// <summary>
        /// ثبت کنترل کیفی رسید
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddReceiptReject = 86,

        /// <summary>
        /// صدور فاکتور
        /// </summary>        
        [Description("ثبت فاکتور")]
        [Display(Name = "Invoice register")]
        AddInvoice = 87,

        /// <summary>
        /// درخواست پرداخت
        /// </summary>
        [Description("ثبت درخواست پرداخت")]
        [Display(Name = "Pending for payment register")]
        AddPendingForPayment = 88,

        /// <summary>
        ///  پرداخت
        /// </summary>
        [Description("ثبت پرداخت")]
        [Display(Name = "Payment register")]
        AddPayment = 89,

        /// <summary>
        /// ثبت ویرایش
        /// </summary>
        [Description("ثبت رویژن مدرک")]
        [Display(Name = "Document revision register")]
        AddRevision = 90,

        /// <summary>
        /// ثبت فعالیت
        /// </summary>
        [Description("ثبت فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity register")]
        AddRevisionActivity = 91,

        /// <summary>
        /// ویرایش فعالیت
        /// </summary>
        [Description("ویرایش فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity edit")]
        EditRevisionActivity = 92,

        /// <summary>
        /// حذف فعالیت
        /// </summary>
        [Description("حذف فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity delete")]
        DeleteRevisionActivity = 93,

        /// <summary>
        /// فعالیت به پایان رسید
        /// </summary>
        [Description("انجام فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity done")]
        RevisionActivityDone = 94,

        /// <summary>
        /// ثبت تایم شیت
        /// </summary>
        [Description("ثبت زمان کاری فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity timesheet register")]
        AddRevisionTimeSheet = 95,

        /// <summary>
        /// حذف تایم شیت
        /// </summary>
        [Description("حذف زمان کاری فعالیت رویژن مدرک")]
        [Display(Name = "Document revision activity timesheet delete")]
        DeleteRevisionTimeSheet = 96,

        /// <summary>
        /// ثبت کامنت
        /// </summary>
        [Description("پرسش و پاسخ رویژن مدرک")]
        [Display(Name = "Document revision comment-reply ")]
        AddRevisionComment = 97,

        /// <summary>
        /// ثبت پاسخ
        /// </summary>
        [Description("پرسش و پاسخ رویژن مدرک")]
        [Display(Name = "Document revision comment-reply ")]
        ReplayRevisionComment = 98,

        /// <summary>
        /// نهایی سازی مدرک
        /// </summary>
        [Description("ارسال رویژن مدرک جهت تایید")]
        [Display(Name = "Document revision finalization")]
        SendingRevisionConfirmation = 99,

        /// <summary>
        /// 
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        BallInCourtRevisionConfirmation = 100,

        /// <summary>
        /// تایید ویرایش
        /// </summary>
        [Description("تایید رویژن مدرک")]
        [Display(Name = "Document revision confirm")]
        RevisionAccept = 101,

        /// <summary>
        /// رد ویرایش
        /// </summary>
        [Description("عدم تایید رویژن مدرک")]
        [Display(Name = "Document revision reject")]
        RevisionReject = 102,

        /// <summary>
        /// تایید نهایی ویرایش
        /// </summary>
        [Description("تایید نهایی رویژن مدرک")]
        [Display(Name = "Document revision final confirm")]
        RevisionFinalConfirm = 103,

        /// <summary>
        /// ثبت ترانسمیتال
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddTransmittal = 104,

        /// <summary>
        /// اشاره به کاربر
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        RevisionCommentUserMention = 105,

        /// <summary>
        /// 
        /// </summary>
        [Description("غیر فعال کردن رویژن مدرک")]
        [Display(Name = "Document revision deactive")]
        RevisionDeActive = 106,

        /// <summary>
        ///  نهایی
        /// </summary>
        [Description("نهایی سازی رویژن مدرک")]
        [Display(Name = "Document revision finalization")]
        RevisionConfirm = 107,

        /// <summary>
        /// ثبت کامنت
        /// </summary>
        [Description("ثبت کامنت")]
        [Display(Name = "Comment register")]
        AddComComment = 108,

        /// <summary>
        /// ثبت TQ
        /// </summary>
        [Description("ثبت TQ")]
        [Display(Name = "TQ register")]
        AddTQ = 109,

        /// <summary>
        /// ثبت NCR
        /// </summary>
        [Description("ثبت NCR")]
        [Display(Name = "NCR register")]
        AddNCR = 110,

        /// <summary>
        /// ثبت پرسش و پاسخ کامنت
        /// </summary>
        [Description(" پرسش و پاسخ کامنت")]
        [Display(Name = "Comment comment-reply")]
        AddComTeamComment = 111,

        /// <summary>
        /// ثبت پرسش و پاسخ TQ
        /// </summary>
        [Description(" پرسش و پاسخ TQ")]
        [Display(Name = "TQ commnet-reply")]
        AddTQTeamComment = 112,

        /// <summary>
        /// ثبت پرسش و پاسخ NCR
        /// </summary>
        [Description("پرسش و پاسخ NCR")]
        [Display(Name = "NCR comment-reply")]
        AddNCRTeamComment = 113,

        /// <summary>
        /// پاسخ کامنت
        /// </summary>
        [Description("ثبت پاسخ کامنت")]
        [Display(Name = "Comment reply register")]
        ReplyComComment = 114,

        /// <summary>
        /// پاسخ TQ
        /// </summary>
        [Description("ثبت پاسخ TQ")]
        [Display(Name = "TQ reply register")]
        ReplyTQComment = 115,

        /// <summary>
        /// پاسخ NCR
        /// </summary>
        [Description("ثبت پاسخ NCR")]
        [Display(Name = "NCR reply register")]
        ReplyNCRComment = 116,

        /// <summary>
        /// 
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionComTeamComment = 117,

        /// <summary>
        /// ثبت پرسش و پاسخ
        /// </summary>
        [Description("پرسش و پاسخ در سفارش خرید")]
        [Display(Name = "PO comment-reply")]
        AddPOComment = 118,

        /// <summary>
        /// 
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionPOComment = 119,
        /// <summary>
        /// ثبت عملیات
        /// </summary>
        [Description("ثبت فعالیت در مدیریت اجرا")]
        [Display(Name = "Operation register")]
        AddOperation = 120,
        /// <summary>
        /// شروع عملیات
        /// </summary>
        [Description("شروع عملیات در مدیریت اجرا")]
        [Display(Name = "Operation start")]
        StartOperation = 121,
        /// <summary>
        /// ثبت فعالیت برای عملیات اجرا
        /// </summary>
        [Description("ثبت فعالیت عملیات در مدیریت اجرا")]
        [Display(Name = "Operation activity register")]
        AddOperationActivity = 122,
        /// <summary>
        /// کامنت در عملیات
        /// </summary>
        [Description("پرسش و پاسخ عملیات")]
        [Display(Name = "Operation comment-reply")]
        AddCommentInOperation = 123,
        /// <summary>
        /// پاسخ کامنت در عملیات
        /// </summary>
        [Description("پرسش و پاسخ عملیات")]
        [Display(Name = "Operation comment-reply")]
        CommentReplyInOperation = 124,
        /// <summary>
        /// منشن در عملیات
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionOperationComment = 125,
        /// <summary>
        /// نهایی سازی عملیات
        /// </summary>
        [Description("نهایی سازی عملیات در مدیریت اجرا")]
        [Display(Name = "Operation finalization")]
        OperationFinalization = 126,

        /// <summary>
        /// انجام فعالیت
        /// </summary>
        [Description("انجام فعالیت عملیات در مدیریت اجرا")]
        [Display(Name = "Operation activity done")]
        OperationActivityDone = 127,

        /// <summary>
        /// در انتظار ثبت کامنت
        /// </summary>
        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        TransmittalPendingForComment = 128,

        [Description("ثبت در خواست پیشنهاد پیش فاکتور")]
        [Display(Name = "RFP proforma register")]
        AddRFPProForma = 129,

        [Description("پرسش و پاسخ درخواست پیشنهاد پیش فاکتور")]
        [Display(Name = "RFP proforma comment-reply")]
        AddFPProFormaComment = 130,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddRFPProFormaCommentMention = 131,

        [Description("عدم تایید قرارداد خرید")]
        [Display(Name = "Pr contract reject")]
        RejectPRContract = 132,

        [Description("ثبت بازرسی ساخت سفارش ")]
        [Display(Name = "PO inspection register")]
        AddPOInspection =133,

        [Description("تایید بازرسی ساخت سفارش")]
        [Display(Name = "PO inspection pass")]
        POInspectionPass =134,

        [Description("عدم تایید بازرسی ساخت سفارش")]
        [Display(Name = "PO inspection failed")]
        POInspectionFailed =135,

        [Description("ثبت مدارک تامین کننده سفارش")]
        [Display(Name = "PO supplier document register")]
        AddPOSupplierDocument =137,

        [Description("پرسش و  پاسخ بازرسی ساخت سفارش")]
        [Display(Name = "PO inspection comment-reply")]
        AddPOInspectionComment = 138,

        [Description("پرسش و  پاسخ مدارک تامین کننده سفارش")]
        [Display(Name = "PO supplierdocument comment-reply")]
        AddPOSupplierDocumentComment = 139,

        [Description("پرسش و پاسخ مالی سفارش")]
        [Display(Name = "PO financial comment-reply")]
        AddPOFinancialComment = 140,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionPOInspectionComment = 141,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionPOSupplierDocumentComment =142,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        MentionPOFinancialComment = 143,

        [Description("ویرایش مدارک تامین کننده سفارش")]
        [Display(Name = "PO supplier document edit")]
        EditPOSupplierDocument = 144,

        [Description("ثبت درخواست خروج از انبار")]
        [Display(Name = "Warehouse output request register")]
        AddWarehouseOutputRequest =145,

        [Description("تایید درخواست خروج از انبار")]
        [Display(Name = "Warehouse output request confirm")]
        ConfirmWarehouseOutputRequest =146,

        [Description("ثبت خروج از انبار")]
        [Display(Name = "Warehouse dispatch register")]
        AddWarehouseDispatch =147,

        [Description("تایید پرداخت سفارش")]
        [Display(Name = "Payment confirm")]
        ConfirmPayment = 148,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        ReplyComTeamComment = 149,


        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        ReplyTQTeamComment = 150,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        ReplyNCRTeamComment = 151,

        [Description("پرسش و پاسخ کامنت")]
        [Display(Name = "Comment comment-reply")]
        AddComTeamCommentReply = 152,

        [Description("پرسش و پاسخ TQ")]
        [Display(Name = "TQ commnet-reply")]
        AddTQTeamCommentReply = 153,

        [Description("پرسش و پاسخ NCR")]
        [Display(Name = "NCR comment-reply")]
        AddNCRTeamCommentReply = 154,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        AddFileDriveComment = 155,

        [Description("ثبت ترانسمیتال")]
        [Display(Name = "Transmittal Register")]
        ReplyFileDriveComment = 156,
    }
}
