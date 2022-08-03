using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Raybod.SCM.Utility.FirebaseNet.Serialization;

namespace Raybod.SCM.Utility.Notification.FirebaseNet.Serialization
{
    internal class JsonNetSerializer : ISerializer
    {

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        static JsonNetSerializer()
        {
            Settings.Converters.Add(new StringEnumConverter());
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, Settings);
        }
    }
}
