using versioning_manager_api.Client;
using versioning_manager_api.DbContext.DevDatabase;

namespace versioning_manager_api.IntegrationTests.TestData;

public abstract class IntegrationTestBase
{
    protected static VmDatabaseContext DbContext => TestsContext.DbContext;
    protected static VersioningManagerApiClientV1 Client => TestsContext.ServiceClient;
}