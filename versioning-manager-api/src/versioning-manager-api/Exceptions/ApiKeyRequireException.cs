namespace versioning_manager_api.Exceptions;

/// <summary>
///     The apikey require exception.
/// </summary>
public class ApiKeyRequireException : Exception
{
    /// <summary>
    ///     Initiates a new instance of <see cref="ApiKeyRequireException" />.
    /// </summary>
    /// <param name="apikeyHeader">The required apikey header.</param>
    /// <param name="innerException">The inner exception. [optional]</param>
    public ApiKeyRequireException(string apikeyHeader, Exception? innerException = null) : base(
        "Apikey header required!", innerException)
    {
        ApiKeyHeader = apikeyHeader;
    }

    /// <summary>
    ///     The required apikey header.
    /// </summary>
    public string ApiKeyHeader { get; }
}