using EventApi.Application.Interfaces;
using EventApi.Infrastructure.Data;
using EventApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("eventapi_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task<ServiceProvider> CreateServiceProviderAsync()
    {
        var resetOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql($"{ConnectionString};Pooling=false")
            .Options;

        await using var resetContext = new AppDbContext(resetOptions);
        await resetContext.Database.EnsureDeletedAsync();
        await resetContext.Database.MigrateAsync();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(ConnectionString));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services.BuildServiceProvider();
    }
}
