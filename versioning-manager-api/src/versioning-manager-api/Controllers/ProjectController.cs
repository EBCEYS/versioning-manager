using System.ComponentModel.DataAnnotations;
using Docker.DotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using versioning_manager_api.Attributes;
using versioning_manager_api.Exceptions;
using versioning_manager_api.Middle.UnitOfWorks.Images;
using versioning_manager_api.Models.Requests.Images;
using versioning_manager_api.Models.Responses.Images;
using versioning_manager_api.StaticStorages;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Controllers;

/// <summary>
/// The project controller. Apikey header required.
/// </summary>
/// <response code="403">Forbidden. Request without Apikey header.</response>
[ApiController]
[Route("api/[controller]")]
[RequireApiKey]
[AllowAnonymous]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class ProjectController : ControllerBase
{
    private readonly ILogger<ProjectController> logger;
    private readonly ImageUnits unit;

    private ApiKeyEntity Requester => HttpContext.Items[ApikeyStorage.ApikeyHeader] as ApiKeyEntity ?? throw new ApiKeyRequireException(ApikeyStorage.ApikeyHeader);

    /// <summary>
    /// Initiates a new instance of <see cref="ProjectController"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="unit">The image units of work.</param>
    /// <exception cref="ApiKeyRequireException"></exception>
    public ProjectController(ILogger<ProjectController> logger, ImageUnits unit)
    {
        this.logger = logger;
        this.unit = unit;
    }
    /// <summary>
    /// Gets the image file.
    /// </summary>
    /// <param name="id">The image id.</param>
    /// <response code="200">The image tag archive file stream.</response>
    /// <response code="404">Image not found in database or registry.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet("image/file")]
    [ProducesResponseType<FileStream>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadImageAsync([Required][FromQuery] int id)
    {
        try
        {
            Stream? imageStream = await unit.GetImageFileAsync(id, HttpContext.RequestAborted);
            if (imageStream == null)
            {
                return NotFoundProblem("Image");
            }
            return File(imageStream, "application/x-tar");
        }
        catch (DockerImageNotFoundException ex)
        {
            logger.LogError(ex, "Docker image {id} not found", id);
            return NotFoundProblem("Image");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on downloading image!");
            return InternalError();
        }
    }

    /// <summary>
    /// Uploads the image info.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <response code="200">Image info uploaded successfully.</response>
    /// <response code="403">Wrong device (requester).</response>
    /// <response code="404">Not found project or image.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost("image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadImageInfoAsync([Required][FromBody] UploadImageInfoModel model)
    {
        ApiKeyEntity requester = Requester;
        using IDisposable? scope = logger.BeginScope("Device {id} try to upload image info", requester.DeviceId);
        try
        {
            OperationResult result = await unit.UploadImageInfoAsync(model, requester.DeviceId, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Image {tag} info uploaded successfully", model.ImageTag);
                    return Ok();
                case OperationResult.Failure:
                    logger.LogWarning("Device {id} is not registered!", requester.DeviceId);
                    return Problem("Device not registered!", GetType().Name, StatusCodes.Status403Forbidden);
                case OperationResult.NotFound:
                    logger.LogWarning("Project {name} not found", model.ProjectName);
                    return NotFoundProblem("Project");
                case OperationResult.Conflict:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (DockerImageNotFoundException ex)
        {
            logger.LogError(ex, "Docker image {tag} not found", model.ImageTag);
            return NotFoundProblem("Image");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on uploading image info!");
            return InternalError();
        }
    }

    /// <summary>
    /// Gets the project info.
    /// </summary>
    /// <param name="name">The project name.</param>
    /// <response code="200">The project info.</response>
    /// <response code="404">Not found actual project.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet("project/{name}/info")]
    [ProducesResponseType<DeviceProjectInfoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectInfo([Required][FromRoute][MaxLength(FieldsLimits.MaxProjectName)] string name)
    {
        try
        {
            DeviceProjectInfoResponse info = await unit.GetProjectInfoAsync(name, HttpContext.RequestAborted);
            return !info.ActualEntries.Any() ? NotFoundProblem("Actual project") : Ok(info);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting project info!");
            return InternalError();
        }
    }

    /// <summary>
    /// Gets the project docker-compose file.
    /// </summary>
    /// <param name="id">The project entry id.</param>
    /// <response code="200">Docker-compose.yaml file stream.</response>
    /// <response code="404">Project entry not found.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet("project/{id}/compose")]
    [ProducesResponseType<FileStream>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectCompose([Required][FromRoute] int id)
    {
        using IDisposable? scope = logger.BeginScope("Device {id} requested docker-compose file", Requester.DeviceId);
        try
        {
            Stream? result = await unit.GetProjectDockerComposeAsync(id, HttpContext.RequestAborted);
            if (result == null)
            {
                return NotFoundProblem("Project entry");
            }
            result.Seek(0, SeekOrigin.Begin);
            return File(result, "application/x-yaml");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting project compose file!");
            return InternalError();
        }
    }
    
    private ObjectResult InternalError()
    {
        return Problem("Internal database error!", GetType().Name, StatusCodes.Status500InternalServerError,
            "Internal database error!");
    }

    private ObjectResult NotFoundProblem(string name)
    {
        return Problem(detail: $"{name} not found!", instance: GetType().Name, 404, $"{name} not found!");
    }
}