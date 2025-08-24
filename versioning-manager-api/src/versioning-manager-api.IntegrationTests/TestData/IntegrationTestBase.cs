using System.Diagnostics.CodeAnalysis;
using versioning_manager_api.Client;
using versioning_manager_api.DbContext.DevDatabase;

namespace versioning_manager_api.IntegrationTests.TestData;

public abstract class IntegrationTestBase
{
    [NotNull]
    protected static VmDatabaseContext? DbContext { get; private set; }
    [NotNull]
    protected static VersioningManagerApiClientV1? Client { get; private set; }

    protected static async Task ResetAsync()
    {
        await TestsContext.SetUpApplicationAsync();
        var scope = TestsContext.Context.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<VmDatabaseContext>();
        Client = TestsContext.Context.Client;
    }
}