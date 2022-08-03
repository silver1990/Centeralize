using System;
using System.Net;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Exceptions
{
    public class FcmException : Exception
    {

        public FcmException(HttpStatusCode statusCode, string message)
            :base(message)
        {
            StatusCode = statusCode;
        }

        public FcmException():base()
        {
        }

        /// <summary>
        /// The HttpStatusCode returned by FCM
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        
    }
}
