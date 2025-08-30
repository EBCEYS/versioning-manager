using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Docker.DotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.Attributes;
using versioning_manager_api.Exceptions;
using versioning_manager_api.Middle.UnitOfWorks.Images;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;
using versioning_manager_api.Routes;
using versioning_manager_api.Routes.StaticStorages;
using versioning_manager_api.SystemObjects;
using static versioning_manager_api.Routes.ControllerRoutes.ProjectV1Routes;

namespace versioning_manager_api.Controllers.V1;

/// <summary>
///     The project controller. Apikey header required.
/// </summary>
/// <response code="403">Forbidden. Request without Apikey header.</response>
[ApiController]
[ApiVersion(ControllerRoutes.ProjectV1Routes.ApiVersion)]
[Route(ControllerRoute)]
[RequireApiKey]
[AllowAnonymous]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> _logger;
    private readonly ImageUnits _unit;

    /// <summary>
    ///     Initiates a new instance of <see cref="ProjectController" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="unit">The image units of work.</param>
    /// <exception cref="ApiKeyRequireException"></exception>
    public ProjectController(ILogger<ProjectController> logger, ImageUnits unit)
    {
        _logger = logger;
        _unit = unit;
    }

    private ApiKeyEntity Requester => HttpContext.Items[ApikeyStorage.ApikeyHeader] as ApiKeyEntity ??
                                      throw new ApiKeyRequireException(ApikeyStorage.ApikeyHeader);

    /// <summary>
    ///     Gets the image file.
    /// </summary>
    /// <param name="id">The image id.</param>
    /// <response code="200">The image tag archive file stream.</response>
    /// <response code="404">Image not found in database or registry.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(DownloadImageRoute)]
    [Produces("application/x-tar")]
    [ProducesResponseType<FileStream>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadImageAsync([Required] [FromQuery] int id)
    {
        var imageStream = await _unit.GetImageFileAsync(id, HttpContext.RequestAborted);
        if (imageStream == null) return NotFoundProblem("Image");

        return File(imageStream, "application/x-tar");
    }

    /// <summary>
    ///     Uploads the image to docker registry.
    /// </summary>
    /// <param name="file">The image file.</param>
    /// <response code="200">Image file uploaded successfully.</response>
    /// <response code="403">Wrong device (requester).</response>
    /// <response code="404">Not found project or image.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(UploadImageRoute)]
    [RequestSizeLimit(1024 * 1024 * 1024)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadImageAsync([Required] IFormFile file)
    {
        var requester = Requester;
        using var scope = _logger.BeginScope("Device {id} try to upload image info", requester.DeviceId);
        var imageStream = file.OpenReadStream();
        imageStream.Seek(0, SeekOrigin.Begin);
        await _unit.UploadImageAsync(imageStream, HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    ///     Uploads the image info.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <response code="200">Image info uploaded successfully.</response>
    /// <response code="403">Wrong device (requester).</response>
    /// <response code="404">Not found project or image.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(PostImageInfoRoute)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadImageInfoAsync([Required] [FromBody] UploadImageInfoModel model)
    {
        var requester = Requester;
        using var scope = _logger.BeginScope("Device {id} try to upload image info", requester.DeviceId);
        try
        {
            var result =
                await _unit.UploadImageInfoAsync(model, requester.DeviceId, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    _logger.LogInformation("Image {tag} info uploaded successfully", model.ImageTag);
                    return Ok();
                case OperationResult.Failure:
                    _logger.LogWarning("Device {id} is not registered!", requester.DeviceId);
                    return Problem("Device not registered!", GetType().Name, StatusCodes.Status403Forbidden);
                case OperationResult.NotFound:
                    _logger.LogWarning("Project {name} not found", model.ProjectName);
                    return NotFoundProblem("Project");
                case OperationResult.Conflict:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (DockerImageNotFoundException ex)
        {
            _logger.LogError(ex, "Docker image {tag} not found", model.ImageTag);
            return NotFoundProblem("Image");
        }
    }

    /// <summary>
    ///     Gets the project info.
    /// </summary>
    /// <param name="name">The project name.</param>
    /// <response code="200">The project info.</response>
    /// <response code="404">Not found actual project.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(GetProjectInfoRoute)]
    [Produces("application/json")]
    [ProducesResponseType<DeviceProjectInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectInfo(
        [Required] [FromRoute] [MaxLength(FieldsLimits.MaxProjectName)]
        string name)
    {
        var info = await _unit.GetProjectInfoAsync(name, HttpContext.RequestAborted);
        return !info.ActualEntries.Any() ? NotFoundProblem("Actual project") : Ok(info);
    }

    /// <summary>
    ///     Gets the project docker-compose file.
    /// </summary>
    /// <param name="id">The project entry id.</param>
    /// <response code="200">Docker-compose.yaml file stream.</response>
    /// <response code="404">Project entry not found.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(GetProjectComposeFileRoute)]
    [Produces("application/x-yaml")]
    [ProducesResponseType<FileStream>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectCompose([Required] [FromRoute] int id)
    {
        using var scope = _logger.BeginScope("Device {id} requested docker-compose file", Requester.DeviceId);
        var result = await _unit.GetProjectDockerComposeAsync(id, HttpContext.RequestAborted);
        if (result == null) return NotFoundProblem("Project entry");

        result.Seek(0, SeekOrigin.Begin);
        return File(result, "application/x-yaml");
    }

    private ObjectResult NotFoundProblem(string name)
    {
        return Problem($"{name} not found!", GetType().Name, 404, $"{name} not found!");
    }
}