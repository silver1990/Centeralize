using Newtonsoft.Json;

namespace Raybod.SCM.Utility.Config
{
    public static class SerializerJsonConfigs
    {
        public static JsonSerializerSettings ReferenceLoopHandlingIgnoreJsonConfig()
        {
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return jss;
        }
    }
}
