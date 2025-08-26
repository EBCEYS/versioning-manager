using System.Diagnostics;
using Testcontainers.PostgreSql;
using versioning_manager_api.Client;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.IntegrationTests.TestData;

namespace versioning_manager_api.IntegrationTests;

[SetUpFixture]
public class TestsContext
{
    private const string DatabaseName = "versioningmanager";
    private const string DbUser = "postgres";
    private const string DbPassword = "postgres";

    public const string DefaultUsername = "user";
    public const string DefaultPassword = "password";
    public const string DefaultRole = "root";

    private static VersioningManagerWebApplicationFactory _appFactory;
    private static PostgreSqlContainer _postgres;

    internal static AppContext Context { get; private set; }

    [OneTimeSetUp]
    public static async Task Initialize()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        await FilesCreator.Initialize();

        _postgres = new PostgreSqlBuilder().WithDatabase(DatabaseName)
            .WithUsername(DbUser).WithPassword(DbPassword).WithImage("postgres:16.3").WithCleanUp(true).Build();
        await _postgres.StartAsync();
        
        _appFactory =
            new VersioningManagerWebApplicationFactory(_postgres.GetConnectionString());

        await SetUpApplicationAsync();
    }

    internal static async Task SetUpApplicationAsync()
    {
        await _appFactory.InitializeAsync();

        Context = new AppContext(new VersioningManagerApiClientV1(_appFactory.BaseClient), _appFactory.Services);
    }


    [OneTimeTearDown]
    public static async Task Cleanup()
    {
        FilesCreator.Cleanup();
        await _appFactory.TeardownAsync();
        await _appFactory.DisposeAsync();

        await _postgres.DisposeAsync();

        Trace.Flush();
    }
}

internal record AppContext(VersioningManagerApiClientV1 Client, IServiceProvider Services);