using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Receipt
{
    public class AddQualityControlReceiptDto : AddQCReceiptDto
    {
        public QCResult QCResult { get; set; }

    }
}
