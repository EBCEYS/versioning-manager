using versioning_manager_api.Client.Exceptions;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Responses.Devices;

namespace versioning_manager_api.Client.Interfaces;

/// <summary>
///     The <see cref="IDeviceAdministrationClientV1" /> interface.
/// </summary>
public interface IDeviceAdministrationClientV1
{
    /// <summary>
    ///     Creates the device.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="DeviceTokenInfoResponse" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<DeviceTokenInfoResponse> CreateDeviceAsync(CreateDeviceModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Refreshes the device.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="DeviceTokenInfoResponse" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<DeviceTokenInfoResponse> RefreshDeviceAsync(UpdateDeviceModel request, string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets all devices.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="IReadOnlyCollection{T}" /> of <see cref="DeviceTokenInfoResponse" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<DeviceInfoResponse>> GetAllDevicesAsync(string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets all active the device.
    /// </summary>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="IReadOnlyCollection{T}" /> of <see cref="DeviceTokenInfoResponse" />.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<IReadOnlyCollection<DeviceInfoResponse>> GetActiveDevicesAsync(string jwt,
        CancellationToken token = default);

    /// <summary>
    ///     Gets the specified device.
    /// </summary>
    /// <param name="id">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The new instance of <see cref="DeviceInfoResponse" /> if device exists; otherwise null.</returns>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task<DeviceInfoResponse?> GetDeviceAsync(Guid id, string jwt, CancellationToken token = default);

    /// <summary>
    ///     Deletes the device.
    /// </summary>
    /// <param name="id">The request.</param>
    /// <param name="jwt">The jwt.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="VersioningManagerApiException{TError}">The server response error.</exception>
    /// <exception cref="VersioningManagerApiException{String}">The internal service error.</exception>
    Task DeleteDeviceAsync(Guid id, string jwt, CancellationToken token = default);
}