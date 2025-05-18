using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Extensions;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Middle.UnitOfWorks.Users;
using versioning_manager_api.Models;
using versioning_manager_api.Models.Requests.Users;
using versioning_manager_api.Models.Responses.Users;
using versioning_manager_api.StaticStorages;
using versioning_manager_api.SystemObjects;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api.Controllers
{
    /// <summary>
    /// The users' controller.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="units">The units of work.</param>
    /// <param name="hasher">The password hasher.</param>
    /// <param name="cache">The cache.</param>
    /// <param name="jwtOpts">The jwt options.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        ILogger<UsersController> logger,
        UserUnits units,
        IHashHelper hasher,
        IMemoryCache cache,
        IOptions<JwtOptions> jwtOpts) : ControllerBase
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="model">The user creation api model.</param>
        /// <returns></returns>
        /// <response code="200">User created successfully.</response>
        /// <response code="400">Incorrect params.</response>
        /// <response code="409">User already exists.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost("create/user")]
        [Authorize(Roles = RolesStorage.CreateUserRole)]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUserAsync([Required] [FromBody] UserCreationApiModel model)
        {
            if (!model.Validate())
            {
                return Problem("Incorrect params!", GetType().Name, 400);
            }

            using IDisposable? scope = logger.BeginScope("User {creator} try to create new user", User.GetUserName());

            logger.LogDebug("Try create new user {username}", model.Username);
            try
            {
                OperationResult<DbUser> result = await units.CreateUserIfNotExistsAsync(model, hasher);
                switch (result.Result)
                {
                    case OperationResult.Success:
                        logger.LogInformation("User {username} created successfully!", model.Username);
                        return Ok("User created successfully!");
                    case OperationResult.Conflict:
                        logger.LogWarning("User {username} already exists!", model.Username);
                        return Conflict("User already exists!");
                    default:
                        logger.LogError("UNSUPPORTED RESULT {result}!", result.Result);
                        return InternalError();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on creating user!");
                return InternalError();
            }
        }
        
        /// <summary>
        /// Authenticate user.
        /// </summary>
        /// <param name="model">The user login model.</param>
        /// <response code="200">User authenticated successfully.</response>
        /// <response code="401">User not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType<TokenResponseModel>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([Required] [FromBody] UserLoginModel model)
        {
            using IDisposable? scope = logger.BeginScope("Try login user {name}", model.Username);
            logger.LogInformation("Try login user {name}", model.Username);
            try
            {
                OperationResult<DbUser?> user = await units.LoginUserAsync(model, hasher);
                if (user.IsNotFound() || user.Object == null)
                {
                    return NotFoundProblem("User");
                }
                else if (!user.IsSuccess())
                {
                    logger.LogError("UNSUPPORTED RESULT {result}!", user.Result);
                    return InternalError();
                }
                
                string sessionId = Guid.NewGuid().ToString("N");
                string token = GenerateJwt(user.Object.Username, user.Object.Role?.Roles, sessionId);
                TokenResponseModel response = new(user.Object.Username, token, sessionId, user.Object.Role?.Roles, jwtOpts.Value.TokenTimeToLive);
                
                cache.Set(sessionId, response, jwtOpts.Value.TokenTimeToLive);

                logger.LogInformation("Successfully login user {name}", model.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on auth user!");
                return InternalError();
            }
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="model">The role creation model.<br/>
        /// WARNING <see cref="CreateRoleModel.Roles"/> should be same as service roles.</param>
        /// <returns></returns>
        /// <response code="200">Role created successfully.</response>
        /// <response code="400">Incorrect params.</response>
        /// <response code="401">Wrong JWT.</response>
        /// <response code="409">Role already exists.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost("create/role")]
        [Authorize(Roles = RolesStorage.CreateRoleRole)]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<string>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRoleAsync([Required] [FromBody] CreateRoleModel model)
        {
            string? username = User.GetUserName();
            if (username == null)
            {
                return WrongUserNameProblem();
            }
            using IDisposable? scope = logger.BeginScope("User {user} try create role {name}", username, model.Name);
            if (model.Roles.Length == 0 || !model.Roles.All(s => RolesStorage.Roles.Contains(s)))
            {
                logger.LogWarning("Tried to create invalid role with roles: {roles}", string.Join(',', model.Roles));
                return Problem($"Incorrect roles! {nameof(model.Roles)} should exists in system roles.", GetType().Name,
                    400);
            }

            try
            {
                OperationResult<DbRole> creationResult = await units.CreateRoleAsync(model);
                switch (creationResult.Result)
                {
                    case OperationResult.Success:
                        
                        return Ok("Role created successfully!");
                    case OperationResult.Conflict:
                        return Conflict("Role already exists!");
                    default:
                        logger.LogError("UNSUPPORTED RESULT {result}!", creationResult.Result);
                        return InternalError();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on creating new role!");
                return InternalError();
            }
        }
        
        /// <summary>
        /// Changes the user password.
        /// </summary>
        /// <param name="userName">The username.</param>
        /// <param name="model">The change password api model.</param>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">User not found or current password was incorrect.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("change/password")]
        [Authorize(Roles = RolesStorage.ChangePasswordRole)]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePasswordAsync([Required][MaxLength(FieldsLimits.MaxUsernameLength)][FromQuery] string userName, [Required][FromBody] ChangePasswordModel model)
        {
            using IDisposable? scope = logger.BeginScope("Try change password for user {username}", userName);
            try
            {
                OperationResult<object> result = await units.ChangePasswordAsync(userName, model, hasher, HttpContext.RequestAborted);
                switch (result.Result)
                {
                    case OperationResult.Success:
                        logger.LogInformation("Password changed successfully!");
                        return Ok("Password changed successfully!");
                    case OperationResult.Failure:
                        logger.LogInformation("Incorrect current password!");
                        return Problem("Incorrect current password!", GetType().Name, 400);
                    case OperationResult.NotFound:
                        logger.LogInformation("User not found!");
                        return NotFoundProblem("User");
                    default:
                        logger.LogError("UNSUPPORTED RESULT {result}!", result.Result);
                        return InternalError();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on changing password!");
                return InternalError();
            }
        }

        /// <summary>
        /// Changes the current user password.
        /// </summary>
        /// <param name="model">The change password api model.</param>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">User not found or current password was incorrect.</response>
        /// <response code="401">Incorrect JWT.</response>
        /// <response code="500">Internal error.</response>
        [HttpPost("change/password")]
        [Authorize]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeSelfPassword([Required] [FromBody] ChangePasswordModel model)
        {
            string? username = User.GetUserName();
            if (username == null)
            {
                return WrongUserNameProblem();
            }

            logger.LogInformation("Try self change password for user {username}", username);
            try
            {
                return await ChangePasswordAsync(username, model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on self changing password!");
                return InternalError();
            }
        }

        /// <summary>
        /// Gets the system roles list.
        /// </summary>
        /// <response code="200">List of system roles.</response>
        [HttpGet("system/roles")]
        [Authorize(Roles = RolesStorage.GetSystemRolesRole)]
        [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
        public IActionResult GetSystemRoles()
        {
            return Ok(RolesStorage.Roles);
        }

        /// <summary>
        /// Gets users roles.
        /// </summary>
        /// <response code="200">The users roles list.</response>
        /// <response code="500">Internal error.</response>
        [HttpGet("user/roles")]
        [Authorize(Roles = RolesStorage.GetUserRolesRole)]
        [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRolesAsync()
        {
            try
            {
                return Ok(await units.GetAllRolesAsync(HttpContext.RequestAborted));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting user roles!");
                return InternalError();
            }
        }

        /// <summary>
        /// Changes the user role.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="newRole">The new role.</param>
        /// <response code="200">User role changed successfully.</response>
        /// <response code="400">Username or role not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("user/role")]
        [Authorize(Roles = RolesStorage.UpdateUserRoleRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserRoleAsync([Required] [FromQuery] [MaxLength(FieldsLimits.MaxUsernameLength)] string username,
            [Required] [FromQuery] [MaxLength(FieldsLimits.MaxPasswordLength)] string newRole)
        {
            string? updater = User.GetUserName();
            if (updater == null)
            {
                return WrongUserNameProblem();
            }

            using IDisposable? scope = logger.BeginScope("User {updater} try change user {username} role to {role}",
                updater, username, newRole);
            try
            {
                OperationResult result = await units.ChangeUserRoleAsync(username, newRole, HttpContext.RequestAborted);
                switch (result)
                {
                    case OperationResult.Success:
                        return Ok();
                    case OperationResult.NotFound:
                        return NotFoundProblem("User or role");
                    case OperationResult.Failure:
                    case OperationResult.Conflict:
                    default:
                        logger.LogError("Operation result {result} is not supported!", result);
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on updating user role!");
                return InternalError();
            }
        }

        /// <summary>
        /// Deletes the role.
        /// </summary>
        /// <param name="role">The role name.</param>
        /// <response code="200">Role successfully marked for deletion.</response>
        /// <response code="401">Wrong JWT.</response>
        /// <response code="404">Role not found.</response>
        /// <response code="500">Internal error.</response>
        [HttpDelete("role")]
        [Authorize(Roles = RolesStorage.DeleteRoleRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRoleAsync([Required] [FromQuery] [MaxLength(FieldsLimits.MaxRoleName)] string role)
        {
            string? username = User.GetUserName();
            if (username == null)
            {
                return WrongUserNameProblem();
            }

            using IDisposable? scope = logger.BeginScope("User {username} try delete role {role}", username, role);
            try
            {
                OperationResult result = await units.DeleteRoleAsync(role, HttpContext.RequestAborted);
                switch (result)
                {
                    case OperationResult.Success:
                        return Ok();
                    case OperationResult.NotFound:
                        return NotFoundProblem("Role");
                    case OperationResult.Failure:
                    case OperationResult.Conflict:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on deleting role!");
                return InternalError();
            }
        }

        /// <summary>
        /// Updates role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="newRoles">The new roles.</param>
        /// <response code="200">Role successfully updated.</response>
        /// <response code="400">New roles does not match to system roles.</response>
        /// <response code="401">Wrong JWT.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("role")]
        [Authorize(Roles = RolesStorage.UpdateRoleRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRoleAsync([Required] [FromQuery] [MaxLength(FieldsLimits.MaxRoleName)] string role,
            [Required] [FromBody] [MaxLength(RolesStorage.Count)] IEnumerable<string> newRoles)
        {
            string? username = User.GetUserName();
            if (username == null)
            {
                return WrongUserNameProblem();
            }

            using IDisposable? scope = logger.BeginScope("User {username} try to update role {role}", username, role);

            IEnumerable<string> enumerable = newRoles as string[] ?? newRoles.ToArray();
            if (!enumerable.All(s => RolesStorage.Roles.Contains(s)))
            {
                logger.LogWarning("Tried to create invalid role with roles: {roles}", string.Join(',', enumerable));
                return Problem($"Incorrect roles! {nameof(newRoles)} should exists in system roles.", GetType().Name,
                    400);
            }
            
            try
            {
                OperationResult result = await units.UpdateRoleAsync(role, enumerable, HttpContext.RequestAborted);
                switch (result)
                {
                    case OperationResult.Success:
                        return Ok();
                    case OperationResult.NotFound:
                        return NotFoundProblem("Role");
                    case OperationResult.Failure:
                    case OperationResult.Conflict:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on updating role!");
                return InternalError();
            }
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <response code="200">User marked for deletion successfully.</response>
        /// <response code="304">No changes.</response>
        /// <response code="401">Wrong JWT.</response>
        /// <response code="500">Internal error.</response>
        [HttpDelete("user")]
        [Authorize(Roles = RolesStorage.DeleteUserRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAsync([Required][FromQuery] [MaxLength(FieldsLimits.MaxUsernameLength)] string username)
        {
            string? updater = User.GetUserName();
            if (updater == null)
            {
                return WrongUserNameProblem();
            }

            using IDisposable? scope =
                logger.BeginScope("User {updater} try delete user {username}", updater, username);
            try
            {
                OperationResult result =
                    await units.UpdateUserIsActiveAsync(username, false, HttpContext.RequestAborted);
                switch (result)
                {
                    case OperationResult.Success:
                        return Ok();
                    case OperationResult.NotFound:
                        return StatusCode(StatusCodes.Status304NotModified);
                    case OperationResult.Conflict:
                    case OperationResult.Failure:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on deleting user!");
                return InternalError();
            }
        }
        
        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <response code="200">User marked for deletion successfully.</response>
        /// <response code="304">No changes.</response>
        /// <response code="401">Wrong JWT.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("change/user/active")]
        [Authorize(Roles = RolesStorage.DeleteUserRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetUserActiveAsync([Required][FromQuery] [MaxLength(FieldsLimits.MaxUsernameLength)] string username)
        {
            string? updater = User.GetUserName();
            if (updater == null)
            {
                return WrongUserNameProblem();
            }

            using IDisposable? scope =
                logger.BeginScope("User {updater} try delete user {username}", updater, username);
            try
            {
                OperationResult result =
                    await units.UpdateUserIsActiveAsync(username, true, HttpContext.RequestAborted);
                switch (result)
                {
                    case OperationResult.Success:
                        return Ok();
                    case OperationResult.NotFound:
                        return StatusCode(StatusCodes.Status304NotModified);
                    case OperationResult.Conflict:
                    case OperationResult.Failure:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on deleting user!");
                return InternalError();
            }
        }

        private string GenerateJwt(string username, IEnumerable<string>? roles, string sessionId)
        {
            List<Claim> claims =
            [
                new(ClaimTypes.NameIdentifier, username),
                new(ClaimTypes.Sid, sessionId),
            ];
            if (roles != null)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            DateTime? expires = jwtOpts.Value.TokenTimeToLive != null ? DateTime.UtcNow.Add(jwtOpts.Value.TokenTimeToLive.Value) : null;
            SymmetricSecurityKey key = new(Convert.FromBase64String(System.IO.File.ReadAllText(jwtOpts.Value.SecretFilePath)));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken jwt = new
            (
                jwtOpts.Value.Issuer,
                jwtOpts.Value.Audience,
                claims,
                null,
                expires,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        /// <summary>
        /// Gets the users info.
        /// </summary>
        /// <param name="searchType">The search user type.</param>
        /// <param name="username">The username. Should be set if <param name="searchType"></param> is <see cref="UsersSearchType.One"/>.</param>
        /// <response code="200">Users list if exists. May be empty.</response>
        /// <response code="400">Invalid params.</response>
        /// <response code="500">Internal error.</response>
        [HttpGet("user")]
        [Authorize(Roles = RolesStorage.GetUsersRole)]
        [ProducesResponseType<IEnumerable<UserInfoResponseModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersAsync([Required] [FromQuery] UsersSearchType searchType,
            [FromQuery] string? username = null)
        {
            try
            {
                IEnumerable<DbUser> result;
                switch (searchType)
                {
                    case UsersSearchType.All:
                        result = await units.GetAllUsersAsync(HttpContext.RequestAborted);
                        break;
                    case UsersSearchType.ActiveOnly:
                        result = await units.GetActiveUsersAsync(HttpContext.RequestAborted);
                        break;
                    case UsersSearchType.One:
                        if (username == null)
                        {
                            return Problem($"If use {UsersSearchType.One} the {nameof(username)} property should be set!",
                                GetType().Name, 400, "Incorrect params!");
                        }
                        DbUser? user = await units.GetUserAsync(username, HttpContext.RequestAborted);
                        result = user != null ? [user] : [];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(searchType), searchType, null);
                }
                return Ok(result.Select(UserInfoResponseModel.CreateFromDbEntity));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting users info!");
                return InternalError();
            }
        }

        private ObjectResult InternalError()
        {
            return Problem("Internal database error!", GetType().Name, StatusCodes.Status500InternalServerError, "Internal database error!");
        }

        private ObjectResult WrongUserNameProblem()
        {
            return Problem("Invalid token! Not found username", GetType().Name, StatusCodes.Status401Unauthorized,
                "Invalid token!");
        }
        private ObjectResult NotFoundProblem(string name)
        {
            return Problem(detail: $"{name} not found!", instance: GetType().Name, 404, $"{name} not found!");
        }
    }

    /// <summary>
    /// The users search types.
    /// </summary>
    public enum UsersSearchType
    {
        /// <summary>
        /// Gets all users.
        /// </summary>
        All,
        /// <summary>
        /// Gets active users only.
        /// </summary>
        ActiveOnly,
        /// <summary>
        /// Gets one user by username.
        /// </summary>
        One
    }
}