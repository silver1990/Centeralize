namespace Raybod.SCM.DataTransferObject.PO
{
    public class EditPOSubjectDto : AddPOPendingSubjectDto
    {
        public long POSubjectId { get; set; }
        public decimal RemainedStock { get; set; }
    }
}
