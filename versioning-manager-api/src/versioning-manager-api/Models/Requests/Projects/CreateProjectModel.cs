using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.Models.Requests.Projects;

/// <summary>
///     The create project api model.
/// </summary>
public class CreateProjectModel
{
    /// <summary>
    ///     The project name.
    /// </summary>
    [MaxLength(FieldsLimits.MaxProjectName)]
    [MinLength(FieldsLimits.MinProjectName)]
    public required string Name { get; init; }

    /// <summary>
    ///     The available sources.
    /// </summary>
    [MaxLength(FieldsLimits.MaxSourceCount)]
    public required IEnumerable<string> AvailableSources { get; init; }
}