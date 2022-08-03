namespace Raybod.SCM.DataTransferObject.User
{
    public class UserMentionDto
    {
        public int Id { get; set; }

        public string Display { get; set; }

        public string Image { get; set; }
        public string Email { get; set; }
        public int UserType { get; set; }
    }
}
