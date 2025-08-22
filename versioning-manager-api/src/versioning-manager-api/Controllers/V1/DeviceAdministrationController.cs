using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Extensions;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Middle.UnitOfWorks.Devices;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Devices;
using versioning_manager_api.Models.Responses.Devices;
using versioning_manager_api.Routes;
using versioning_manager_api.Routes.StaticStorages;
using versioning_manager_api.SystemObjects;
using static versioning_manager_api.Routes.ControllerRoutes.DeviceAdministrationV1Routes;

namespace versioning_manager_api.Controllers.V1;

/// <summary>
///     The device administration controller.
/// </summary>
/// <param name="logger"></param>
/// <param name="units"></param>
/// <param name="hasher"></param>
/// <param name="keyGen"></param>
[ApiController]
[ApiVersion(ControllerRoutes.DeviceAdministrationV1Routes.ApiVersion)]
[Route(ControllerRoute)]
public class DeviceAdministrationController(
    ILogger<DeviceAdministrationController> logger,
    DeviceUnits units,
    IHashHelper hasher,
    IApiKeyProcessor keyGen) : ControllerBase
{
    private ObjectResult InternalError()
    {
        return Problem("Internal database error!", GetType().Name, StatusCodes.Status500InternalServerError,
            "Internal database error!");
    }

    private ObjectResult WrongUserNameProblem()
    {
        return Problem("Invalid token! Not found username", GetType().Name, StatusCodes.Status401Unauthorized,
            "Invalid token!");
    }

    private ObjectResult NotFoundProblem(string name)
    {
        return Problem($"{name} not found!", GetType().Name, 404, $"{name} not found!");
    }

    /// <summary>
    ///     Creates a new device if not exists.
    /// </summary>
    /// <param name="model">The create device api model.</param>
    /// <response code="200">Successfully create a new device.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">Not found creator user (you).</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(PostDeviceRoute)]
    [Authorize(Roles = RolesStorage.CreateDeviceRole)]
    [ProducesResponseType<DeviceTokenInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDeviceAsync([Required] [FromBody] CreateDeviceModel model)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope = logger.BeginScope("User {username} try create device with source {source}", username,
            model.Source);

        try
        {
            Guid deviceId = Guid.CreateVersion7();
            string apiKey = keyGen.Generate(deviceId, model.Source, model.ExpiresUtc);
            OperationResult<DbDevice> result = await units.CreateDeviceAsync(deviceId, username, apiKey, model, hasher,
                HttpContext.RequestAborted);
            if (result is { Result: OperationResult.Success, Object: not null })
            {
                logger.LogInformation("New device created successfully!");
                DeviceTokenInfoResponse response = new()
                {
                    DeviceId = deviceId,
                    ApiKey = apiKey,
                    Source = model.Source,
                    Expires = model.ExpiresUtc
                };
                return Ok(response);
            }

            if (result.Result == OperationResult.NotFound)
            {
                logger.LogWarning("User not found on device creation!");
                return NotFoundProblem("User");
            }

            logger.LogError("Unsupported response condition! {result}", result.Result);
            return InternalError();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on creating device!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Refreshes the device.
    /// </summary>
    /// <param name="model">The update device api model.</param>
    /// <response code="200">Successfully refreshed a device.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">Not found device.</response>
    /// <response code="500">Internal error.</response>
    [HttpPut(RefreshDeviceRoute)]
    [Authorize(Roles = RolesStorage.UpdateDeviceRole)]
    [ProducesResponseType<DeviceTokenInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateDeviceAsync([Required] [FromBody] UpdateDeviceModel model)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope =
            logger.BeginScope("User {username} try update device {id}", username, model.DeviceKey);
        try
        {
            string newKey = keyGen.Generate(model.DeviceKey, model.Source, model.ExpiresUtc);
            OperationResult<DbDevice> result =
                await units.UpdateDeviceAsync(model, hasher, newKey, HttpContext.RequestAborted);
            if (result.Result == OperationResult.Success)
            {
                DeviceTokenInfoResponse response = new()
                {
                    ApiKey = newKey,
                    DeviceId = model.DeviceKey,
                    Expires = model.ExpiresUtc,
                    Source = model.Source
                };
                logger.LogInformation("Successfully refresh token for device {id}", model.DeviceKey);
                return Ok(response);
            }

            if (result.Result == OperationResult.NotFound)
            {
                logger.LogWarning("Device {id} not found!", model.DeviceKey);
                return NotFoundProblem("Device");
            }

            logger.LogError("Unsupported response condition! {result}", result.Result);
            return InternalError();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on updating device!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Gets the devices.
    /// </summary>
    /// <param name="id">The device id. Required if <see cref="DeviceSearchType.One" />.</param>
    /// <param name="searchType">The device search type.</param>
    /// <response code="200">The device list. May be empty ;)</response>
    /// <response code="400">
    ///     Invalid params. Returns if <paramref name="searchType" /> is <see cref="DeviceSearchType.One" />
    ///     and <paramref name="id" /> is empty or null.
    /// </response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(GetDevicesRoute)]
    [Authorize(Roles = RolesStorage.ListDeviceRole)]
    [ProducesResponseType<IEnumerable<DeviceInfoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDevicesAsync([FromQuery] Guid? id = null,
        [FromQuery] DeviceSearchType searchType = DeviceSearchType.Active)
    {
        if (searchType == DeviceSearchType.One && id == null)
            return Problem($"If {searchType} is {DeviceSearchType.One} the {nameof(id)} query param should be set!",
                GetType().Name, StatusCodes.Status400BadRequest);

        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope =
            logger.BeginScope("User {username} try get devices {searchType}", username, searchType);
        try
        {
            DbDevice? oneDevice = null;
            if (searchType == DeviceSearchType.One && id != null) oneDevice = await units.GetDeviceInfo(id.Value);

            IEnumerable<DbDevice> devices = searchType switch
            {
                DeviceSearchType.All => await units.GetAllDevicesAsync(HttpContext.RequestAborted),
                DeviceSearchType.Active => await units.GetActiveDevicesAsync(HttpContext.RequestAborted),
                DeviceSearchType.One => oneDevice != null ? [oneDevice] : [],
                _ => []
            };
            IEnumerable<DeviceInfoResponse> response = devices.Select(d => new DeviceInfoResponse
            {
                Id = d.Id,
                ExpiresUtc = d.ExpireUTC,
                IsActive = d.IsActive
            });
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting devices!`");
            return InternalError();
        }
    }

    /// <summary>
    ///     Marks device for deletion.
    /// </summary>
    /// <param name="id">The device id.</param>
    /// <response code="200">Successfully marked device for deletion.</response>
    /// <response code="400">Empty device id.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">Device not found.</response>
    /// <response code="500">Internal error.</response>
    [HttpDelete(DeleteDeviceRoute)]
    [Authorize(Roles = RolesStorage.DeleteDeviceRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDeviceAsync([Required] [FromQuery] Guid id)
    {
        if (id == Guid.Empty) return Problem("Device ID is required!", GetType().Name, StatusCodes.Status400BadRequest);

        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope = logger.BeginScope("User {username} try delete device {id}", username, id);
        try
        {
            OperationResult<object> result = await units.DeleteDeviceAsync(id, HttpContext.RequestAborted);
            return result.Result switch
            {
                OperationResult.Success => Ok(),
                OperationResult.NotFound => NotFoundProblem("Device"),
                _ => throw new ArgumentOutOfRangeException(nameof(result.Result))
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on deleting device!");
            return InternalError();
        }
    }
}