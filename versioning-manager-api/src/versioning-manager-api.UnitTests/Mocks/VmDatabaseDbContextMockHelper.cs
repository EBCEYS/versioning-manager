using MockQueryable.NSubstitute;
using NSubstitute;
using versioning_manager_api.DbContext.DevDatabase;

namespace versioning_manager_api.UnitTests.Mocks;

public class VmDatabaseDbContextMockHelper
{
    public List<DbUser> Users { get; } = [];
    public List<DbRole> Roles { get; } = [];
    public List<DbProject> Projects { get; } = [];
    public List<DbProjectEntry> ProjectEntries { get; } = [];
    public List<DbImageInfo> Images { get; } = [];
    public List<DbDevice> Devices { get; } = [];

    public VmDatabaseContext Initialize()
    {
        var dbContext = Substitute.For<VmDatabaseContext>();

        var devices = Devices.BuildMockDbSet();
        dbContext.Devices.Returns(devices);
        var roles = Roles.BuildMockDbSet();
        dbContext.Roles.Returns(roles);
        var users = Users.BuildMockDbSet();
        dbContext.Users.Returns(users);
        var projects = Projects.BuildMockDbSet();
        dbContext.Projects.Returns(projects);
        var projectEntries = ProjectEntries.BuildMockDbSet();
        dbContext.ProjectEntries.Returns(projectEntries);
        var images = Images.BuildMockDbSet();
        dbContext.Images.Returns(images);

        return dbContext;
    }
}