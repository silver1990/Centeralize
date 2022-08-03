
namespace Raybod.SCM.Utility.FirebaseNet.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(string json);
        string Serialize<T>(T value);
    }
}
