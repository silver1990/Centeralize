namespace Raybod.SCM.DataTransferObject.Document
{
    public class ConfirmationUserWorkFlowDto
    {
        public long ConfirmationWorkFlowUserId { get; set; }

        public int UserId { get; set; }

        public string UserFullName { get; set; }

        public string UserImage { get; set; }

        public int OrderNumber { get; set; }

        public string Note { get; set; }

        public bool IsBallInCourt { get; set; }

        public bool IsAccept { get; set; }

        public long? DateStart { get; set; }

        public long? DateEnd { get; set; }
    }
}
