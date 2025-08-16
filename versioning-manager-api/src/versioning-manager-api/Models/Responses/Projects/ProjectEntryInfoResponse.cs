using versioning_manager_api.DevDatabase;

namespace versioning_manager_api.Models.Responses.Projects;

/// <summary>
///     The project entry info response.
/// </summary>
public class ProjectEntryInfoResponse(
    int id,
    string projectName,
    string version,
    bool isActual,
    DateTimeOffset lastUpdatedUtc,
    int? imageCount)
{
    /// <summary>
    ///     The project entry id.
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    ///     The project name.
    /// </summary>
    public string ProjectName { get; } = projectName;

    /// <summary>
    ///     The version.
    /// </summary>
    public string Version { get; } = version;

    /// <summary>
    ///     Is project entry actual.
    /// </summary>
    public bool IsActual { get; } = isActual;

    /// <summary>
    ///     Last update UTC datetime.
    /// </summary>
    public DateTimeOffset LastUpdatedUtc { get; } = lastUpdatedUtc;

    /// <summary>
    ///     The image count.
    /// </summary>
    public int? ImageCount { get; } = imageCount;

    /// <summary>
    ///     Creates an instance of <see cref="ProjectInfoResponse" /> from <paramref name="projectName" /> and
    ///     <see cref="DbProjectEntry" />.
    /// </summary>
    /// <param name="projectName">The source project name.</param>
    /// <param name="entry">The database project entry entity.</param>
    /// <returns></returns>
    public static ProjectEntryInfoResponse Create(string projectName, DbProjectEntry entry)
    {
        return new ProjectEntryInfoResponse(entry.Id, projectName, entry.Version, entry.IsActual, entry.LastUpdateUTC,
            entry.Images?.Count);
    }
}