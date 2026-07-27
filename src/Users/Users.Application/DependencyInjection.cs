using Microsoft.Extensions.DependencyInjection;
using Users.Application.Interfaces;
using Users.Application.Services;

namespace Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
