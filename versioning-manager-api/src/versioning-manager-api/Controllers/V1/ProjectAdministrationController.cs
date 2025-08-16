using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Extensions;
using versioning_manager_api.Middle.UnitOfWorks.Projects;
using versioning_manager_api.Models.Requests.Projects;
using versioning_manager_api.Models.Responses.Projects;
using versioning_manager_api.Routes;
using versioning_manager_api.StaticStorages;
using versioning_manager_api.SystemObjects;

namespace versioning_manager_api.Controllers.V1;

/// <summary>
///     The project administration controller.
/// </summary>
/// <param name="logger"></param>
/// <param name="units"></param>
[ApiController]
[ApiVersion(ControllerRoutes.ProjectAdministrationV1Routes.ApiVersion)]
[Route(ControllerRoutes.ProjectAdministrationV1Routes.ControllerRoute)]
public class ProjectAdministrationController(ILogger<ProjectAdministrationController> logger, ProjectsUnits units)
    : ControllerBase
{
    /// <summary>
    ///     Creates a new project.
    /// </summary>
    /// <param name="model">The project creation api model.</param>
    /// <response code="200">Project created successfully.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">Project with such name already exists.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(ControllerRoutes.ProjectAdministrationV1Routes.PostProjectRoute)]
    [Authorize(Roles = RolesStorage.ProjectCreateRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProjectAsync([Required] [FromBody] CreateProjectModel model)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope = logger.BeginScope("User {username} try create project.", username);
        try
        {
            OperationResult result = await units.CreateProjectAsync(username, model.Name, model.AvailableSources,
                HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Project {name} was successfully created.", model.Name);
                    return Ok();
                case OperationResult.NotFound:
                    logger.LogWarning("User {username} not found.", username);
                    return NotFoundProblem("User");
                case OperationResult.Conflict:
                    logger.LogWarning("Project {name} already exists!", model.Name);
                    return Problem($"Project with name {model.Name} already exists.", GetType().Name,
                        StatusCodes.Status409Conflict);
                case OperationResult.Failure:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on creating project!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Creates the project entry.
    /// </summary>
    /// <param name="model">The project entry creation api model.</param>
    /// <response code="200">Project entry created successfully.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">Project not found.</response>
    /// <response code="409">Project entry with such version already exists.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(ControllerRoutes.ProjectAdministrationV1Routes.PostProjectEntryRoute)]
    [Authorize(Roles = RolesStorage.ProjectCreateRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProjectEntryAsync([Required] [FromBody] CreateProjectEntryModel model)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope = logger.BeginScope("User {username} try create project entry.", username);
        try
        {
            OperationResult result = await units.CreateProjectEntryAsync(username, model.ProjectName, model.Version,
                model.DefaultActuality, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Project {name} entry {version} created successfully.", model.ProjectName,
                        model.Version);
                    return Ok();
                case OperationResult.NotFound:
                    logger.LogWarning("Project {name} or user {username} not found.", model.ProjectName, username);
                    return NotFoundProblem("Project");
                case OperationResult.Conflict:
                    logger.LogWarning("Project {name} with version {version} already exists.", model.ProjectName,
                        model.Version);
                    return Problem("Project with name {name} already exists.", GetType().Name,
                        StatusCodes.Status409Conflict);
                case OperationResult.Failure:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on creating project entry!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Gets all projects.
    /// </summary>
    /// <response code="200">List of all projects.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(ControllerRoutes.ProjectAdministrationV1Routes.GetProjectsRoute)]
    [Authorize(Roles = RolesStorage.GetProjectsRole)]
    [ProducesResponseType<IEnumerable<ProjectInfoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProjectsAsync()
    {
        try
        {
            return Ok((await units.GetAllProjectsAsync(HttpContext.RequestAborted)).Select(ProjectInfoResponse.Create));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting all projects!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Gets project entries by <paramref name="searchType" />.
    /// </summary>
    /// <param name="name">The project name.</param>
    /// <param name="searchType">The search type.</param>
    /// <response code="200">List of all project entries. May be empty ;)</response>
    /// <response code="404">Project not found.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(ControllerRoutes.ProjectAdministrationV1Routes.GetProjectEntriesRoute)]
    [Authorize(Roles = RolesStorage.GetProjectsRole)]
    [ProducesResponseType<IEnumerable<ProjectEntryInfoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectEntriesAsync(
        [Required] [FromRoute] [MaxLength(FieldsLimits.MaxProjectName)]
        string name,
        [Required] [FromQuery] ProjectEntrySearchTypes searchType)
    {
        try
        {
            OperationResult<IEnumerable<DbProjectEntry>> result = await units.GetAllProjectEntriesAsync(name,
                searchType == ProjectEntrySearchTypes.Actual, HttpContext.RequestAborted);
            if (result is { Result: OperationResult.Success, Object: not null })
                return Ok(result.Object.Select(p => ProjectEntryInfoResponse.Create(name, p)));

            if (result.Result == OperationResult.NotFound) return NotFoundProblem("Project");

            logger.LogError("UNSUPPORTED RESULT TYPE {result}", result.Result);
            throw new ArgumentOutOfRangeException(nameof(result.Result));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting project entries!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Changes project entry actuality.
    /// </summary>
    /// <param name="id">The project entry id.</param>
    /// <param name="newStatus">
    ///     The new actuality status. <br />
    ///     <c>true</c> - actual. <br />
    ///     <c>false</c> - non-actual.
    /// </param>
    /// <response code="200">Successfully changed project entry actuality.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="404">Project entry not found.</response>
    /// <response code="500">Internal error.</response>
    [HttpPut(ControllerRoutes.ProjectAdministrationV1Routes.ChangeProjectEntryActualityRoute)]
    [Authorize(Roles = RolesStorage.ProjectUpdateRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeProjectEntryActuality([Required] [FromRoute] int id,
        [Required] [FromQuery] bool newStatus)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope =
            logger.BeginScope("User {username} try change project entry actuality to {newStatus}.", username,
                newStatus);
        try
        {
            OperationResult result =
                await units.ChangeProjectEntryActualityAsync(id, newStatus, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Successfully change actuality.");
                    return Ok();
                case OperationResult.NotFound:
                    logger.LogInformation("Not found project entry with id {id}", id);
                    return NotFoundProblem("Project entry");
                case OperationResult.Failure:
                case OperationResult.Conflict:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on changing project entry actuality!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Gets all project entry images.
    /// </summary>
    /// <param name="id">The project entry id.</param>
    /// <response code="200">List of images.</response>
    /// <response code="500">Internal error.</response>
    [HttpGet(ControllerRoutes.ProjectAdministrationV1Routes.GetProjectEntryImagesRoute)]
    [Authorize(Roles = RolesStorage.GetProjectsRole)]
    [ProducesResponseType<IEnumerable<ImageInfoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProjectImagesAsync([Required] [FromRoute] int id)
    {
        try
        {
            IEnumerable<DbImageInfo> result = await units.GetImageInfosAsync(id, HttpContext.RequestAborted);
            return Ok(result.Select(ImageInfoResponse.Create));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on getting project images!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Copies images to new project entry.
    /// </summary>
    /// <param name="id">The project entry id.</param>
    /// <param name="images">
    ///     The list of images id.<br />
    ///     Min length - 1; <br />
    ///     Max length - 100.
    /// </param>
    /// <response code="200">Successfully copied images to new project entry.</response>
    /// <response code="400">Incorrect images.</response>
    /// <response code="401">Wrong JWT.</response>
    /// <response code="500">Internal error.</response>
    [HttpPost(ControllerRoutes.ProjectAdministrationV1Routes.MigrateImagesToAnotherProjectRoute)]
    [Authorize(Roles = RolesStorage.ProjectUpdateRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CopyImagesToProjectAsync([Required] [FromRoute] int id,
        [Required] [FromBody] [MinLength(1)] [MaxLength(100)]
        IEnumerable<int> images)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope = logger.BeginScope("User {username} try copy images to project {id}.", username, id);
        try
        {
            int[] imagesArray = images as int[] ?? images.ToArray();
            OperationResult result =
                await units.CopyImagesToNewProjectEntry(imagesArray.ToArray(), id, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Successfully copied images.");
                    return Ok();
                case OperationResult.NotFound:
                    logger.LogInformation("Not found project entry with id {id}", id);
                    return NotFoundProblem("Project entry");
                case OperationResult.Failure:
                    logger.LogInformation("Incorrect images {images}", string.Join(", ", imagesArray));
                    return Problem("Incorrect images", GetType().Name, StatusCodes.Status400BadRequest);
                case OperationResult.Conflict:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error on copy images to project entry!");
            return InternalError();
        }
    }

    /// <summary>
    ///     Changes the image activity.
    /// </summary>
    /// <param name="id">The image id.</param>
    /// <param name="newState">The new state.</param>
    /// <response code="200">Successfully changed image activity.</response>
    /// <response code="401">Wrong username.</response>
    /// <response code="404">Not found image.</response>
    /// <response code="500">Internal error.</response>
    [HttpPut(ControllerRoutes.ProjectAdministrationV1Routes.ChangeImageActualityRoute)]
    [Authorize(Roles = RolesStorage.ProjectUpdateRole)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeImageActivityAsync([Required] [FromRoute] int id,
        [Required] [FromQuery] bool newState)
    {
        string? username = User.GetUserName();
        if (username == null) return WrongUserNameProblem();

        using IDisposable? scope =
            logger.BeginScope("User {username} try change image activity to project {id}.", username, id);
        try
        {
            OperationResult result = await units.ChangeImageActivityAsync(id, newState, HttpContext.RequestAborted);
            switch (result)
            {
                case OperationResult.Success:
                    logger.LogInformation("Successfully changed image activity.");
                    return Ok();
                case OperationResult.NotFound:
                    logger.LogWarning("Image {id} not found!", id);
                    return NotFoundProblem("Image");
                case OperationResult.Failure:
                case OperationResult.Conflict:
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on changing project image activity!");
            return InternalError();
        }
    }

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
}

/// <summary>
///     The project entry search types.
/// </summary>
public enum ProjectEntrySearchTypes
{
    /// <summary>
    ///     All.
    /// </summary>
    All,

    /// <summary>
    ///     Actual only.
    /// </summary>
    Actual
}