using EBCEYS.ContainersEnvironment.ServiceEnvironment;

namespace ServiceUploader.Environment;

public static class SupportedEnvironmentVariables
{
    public static ServiceEnvironmentVariable<string> Uri { get; } = new("SERVICEUPLOADER_VM_URI");
    public static ServiceEnvironmentVariable<string> Token { get; } = new("SERVICEUPLOADER_TOKEN");
    public static ServiceEnvironmentVariable<string> ProjectName { get; } = new("SERVICEUPLOADER_PROJECT_NAME");
    public static ServiceEnvironmentVariable<string> ImageVersion { get; } = new("SERVICEUPLOADER_IMAGE_VERSION");
    public static ServiceEnvironmentVariable<string> ImageTag { get; } = new("SERVICEUPLOADER_IMAGE_TAG");
    public static ServiceEnvironmentVariable<string> ServiceName { get; } = new("SERVICEUPLOADER_SERVICE_NAME");

    public static ServiceEnvironmentVariable<string> DockerComposeFile { get; } =
        new("SERVICEUPLOADER_DOCKER_COMPOSE_FILE");
}