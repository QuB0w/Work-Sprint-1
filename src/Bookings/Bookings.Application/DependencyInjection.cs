using Bookings.Application.Interfaces;
using Bookings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        services.AddHostedService<BookingConfirmationService>();
        return services;
    }
}
