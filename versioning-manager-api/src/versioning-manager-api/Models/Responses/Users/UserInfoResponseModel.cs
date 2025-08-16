using versioning_manager_api.DevDatabase;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace versioning_manager_api.Models.Responses.Users;

/// <summary>
///     The user info response api model.
/// </summary>
public class UserInfoResponseModel
{
    /// <summary>
    ///     The username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    ///     The user activity status.
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    ///     The role name.
    /// </summary>
    public string? RoleName { get; init; }

    public static UserInfoResponseModel CreateFromDbEntity(DbUser user)
    {
        return new UserInfoResponseModel
        {
            Username = user.Username,
            IsActive = user.IsActive,
            RoleName = user.Role?.Name
        };
    }
}