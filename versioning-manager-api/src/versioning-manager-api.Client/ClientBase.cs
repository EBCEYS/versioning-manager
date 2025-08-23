using System.Text.Json;
using Flurl;
using Flurl.Http;
using versioning_manager_api.Client.Exceptions;

namespace versioning_manager_api.Client;

internal abstract class ClientBase(IFlurlClient client, TimeSpan defaultTimeout, JsonSerializerOptions jsonOptions)
{
    private const string AuthorizationHeader = "Authorization";
    private const string AuthorizationSchema = "Bearer";
    private const string UnsuccessStatusCodeExceptionMessage = "Unsuccess status code!";
    private const string IncorrectParsingExceptionMessage = "Incorrect response parsing!";

    private readonly Url baseUrl = client.BaseUrl;

    protected ClientBase() : this(new FlurlClient(), TimeSpan.FromSeconds(10), JsonSerializerOptions.Web)
    {
    }

    /// <summary>
    ///     Posts the GET request.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <typeparam name="TError">The error type for unsuccess response status code.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResponse" /> if requested successfully.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task<TResponse> GetJsonAsync<TResponse, TError>(Action<Url> urlAction,
        Dictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response = await request.AllowAnyHttpStatus().GetAsync(cancellationToken: token);

        return await ProcessResponse<TResponse, TError>(response, token);
    }

    /// <summary>
    ///     Posts the GET request and reads the response content as stream.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError"></typeparam>
    /// <returns>The stream with response content.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     Throws with response content deserialized as
    ///     <typeparamref name="TError" />.
    /// </exception>
    /// <exception cref="VersioningManagerApiException{String}">Throws with string on unexpected exceptions.</exception>
    protected async Task<Stream> GetStreamAsync<TError>(Action<Url> urlAction,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().GetAsync(HttpCompletionOption.ResponseHeadersRead, token);

        if (response.ResponseMessage.IsSuccessStatusCode)
            return await response.ResponseMessage.Content.ReadAsStreamAsync(token);

        if (TryDeserialize(await response.ResponseMessage.Content.ReadAsByteArrayAsync(token),
                out TError? error) && error != null)
            throw new VersioningManagerApiException<TError>(response.StatusCode, error,
                UnsuccessStatusCodeExceptionMessage);

        throw new VersioningManagerApiException<string>(response.StatusCode, await response.GetStringAsync(),
            IncorrectParsingExceptionMessage);
    }

    /// <summary>
    ///     Posts the POST request with json body.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="requestObject">The object to place it in request body.</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns>The new instance of <typeparamref name="TResponse" /> if requested successfully.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task<TResponse> PostJsonAsync<TRequest, TResponse, TError>(Action<Url> urlAction,
        TRequest? requestObject,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().PostJsonAsync(requestObject, cancellationToken: token);

        return await ProcessResponse<TResponse, TError>(response, token);
    }

    /// <summary>
    ///     Posts the POST request with json body.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="requestObject">The object to place it in request body.</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task PostJsonAsync<TRequest, TError>(Action<Url> urlAction,
        TRequest? requestObject,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().PostJsonAsync(requestObject, cancellationToken: token);

        await ProcessResponse<TError>(response, token);
    }

    /// <summary>
    ///     Posts the PUT request with json body.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="requestObject">The object to place it in request body.</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns>The new instance of <typeparamref name="TResponse" /> if requested successfully.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task<TResponse> PutJsonAsync<TRequest, TResponse, TError>(Action<Url> urlAction,
        TRequest requestObject,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().PutJsonAsync(requestObject, cancellationToken: token);

        return await ProcessResponse<TResponse, TError>(response, token);
    }

    /// <summary>
    ///     Posts the PUT request with json body.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <param name="requestObject">The object to place it in request body.</param>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task PutJsonAsync<TRequest, TError>(Action<Url> urlAction,
        TRequest requestObject,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().PutJsonAsync(requestObject, cancellationToken: token);

        await ProcessResponse<TError>(response, token);
    }

    /// <summary>
    ///     Posts the PUT request.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task PutAsync<TError>(Action<Url> urlAction,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().PutAsync(cancellationToken: token);

        await ProcessResponse<TError>(response, token);
    }

    private static Url FormatUri(Action<Url> urlAction, Url url)
    {
        urlAction.Invoke(url);
        return url;
    }

    /// <summary>
    ///     Posts the DELETE request.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <returns>The new instance of <typeparamref name="TResponse" /> if requested successfully.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task<TResponse> DeleteJsonAsync<TResponse, TError>(Action<Url> urlAction,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().DeleteAsync(cancellationToken: token);

        return await ProcessResponse<TResponse, TError>(response, token);
    }

    /// <summary>
    ///     Posts the DELETE request.
    /// </summary>
    /// <param name="urlAction">The action to enhance base url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="VersioningManagerApiException{TError}">The exception with <typeparamref name="TError" /> object.</exception>
    /// <exception cref="VersioningManagerApiException{TError}">
    ///     The exception with <see cref="String" /> reprezentation of
    ///     response content or service message.
    /// </exception>
    protected async Task DeleteAsync<TError>(Action<Url> urlAction,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response =
            await request.AllowAnyHttpStatus().DeleteAsync(cancellationToken: token);

        await ProcessResponse<TError>(response, token);
    }

    protected async Task PostStreamAsync<TError>(Action<Url> urlAction, Stream stream,
        Dictionary<string, object>? headers = null, CancellationToken token = default) where TError : class
    {
        var url = FormatUri(urlAction, baseUrl);
        var request = PrepareRequest(headers, url);

        var response = await request.AllowAnyHttpStatus()
            .PostAsync(new StreamContent(stream), cancellationToken: token);

        await ProcessResponse<TError>(response, token);
    }

    private async Task<TResponse> ProcessResponse<TResponse, TError>(IFlurlResponse response, CancellationToken token)
        where TError : class
    {
        if (response == null) throw new VersioningManagerApiException<string>(-1, "response is null!");

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            if (TryDeserialize(await response.ResponseMessage.Content.ReadAsByteArrayAsync(token), out TError? error) &&
                error != null)
                throw new VersioningManagerApiException<TError>(response.StatusCode, error,
                    UnsuccessStatusCodeExceptionMessage);

            throw new VersioningManagerApiException<TError>(response.StatusCode, UnsuccessStatusCodeExceptionMessage);
        }

        if (TryDeserialize(await response.ResponseMessage.Content.ReadAsByteArrayAsync(token),
                out TResponse? responseContent) && responseContent != null)
            return responseContent;

        throw new VersioningManagerApiException<string>(response.StatusCode,
            await response.GetStringAsync(), IncorrectParsingExceptionMessage);
    }

    private async Task ProcessResponse<TError>(IFlurlResponse response, CancellationToken token)
        where TError : class
    {
        if (response == null) throw new VersioningManagerApiException<string>(-1, "response is null!");

        if (!response.ResponseMessage.IsSuccessStatusCode)
        {
            if (TryDeserialize(await response.ResponseMessage.Content.ReadAsByteArrayAsync(token), out TError? error) &&
                error != null)
                throw new VersioningManagerApiException<TError>(response.StatusCode, error,
                    UnsuccessStatusCodeExceptionMessage);

            throw new VersioningManagerApiException<TError>(response.StatusCode, UnsuccessStatusCodeExceptionMessage);
        }
    }

    private FlurlRequest PrepareRequest(Dictionary<string, object>? headers, Url url)
    {
        FlurlRequest request = new(url)
        {
            Client = client
        };
        headers ??= GetDefaultHeaders();
        foreach (var header in headers)
            request.Headers.Add(header.Key, header.Value.ToString());

        request.Settings.Timeout = defaultTimeout;
        return request;
    }

    private bool TryDeserialize<TResult>(byte[] content, out TResult? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<TResult>(content, jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }

    protected static Dictionary<string, object> GetDefaultHeaders()
    {
        return new Dictionary<string, object>();
    }

    protected static Dictionary<string, object> GetJwtHeaders(string jwt)
    {
        return new Dictionary<string, object>(GetDefaultHeaders())
        {
            { AuthorizationHeader, $"{AuthorizationSchema} {jwt}" }
        };
    }
}