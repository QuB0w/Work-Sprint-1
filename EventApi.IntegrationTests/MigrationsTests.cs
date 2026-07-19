using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sprint_1_WebAPI.DataAccess;

namespace EventApi.IntegrationTests;

[Collection("PostgreSql")]
public sealed class MigrationsTests : IDisposable
{
    private readonly PostgreSqlFixture _fixture;
    private ServiceProvider? _serviceProvider;

    public MigrationsTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task InitialCreate_CreatesEventsAndBookingsTables_WithForeignKey()
    {
        _serviceProvider = await _fixture.CreateServiceProviderAsync();
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        var eventsTableExists = await TableExistsAsync(connection, "events");
        var bookingsTableExists = await TableExistsAsync(connection, "bookings");
        var foreignKeyExists = await ForeignKeyExistsAsync(connection, "bookings", "fk_bookings_events");

        Assert.True(eventsTableExists, "events table should be created by migrations");
        Assert.True(bookingsTableExists, "bookings table should be created by migrations");
        Assert.True(foreignKeyExists, "bookings should have a foreign key to events");
    }

    private static async Task<bool> TableExistsAsync(DbConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @tableName)";
        AddParameter(command, "tableName", tableName);
        var result = await command.ExecuteScalarAsync();
        return result is true;
    }

    private static async Task<bool> ForeignKeyExistsAsync(DbConnection connection, string tableName, string foreignKeyNamePrefix)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT EXISTS (" +
            "SELECT 1 FROM information_schema.table_constraints " +
            "WHERE table_name = @tableName AND constraint_type = 'FOREIGN KEY' AND constraint_name LIKE @constraintName)";
        AddParameter(command, "tableName", tableName);
        AddParameter(command, "constraintName", $"{foreignKeyNamePrefix}%");
        var result = await command.ExecuteScalarAsync();
        return result is true;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
