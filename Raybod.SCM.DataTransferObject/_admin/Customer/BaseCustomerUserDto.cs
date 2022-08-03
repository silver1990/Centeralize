namespace Raybod.SCM.DataTransferObject.Customer
{
    public class BaseCustomerUserDto : AddCustomerUserDto
    {
        public int CustomerUserId { get; set; }
        public int? UserType { get; set; }
    }
}
