using versioning_manager_api.Middle.UnitOfWorks.Devices;
using versioning_manager_api.Middle.UnitOfWorks.Images;
using versioning_manager_api.Middle.UnitOfWorks.Projects;
using versioning_manager_api.Middle.UnitOfWorks.Users;

namespace versioning_manager_api.Middle.UnitOfWorks;

/// <summary>
/// The units service collection extensions.
/// </summary>
public static class UnitsIServiceCollectionExtensions
{
    /// <summary>
    /// Adds unit of works to <paramref name="sc"/> as scoped.
    /// </summary>
    /// <param name="sc">The service collection.</param>
    /// <returns>An instance of <paramref name="sc"/>.</returns>
    public static IServiceCollection AddUnitsOfWork(this IServiceCollection sc)
    {
        return sc
            .AddScoped<UserUnits>()
            .AddScoped<DeviceUnits>()
            .AddScoped<ProjectsUnits>()
            .AddScoped<ImageUnits>();
    }
}