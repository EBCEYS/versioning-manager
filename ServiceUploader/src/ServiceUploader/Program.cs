using System.CommandLine;
using Docker.DotNet;
using Docker.DotNet.Models;
using ServiceUploader.Environment;
using ServiceUploader.Extensions;
using ServiceUploader.Middle;
using versioning_manager_api.Models.Requests.Images;

namespace ServiceUploader;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static string ModeDescription { get; } = "The mode used to start application\n" +
                                                     $"{Modes.Load} - loads image info to server.\n" +
                                                     "Options:\n" +
                                                     "\t-t|--token\n" +
                                                     "\t--uri\n" +
                                                     "\t--image\n" +
                                                     "\t--project-name\n" +
                                                     "\t--service-name\n" +
                                                     "\t--service-version\n" +
                                                     "\t--compose-path [optional]\n" +
                                                     $"{Modes.Save} - downloads images from server.\n" +
                                                     "Options:\n" +
                                                     "\t-t|--token\n" +
                                                     "\t--uri\n" +
                                                     "\t--project-name\n" +
                                                     $"{Modes.Update} - uploads images to docker (connecting by --uri). Images archives will be taken from current working directory.\n" +
                                                     "Options:\n" +
                                                     "\t--uri [optional]\n" +
                                                     $"{Modes.Post} - uploads the image file to server.\n" +
                                                     "Options: \n" + 
                                                     "\t-t|--token\n" +
                                                     "\t--compose-path - path to image.tar file\n";

    private static string IfNotSpecifiedString(string envKey)
    {
        return $"If not specified, environment variable '{envKey}' will be used.";
    }

    internal static int Main(string[] args)
    {
        Option<Modes> modeOption = new
        (
            "--mode",
            ModeDescription
        )
        {
            IsRequired = true
        };
        modeOption.AddAlias("-m");
        Option<string?> tokenOption = new
        (
            "--token",
            () => SupportedEnvironmentVariables.Token.Value,
            "The token used to start application. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.Token.Key)
        );
        tokenOption.AddAlias("-t");
        Option<string?> uriOption = new
        (
            "--uri",
            () => SupportedEnvironmentVariables.Uri.Value,
            "The URL for connection to versioning-manager server. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.Uri.Key)
        );
        Option<string?> imageTagOption = new
        (
            "--image",
            () => SupportedEnvironmentVariables.ImageTag.Value,
            "The image tag for the service uploader. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.ImageTag.Key)
        );
        Option<string?> projectNameOption = new
        (
            "--project-name",
            () => SupportedEnvironmentVariables.ProjectName.Value,
            "The project name for the service uploader. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.ProjectName.Key)
        );
        Option<string?> serviceNameOption = new
        (
            "--service-name",
            () => SupportedEnvironmentVariables.ServiceName.Value,
            "The service name for the service uploader. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.ServiceName.Key)
        );
        Option<string?> versionOption = new
        (
            "--service-version",
            () => SupportedEnvironmentVariables.ImageVersion.Value,
            "The version for the service uploader. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.ImageVersion.Key)
        );
        Option<string?> dockerComposePath = new
        (
            "--compose-path",
            () => SupportedEnvironmentVariables.DockerComposeFile.Value,
            "[OPTIONAL] The compose path for the service uploader. " +
            IfNotSpecifiedString(SupportedEnvironmentVariables.DockerComposeFile.Key)
        );

        RootCommand root = new();
        root.AddOption(modeOption);
        root.AddOption(tokenOption);
        root.AddOption(uriOption);
        root.AddOption(imageTagOption);
        root.AddOption(projectNameOption);
        root.AddOption(serviceNameOption);
        root.AddOption(versionOption);
        root.AddOption(dockerComposePath);

        root.SetHandler(async (mode, token, uri, image, project, service, version, compose) =>
            {
                switch (mode)
                {
                    case Modes.Load:
                        if (string.IsNullOrWhiteSpace(token))
                            throw new InvalidOperationException("The token used to start application");

                        await Load(uri, token, image, project, service, version, compose);
                        break;
                    case Modes.Save:
                        if (string.IsNullOrWhiteSpace(token))
                            throw new InvalidOperationException("The token used to start application");

                        await Save(uri, token, project);
                        break;
                    case Modes.Update:
                        await Update(uri);
                        break;
                    case Modes.Post:
                        if (string.IsNullOrWhiteSpace(token))
                            throw new InvalidOperationException("The token used to start application");
                        if (string.IsNullOrWhiteSpace(compose) || !File.Exists(compose))
                            throw new InvalidOperationException("The image.tar file should exist!");
                        if (string.IsNullOrWhiteSpace(uri))
                            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
                        await Post(uri, token, compose);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, "Mode parameter out of range");
                }
            }, modeOption, tokenOption, uriOption, imageTagOption, projectNameOption, serviceNameOption, versionOption,
            dockerComposePath);
        return root.Invoke(args);
    }

    private static async Task Post(string uri, string token, string file)
    {
        Uri path = new(uri);
        Console.WriteLine($"Send image archive {file} to server: {path}");
        VersioningManagerClient client = new(path, token, TimeSpan.FromMinutes(10.0));
        await using var fileStream = File.OpenRead(file);
        await client.PostImageAsync(fileStream);
        Console.WriteLine("Done!");
    }

    private static async Task Load(string? uri, string token, string? image, string? project, string? service,
        string? version, string? composePath)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
        if (string.IsNullOrWhiteSpace(image))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(image));
        if (string.IsNullOrWhiteSpace(project))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(project));
        if (string.IsNullOrWhiteSpace(service))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(service));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(version));
        var composeContext = string.Empty;
        if (composePath != null && File.Exists(composePath))
        {
            composeContext = await File.ReadAllTextAsync(composePath);
            Console.WriteLine("Read docker-compose file.");
        }

        UploadImageInfoModel model = new()
        {
            ImageTag = image,
            ProjectName = project,
            ServiceName = service,
            Version = version,
            DockerCompose = composeContext
        };
        Uri path = new(uri);
        Console.WriteLine($"Send image info to {path}");
        VersioningManagerClient client = new(path, token, TimeSpan.FromMinutes(10.0));
        await client.UploadImageAsync(model);
        Console.WriteLine("Done!");
    }

    private static async Task Save(string? uri, string token, string? project)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
        if (string.IsNullOrWhiteSpace(project))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(project));
        Uri url = new(uri);
        Console.WriteLine($"Start saving process of project {project} from {uri}...");
        VersioningManagerClient client = new(url, token, TimeSpan.FromMinutes(10.0));
        var info = await client.GetProjectInfoAsync(project);
        Console.WriteLine($"Get project info {info.Name} with actual members count {info.ActualEntries.Count()}");
        if (!info.ActualEntries.Any())
        {
            Console.WriteLine("No actual project versions found.");
            return;
        }

        Console.WriteLine("Select project version:");
        var entryVersion = SelectOne(info.ActualEntries.Select(x => x.Version));
        var entry = info.ActualEntries.First(e => e.Version == entryVersion);
        Console.WriteLine($"Selected entry {entry.Version}");
        if (!entry.Images.Any())
        {
            Console.WriteLine("No images found.");
            return;
        }

        var projPath = Path.Combine(Directory.GetCurrentDirectory(), project, entry.Version);

        Directory.CreateDirectory(projPath);

        foreach (var image in entry.Images)
        {
            var filePath = Path.Combine(projPath, $"{image.Tag.ToHash()}.tar");
            if (File.Exists(filePath))
            {
                Console.WriteLine("Image already exists. Go next...");
                continue;
            }

            Console.WriteLine($"Downloading image {image.Tag}...");
            await client.DownloadImageAsync(image.Id, filePath, (totalRead, currentSpeed) =>
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(
                    $"Progress: {FormatBytes(totalRead)} Speed: {currentSpeed:0.00} MB/s");
            }, (totalBytes, totalSeconds) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine($"Downloaded: {FormatBytes(totalBytes)} in " +
                                  $"{totalSeconds:0.00} seconds. " +
                                  $"Avg speed: {totalBytes / (totalSeconds * 1024 * 1024):0.00} MB/s");
                Console.ResetColor();
            });
        }

        try
        {
            var filePath = Path.Combine(projPath, "docker-compose.yaml");
            Console.WriteLine("Try to download docker-compose file...");
            await using var stream = await client.GetProjectComposeAsync(entry.Id);
            await using var fs = File.Create(filePath);
            await stream.CopyToAsync(fs);
            Console.WriteLine($"Save docker-compose file to {filePath}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error on downloading docker-compose file: {e.StatusCode}, {e.Message}");
        }

        Console.WriteLine("Done!");
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        var order = 0;
        double len = bytes;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private static T SelectOne<T>(IEnumerable<T> vals)
    {
        var valsList = vals.ToList();
        if (valsList.Count == 1) return valsList.First();

        Console.WriteLine("Objects: ");
        for (var i = 0; i < valsList.Count; i++) Console.WriteLine($"{i + 1}:\t{valsList[i]}");

        for (;;)
        {
            Console.WriteLine($"Select object: (1..{valsList.Count + 1})");
            if (!int.TryParse(Console.ReadLine(), out var index) || index < 1 || index > valsList.Count + 1)
            {
                Console.WriteLine("Incorrect input!");
                continue;
            }

            return valsList[index - 1];
        }
    }

    private static async Task Update(string? uri)
    {
        var config =
            uri == null ? new DockerClientConfiguration() : new DockerClientConfiguration(new Uri(uri));
        var client = config.CreateClient();
        foreach (var archive in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tar"))
        {
            Console.WriteLine($"Uploading archive '{archive}'...");
            await using var fs = File.OpenRead(archive);
            await client.Images.LoadImageAsync(new ImageLoadParameters(), fs,
                new Progress<JSONMessage>(msg => { Console.WriteLine($"{msg.Status}: {msg.ProgressMessage}"); }));
            Console.WriteLine($"Image '{archive}' uploaded successfully.");
        }

        Console.WriteLine("Done!"); //TODO: test it
    }
}

internal enum Modes
{
    Load,
    Save,
    Update,
    Post
}