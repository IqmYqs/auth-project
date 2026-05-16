using Microsoft.Extensions.DependencyInjection;
using auth_dotnet_api.Repositories;

namespace auth_dotnet_api.Data
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<UserRepository>();

            return services;
        }
    }
}