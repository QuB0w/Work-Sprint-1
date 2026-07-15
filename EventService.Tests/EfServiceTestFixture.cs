using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sprint_1_WebAPI.DataAccess;

namespace EventServiceUnitTests;

public sealed class EfServiceTestFixture : IDisposable
{
    public EfServiceTestFixture()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
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
