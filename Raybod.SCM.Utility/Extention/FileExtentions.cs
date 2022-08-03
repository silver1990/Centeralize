using System.Net;

namespace Raybod.SCM.Utility.Extention
{
    public static class FileExtentions
    {

        public static string DownloadAndSaveFile(this string remoteUri,string fileName="")
        {

            // Create a new WebClient instance.
            using (var WebClient = new WebClient())
            {
            
                // Download the Web resource and save it into the current filesystem folder.
                var value = WebClient.DownloadString(remoteUri);
                // Append url.
                var webClient = new WebClient();
                 webClient.DownloadFile(remoteUri,fileName);
                return fileName;

            }
        }
    }
}
