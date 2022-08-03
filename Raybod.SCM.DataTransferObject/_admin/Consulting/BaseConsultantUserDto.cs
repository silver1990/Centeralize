namespace Raybod.SCM.DataTransferObject.Consultant
{
    public class BaseConsultantUserDto : AddConsultantUserDto
    {
        public int ConsultantUserId { get; set; }
        public int? UserType { get; set; }
    }
}
