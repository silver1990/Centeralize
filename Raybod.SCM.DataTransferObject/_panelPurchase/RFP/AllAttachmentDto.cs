using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.RFP
{
    public class AllAttachmentDto
    {
        public long? Id { get; set; }

        public long? RFPId { get; set; }

        public long? ProductId { get; set; }

        public long? RFPSupplierStateId { get; set; }

        public RFPStatus RFPSupplierStatus { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string FileType { get; set; }

        public string FileSrc { get; set; }

    }
}
