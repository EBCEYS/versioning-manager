using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using versioning_manager_api.DevDatabase;

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
        VmDatabaseContext dbContext = Substitute.For<VmDatabaseContext>();

        DbSet<DbDevice> devices = Devices.BuildMockDbSet();
        dbContext.Devices.Returns(devices);
        DbSet<DbRole> roles = Roles.BuildMockDbSet();
        dbContext.Roles.Returns(roles);
        DbSet<DbUser> users = Users.BuildMockDbSet();
        dbContext.Users.Returns(users);
        DbSet<DbProject> projects = Projects.BuildMockDbSet();
        dbContext.Projects.Returns(projects);
        DbSet<DbProjectEntry> projectEntries = ProjectEntries.BuildMockDbSet();
        dbContext.ProjectEntries.Returns(projectEntries);
        DbSet<DbImageInfo> images = Images.BuildMockDbSet();
        dbContext.Images.Returns(images);

        return dbContext;
    }
}