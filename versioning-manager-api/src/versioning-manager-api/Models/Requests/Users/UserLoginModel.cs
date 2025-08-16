using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace versioning_manager_api.Models.Requests.Users;

/// <summary>
///     The user login api model.
/// </summary>
public readonly struct UserLoginModel
{
    /// <summary>
    ///     The username.
    /// </summary>
    [MaxLength(FieldsLimits.MaxUsernameLength)]
    public required string Username { get; init; }

    /// <summary>
    ///     The password.
    /// </summary>
    [MaxLength(FieldsLimits.MaxPasswordLength)]
    [MinLength(FieldsLimits.MinPasswordLength)]
    public required string Password { get; init; }
}