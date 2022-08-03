namespace Raybod.SCM.Services.Utilitys.MailService
{
    public class EmailConfiguration
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public bool Security { get; set; }

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }
    }

}
