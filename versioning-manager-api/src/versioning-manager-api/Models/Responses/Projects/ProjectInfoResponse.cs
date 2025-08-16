using versioning_manager_api.DevDatabase;

namespace versioning_manager_api.Models.Responses.Projects;

/// <summary>
///     The project info api response.
/// </summary>
public class ProjectInfoResponse(int id, string name, string[] availableSources)
{
    /// <summary>
    ///     The project id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    ///     The project name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     The available sources.
    /// </summary>
    public string[] AvailableSources { get; } = availableSources;

    /// <summary>
    ///     Creates <see cref="ProjectInfoResponse" /> from <see cref="DbProject" />.
    /// </summary>
    /// <param name="project">The project db entity.</param>
    /// <returns></returns>
    public static ProjectInfoResponse Create(DbProject project)
    {
        return new ProjectInfoResponse(project.Id, project.Name, project.AvailableSources);
    }
}