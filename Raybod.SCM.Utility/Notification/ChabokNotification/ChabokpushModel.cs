using System;
using System.Collections.Generic;

namespace Raybod.SCM.Utility.Notification.ChabokNotification
{
    public class ChabokpushModel
    {
        public Dictionary<string, dynamic> Target { get; set; }

        public int Id { get; set; }

        public string User { get; set; } = "";

        public string UserMobile { get; set; } = "";

        internal string ClientId { get; set; } = "";

        internal string Channel { get; set; } = "default";

        public string Content { get; set; } = "";

        public Notification Notification { get; set; }

        public Dictionary<string, object> Data { get; set; }

        public string TrackId { get; set; } = "";

        public bool InApp { get; set; }

        public bool Live { get; set; }

        public bool UseAsAlert { get; set; }

        public string AlertText { get; set; } = "";

        public DateTime Ttl { get; set; }

        public bool Idr { get; set; }

        public bool Silent { get; set; }

        internal string ContentBinary { get; set; } = "";

        internal string ContentType { get; set; } = "";

        internal string Application { get; set; } = "";


    }
}
