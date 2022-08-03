namespace Raybod.SCM.Domain.Enum
{
    //RFPStatusLog
    public enum RFPLogStatus
    {
        /// <summary>
        /// کنسل شده
        /// </summary>
        Canceled = 0,

        /// <summary>
        /// ثبت شده
        /// </summary>
        Register = 1,

        /// <summary>
        /// پیشنهاد فنی
        /// </summary>
        TechnicalProposal = 2,

        /// <summary>
        /// پیشنهاد بازرگانی
        /// </summary>
        CommercialProposal = 3,

        /// <summary>
        /// انتخاب پیشنهاد
        /// </summary>
        RFPSelection = 5,

        /// <summary>
        /// ارزیابی فنی پیشنهادات
        /// </summary>
        TechEvaluation = 6,

        /// <summary>
        /// ارزیابی بازرگانی پیشنهادات
        /// </summary>
        CommercialEvaluation = 7,

        /// <summary>
        /// ارزیابی پیشنهادات
        /// </summary>
        RFPEvaluation = 8,


    }
}
