using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using versioning_manager_api.Client;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.IntegrationTests.TestData;

namespace versioning_manager_api.IntegrationTests;

[SetUpFixture]
public static class TestsContext
{
    public const string DatabaseName = "versioningmanager";
    public const string DbUser = "postgres";
    public const string DbPassword = "postgres";

    public const string DefaultUsername = "user";
    public const string DefaultPassword = "password";
    public const string DefaultRole = "root";

    private static VersioningManagerWebApplicationFactory _appFactory;
    private static PostgreSqlContainer _postgres;

    public static VersioningManagerApiClientV1 ServiceClient { get; private set; }
    public static VmDatabaseContext DbContext { get; private set; }

    public static string BaseAddress { get; private set; }

    [OneTimeSetUp]
    public static async Task Initialize()
    {
        await FilesCreator.Initialize();

        _postgres = new PostgreSqlBuilder().WithDatabase(DatabaseName)
            .WithUsername(DbUser).WithPassword(DbPassword).WithImage("postgres:16.3").WithCleanUp(true).Build();
        await _postgres.StartAsync();
        await WaitForDatabase(_postgres.GetConnectionString());

        _appFactory =
            new VersioningManagerWebApplicationFactory(_postgres.GetConnectionString());
        await _appFactory.InitializeAsync();

        var scope = _appFactory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<VmDatabaseContext>();

        ServiceClient = new VersioningManagerApiClientV1(_appFactory.BaseClient);
    }

    [OneTimeTearDown]
    public static async Task Cleanup()
    {
        FilesCreator.Cleanup();
        await _appFactory.TeardownAsync();
        await _appFactory.DisposeAsync();
        await DbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static async Task WaitForDatabase(string connectionString)
    {
        var maxAttempts = 10;
        var delay = 1000;

        for (var i = 0; i < maxAttempts; i++)
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch
            {
                await Task.Delay(delay);
            }

        throw new Exception("Database not ready after waiting");
    }
}