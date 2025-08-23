using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Middle.UnitOfWorks.Users;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.Routes.StaticStorages;
using versioning_manager_api.SystemObjects;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api.Middle;

public class AppStartingService(
    IServiceScopeFactory scopeFactory,
    IOptions<DefaultUserOptions> opts,
    IHashHelper hasher) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        await using var db = scope.ServiceProvider.GetRequiredService<VmDatabaseContext>();
        var units = scope.ServiceProvider.GetRequiredService<UserUnits>();

        await db.Database.MigrateAsync(cancellationToken);

        var roleCreateResult = await units.CreateRoleAsync(new CreateRoleModel
        {
            Name = opts.Value.DefaultRoleName,
            Roles = RolesStorage.Roles.ToArray()
        }, cancellationToken);
        if (roleCreateResult.Result == OperationResult.Success)
            await units.CreateUserIfNotExistsAsync(new UserCreationApiModel
            {
                Username = opts.Value.DefaultUsername,
                Password = opts.Value.DefaultPassword,
                Role = opts.Value.DefaultRoleName
            }, hasher, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public static class AppStartingServiceExtensions
{
    public static IServiceCollection AddAppStartingService(this IServiceCollection sc)
    {
        return sc.AddHostedService<AppStartingService>();
    }
}