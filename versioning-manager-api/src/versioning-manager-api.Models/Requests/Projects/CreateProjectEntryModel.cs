using System.ComponentModel.DataAnnotations;
using versioning_manager_api.Routes.StaticStorages;

namespace versioning_manager_api.Models.Requests.Projects;

/// <summary>
///     The project entry creation api model.
/// </summary>
public class CreateProjectEntryModel
{
    /// <summary>
    ///     The project name.
    /// </summary>
    [MaxLength(FieldsLimits.MaxProjectName)]
    [MinLength(FieldsLimits.MinProjectName)]
    public required string ProjectName { get; init; }

    /// <summary>
    ///     The project entry version.
    /// </summary>
    [MaxLength(FieldsLimits.MaxProjectEntryVersion)]
    [MinLength(FieldsLimits.MinProjectEntryVersion)]
    public required string Version { get; init; }

    /// <summary>
    ///     The project entry default actuality state. <br />
    ///     <c>true</c> for actual; <br />
    ///     <c>false</c> for non-actual.
    /// </summary>
    public required bool DefaultActuality { get; init; }
}