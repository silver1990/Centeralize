namespace Raybod.SCM.Utility.Notification.ChabokNotification
{
    public class Notification
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public string Sound { get; set; } = "pakzi";

        public string MediaType { get; set; } = "";

        public string MediaUrl { get; set; } = "";

        public bool ContentAvailable { get; set; }

        public bool MutableContent { get; set; }

        public string Category { get; set; } = "";
    }
}
