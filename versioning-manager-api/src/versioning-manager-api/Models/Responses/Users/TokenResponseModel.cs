namespace versioning_manager_api.Models;

/// <summary>
///     The token response api model.
/// </summary>
/// <param name="username">The username.</param>
/// <param name="token">The jwt token.</param>
/// <param name="roles"></param>
/// <param name="timeToLive"></param>
public class TokenResponseModel(
    string username,
    string token,
    string sessionId,
    IEnumerable<string>? roles,
    TimeSpan? timeToLive)
{
    /// <summary>
    ///     The username.
    /// </summary>
    public string Username { get; init; } = username;

    /// <summary>
    ///     The JWT token.
    /// </summary>
    public string Token { get; init; } = token;

    /// <summary>
    ///     The available roles.
    /// </summary>
    public IEnumerable<string>? Roles { get; init; } = roles;

    /// <summary>
    ///     The session id.
    /// </summary>
    public string SessionId { get; init; } = sessionId;

    /// <summary>
    ///     The token time to live.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; } = timeToLive;
}