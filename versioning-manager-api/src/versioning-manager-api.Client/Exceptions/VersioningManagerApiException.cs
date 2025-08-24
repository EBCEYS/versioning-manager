using Flurl.Http;
using versioning_manager_api.Client.Extensions;

namespace versioning_manager_api.Client.Exceptions;

/// <summary>
///     The <see cref="VersioningManagerApiException{TError}" /> class.
/// </summary>
/// <typeparam name="TError">The error response content type.</typeparam>
public class VersioningManagerApiException<TError> : Exception where TError : class
{
    /// <summary>
    ///     Initiates the new instance of <see cref="VersioningManagerApiException{TError}" />.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public VersioningManagerApiException(IFlurlResponse? response, string message, Exception? innerException = null) :
        base(
            $"Status code: {response?.StatusCode}, message: {message}",
            innerException)
    {
        StatusCode = response?.StatusCode;
        ErrorInfo = null;
    }

    /// <summary>
    ///     Initiates the new instance of <see cref="VersioningManagerApiException{TError}" />.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="errorInfo"></param>
    /// <param name="message"></param>
    public VersioningManagerApiException(IFlurlResponse? response, TError errorInfo, string message) : base(
        $"Status code: {response?.StatusCode} {message} details: {errorInfo.ToJson()}")
    {
        StatusCode = response?.StatusCode;
        ErrorInfo = errorInfo;
    }

    /// <summary>
    ///     The status code.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    ///     The error info.
    /// </summary>
    public TError? ErrorInfo { get; }
}