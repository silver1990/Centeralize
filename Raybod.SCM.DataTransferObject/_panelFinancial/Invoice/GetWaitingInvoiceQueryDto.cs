using Raybod.SCM.Domain.Enum;

namespace Raybod.SCM.DataTransferObject.Invoice
{
    public class GetWaitingInvoiceQueryDto
    {
        public int ProductId { get; set; }

        public long RefrenceId { get; set; }

        public WaitingForInvoiceType WaitingForInvoiceType { get; set; }
    }
}
