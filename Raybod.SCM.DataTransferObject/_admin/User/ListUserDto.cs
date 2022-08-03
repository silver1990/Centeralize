namespace Raybod.SCM.DataTransferObject.User
{
    public class ListUserDto : BaseUserDto
    {
        public int userId { get; set; }
    }

    public class EditCustomerUserResultDto 
    {
        public bool NewUser { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }
}