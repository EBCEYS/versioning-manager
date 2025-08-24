using Microsoft.AspNetCore;
using versioning_manager_api.Routes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace versioning_manager_api;

// ReSharper disable once ClassNeverInstantiated.Global
public class Program
{
    public const string SwaggerV1Name = "v1";
    public const string OptionConfigKey = "JWTOptions";
    public const string ApiKeyConfigKey = "ApiKeyOptions";
    public const string DefaultUserConfigKey = "DefaultUser";
    public const string DockerClientConfigKey = "DockerClient";
    public const string GitlabRegistryConfigKey = "GitlabRegistry";

    public static readonly string BaseServicePath = $"/{ControllerRoutes.BaseUrlPath}";

    /* Plans:
     * Post image info:
     * 1. post service image data with binding to project
     * 2. posting access granted by api key
     * 3. store information in postgresql mb
     * Administration: +
     * 1. create api tokens for posters
     * 2. in first start: create administrator user
     * 3. available to change password && create new users and blah-blah-blah
     * Get app:
     * 1. request app by project name.
     * 2. foreach (image in gitlab-registry where image_name == project.image_name) download();
     * 3. create docker-compose file.
     * 4. mb create changelog document
     */
    public static void Main(string[] args)
    {
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run();
    }
}