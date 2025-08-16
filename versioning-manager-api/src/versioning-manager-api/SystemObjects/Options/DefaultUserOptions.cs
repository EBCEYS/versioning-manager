using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.SystemObjects.Options;

public class DefaultUserOptions
{
    [MaxLength(FieldsLimits.MaxUsernameLength)]
    public required string DefaultUsername { get; set; }

    [MaxLength(FieldsLimits.MaxPasswordLength)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string DefaultPassword { get; set; }

    [MaxLength(FieldsLimits.MaxRoleName)]
    [MinLength(1)]
    public required string DefaultRoleName { get; set; }
}