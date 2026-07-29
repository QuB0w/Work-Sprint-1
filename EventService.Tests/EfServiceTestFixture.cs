using EventApi.Application.Interfaces;
using EventApi.Application.Services;
using EventApi.Infrastructure.Data;
using EventApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventServiceUnitTests;

public sealed class EfServiceTestFixture : IDisposable
{
    public EfServiceTestFixture()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}
