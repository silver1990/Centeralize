using Raybod.SCM.DataTransferObject.License;

namespace Raybod.SCM.Services.Core
{
    public interface ISecurity
    {
        LicenceEntities DecryptFile();
        byte[] EncodingFile(LicenceEntities licence);
        string DecryptLicence(string licence,string key);
        string EncryptLicene(string userId,string key);

    }
}
