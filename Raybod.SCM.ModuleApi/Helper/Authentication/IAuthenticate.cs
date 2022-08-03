using Microsoft.AspNetCore.Http;
using Raybod.SCM.DataTransferObject;
using Raybod.SCM.DataTransferObject.User;
using Raybod.SCM.ModuleApi.Model;
using System.Threading.Tasks;

namespace Raybod.SCM.ModuleApi.Helper.Authentication
{
    public interface IAuthenticate
    {
        Task<object> IsAuthenticatedAsync(SigningApiDto request,string language);
        Task<object> RefreshTokenAsync(string token, string refreshToken);

    }
}
