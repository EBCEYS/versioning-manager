using System.ComponentModel.DataAnnotations;
using versioning_manager_api.Routes.StaticStorages;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace versioning_manager_api.SystemObjects.Options;

public class DefaultUserOptions
{
    [MaxLength(FieldsLimits.MaxUsernameLength)]
    public required string DefaultUsername { get; init; }

    [MaxLength(FieldsLimits.MaxPasswordLength)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string DefaultPassword { get; init; }

    [MaxLength(FieldsLimits.MaxRoleName)]
    [MinLength(1)]
    public required string DefaultRoleName { get; init; }
}