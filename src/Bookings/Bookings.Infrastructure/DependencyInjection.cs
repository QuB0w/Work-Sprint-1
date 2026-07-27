using Bookings.Application.Interfaces;
using Bookings.Infrastructure.Data;
using Bookings.Infrastructure.Kafka;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<BookingsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddSingleton<IBookingEventPublisher, KafkaBookingEventPublisher>();

        return services;
    }
}
