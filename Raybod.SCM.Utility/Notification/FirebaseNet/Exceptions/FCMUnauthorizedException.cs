using System.Net;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Exceptions
{
    public class FcmUnauthorizedException: FcmException
    { 

        public FcmUnauthorizedException():base(HttpStatusCode.Unauthorized, "Unauthorized")
        {
            
        }
    }
}
