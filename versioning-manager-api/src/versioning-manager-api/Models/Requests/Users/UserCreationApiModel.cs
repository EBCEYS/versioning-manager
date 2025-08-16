// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel.DataAnnotations;
using versioning_manager_api.StaticStorages;

namespace versioning_manager_api.Models.Requests.Users;

/// <summary>
///     The user creation api model.
/// </summary>
public readonly struct UserCreationApiModel
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

    /// <summary>
    ///     [optional] The role name.
    /// </summary>
    [MaxLength(FieldsLimits.MaxRoleName)]
    public string? Role { get; init; }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
}