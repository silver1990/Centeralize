using System.ComponentModel.DataAnnotations;
using Raybod.SCM.DataTransferObject.PRContract;

namespace Raybod.SCM.DataTransferObject.PO
{
    public class BasePOAttachmentDto : BasePAttachmentDto
    {
        public long POId { get; set; }
    }
}