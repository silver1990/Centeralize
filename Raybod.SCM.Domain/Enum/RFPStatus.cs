namespace Raybod.SCM.Domain.Enum
{
    //RFP
    public enum RFPStatus
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
        /// ارزیابی پیشنهادات
        /// </summary>
        RFPEvaluation = 4,

        /// <summary>
        /// انتخاب پیشنهاد
        /// </summary>
        RFPSelection = 5

    }
}
