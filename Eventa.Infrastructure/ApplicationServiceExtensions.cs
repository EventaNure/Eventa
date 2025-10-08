using Eventa.Application.Services;
using Eventa.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eventa.Infrastructure
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddAutoMapper((e) => { }, AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
