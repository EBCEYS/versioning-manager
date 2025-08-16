namespace versioning_manager_api.Models;

/// <summary>
///     The users search types.
/// </summary>
public enum UsersSearchType
{
    /// <summary>
    ///     Gets all users.
    /// </summary>
    All,

    /// <summary>
    ///     Gets active users only.
    /// </summary>
    ActiveOnly,

    /// <summary>
    ///     Gets one user by username.
    /// </summary>
    One
}