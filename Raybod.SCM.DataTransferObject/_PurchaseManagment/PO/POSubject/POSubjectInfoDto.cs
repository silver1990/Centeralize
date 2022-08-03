namespace Raybod.SCM.DataTransferObject.PO
{
    public class POSubjectInfoDto : POSubjectWithListPartDto
    {

        public string MrpCode { get; set; }

        public decimal ReceiptQuantity { get; set; }

        public long DateRequired { get; set; }
    }
}
