namespace versioning_manager_api.StaticStorages;

/// <summary>
/// The fields limit.
/// </summary>
public static class FieldsLimits
{
    /// <summary>
    /// The max username length.
    /// </summary>
    public const int MaxUsernameLength = 20;

    /// <summary>
    /// The max password length.
    /// </summary>
    public const int MaxPasswordLength = 32;

    /// <summary>
    /// The min password length.
    /// </summary>
    public const int MinPasswordLength = 4;

    /// <summary>
    /// The max role name.
    /// </summary>
    public const int MaxRoleName = 20;

    /// <summary>
    /// The max source name.
    /// </summary>
    public const int MaxSourceName = 64;

    /// <summary>
    /// The max source count.
    /// </summary>
    public const int MaxSourceCount = 5;

    /// <summary>
    /// The max project name.
    /// </summary>
    public const int MaxProjectName = 32;

    /// <summary>
    /// The min project name.
    /// </summary>
    public const int MinProjectName = 4;

    /// <summary>
    /// The max project entry version length.
    /// </summary>
    public const int MaxProjectEntryVersion = 16;
    /// <summary>
    /// The min project entry version length.
    /// </summary>
    public const int MinProjectEntryVersion = 4;
}