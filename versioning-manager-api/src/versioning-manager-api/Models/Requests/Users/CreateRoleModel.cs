using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.Models.Requests.Users;

/// <summary>
/// The create role api model.
/// </summary>
public readonly struct CreateRoleModel
{
    /// <summary>
    /// The role name.
    /// </summary>
    [MaxLength(FieldsLimits.MaxRoleName)]
    public string Name { get; init; }
    /// <summary>
    /// The system roles list.
    /// </summary>
    [MaxLength(RolesStorage.Count)]
    public string[] Roles { get; init; }
}