using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Raybod.SCM.Services.Core
{
    public static class CheckLicenceMiddleware
    {
        public static IApplicationBuilder UseCheckLicence(this IApplicationBuilder app)
        {
            var serviceScope = app.ApplicationServices.CreateScope();
            return app.UseMiddleware<CheckLicence>(serviceScope.ServiceProvider.GetService<ISecurity>());
        }
    }
}
