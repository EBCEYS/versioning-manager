namespace versioning_manager_api.Client.Exceptions;

public class VersioningManagerApiException<TError> : Exception where TError : class
{
    public VersioningManagerApiException(int statusCode, string message, Exception? innerException = null) : base(
        message,
        innerException)
    {
        StatusCode = statusCode;
        ErrorInfo = null;
    }

    public VersioningManagerApiException(int statusCode, TError errorInfo, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorInfo = errorInfo;
    }

    public int StatusCode { get; }
    public TError? ErrorInfo { get; }
}