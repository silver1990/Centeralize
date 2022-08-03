using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Transmittal
{
    public class TransmittalExportExcelDto
    {
        public string TransmittalNumber { get; set; }

        public string DocumentGroupTitle { get; set; }

        public string CompanyReceiver { get; set; }

        public string FullName { get; set; }

        public string DocTitle { get; set; }

        public string DocNumber { get; set; }

        public string ClientDocNumber { get; set; }

        public DocumentClass DocClass { get; set; }

        public string RevisionCode { get; set; }

        public int? PageNumber { get; set; }

        public string PageSize { get; set; }

        public string UserSenderName { get; set; }

        public long? CreateDate { get; set; }
        public string POI { get; set; }

    }
}
