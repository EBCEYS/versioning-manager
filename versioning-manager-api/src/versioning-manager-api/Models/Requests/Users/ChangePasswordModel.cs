using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace versioning_manager_api.Models.Requests.Users;

/// <summary>
///     The change password api model.
/// </summary>
public class ChangePasswordModel
{
    /// <summary>
    ///     The current password.
    /// </summary>
    [MaxLength(FieldsLimits.MaxPasswordLength)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string CurrentPassword { get; init; }

    /// <summary>
    ///     The new password.
    /// </summary>
    [MaxLength(FieldsLimits.MaxPasswordLength)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string NewPassword { get; init; }
}