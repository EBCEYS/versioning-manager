using System.Text;
using FluentAssertions;
using versioning_manager_api.Middle.DockerCompose;

namespace versioning_manager_api.UnitTests.Helpers;

public class DockerComposeHelperTests
{
    private DockerComposeHelper dcHelper = null!;

    [SetUp]
    public void Initialize()
    {
        dcHelper = new DockerComposeHelper();
    }

    [Test]
    public void When_MergeVersionsTag_With_DockerComposeFiles_Result_OneVersionTag()
    {
        string[] dockerComposeFiles =
        [
            "version: \"3.8\"", "version: \"3.8\"\nx-package: 0.72.0", "version: \"3.8\"\nx-package: 0.72.0\nservices:"
        ];


        using Stream result = dcHelper.GetTotalCompose(dockerComposeFiles);
        string totalDockerCompose = GetString(result);

        string[] lines = totalDockerCompose.Split("\n").Where(l => !string.IsNullOrEmpty(l.Replace("...", "").Trim()))
            .ToArray();
        lines.Length.Should().Be(3);
        int count = lines.Count(line => line.Contains("version: \"3.8\""));
        count.Should().Be(1);
    }

    [Test]
    public void When_MergeServices_With_DockerComposeFiles_Result_TwoServices()
    {
        string[] dockerComposeFiles =
        [
            "version: \"3.8\"\nx-package: 0.72.0\nservices:\n  http-receiver-v2-c:\n    container_name: http-receiver-v2\n    hostname: http-receiver-v2\n    image: registry.loc/platform/iot/http-receiver-v2:2.1.7.7\n    command: app-runner --config /app-runner/app.toml\n    entrypoint:\n      - /usr/local/bin/dumb-init",
            "version: \"3.8\"\nx-package: 0.72.0\nservices:\n  http-receiver-v2-c-1:\n    container_name: http-receiver-v2-1\n    hostname: http-receiver-v2-1\n    image: registry.loc/platform/iot/http-receiver-v2:2.1.7.7\n    command: app-runner --config /app-runner/app.toml\n    entrypoint:\n      - /usr/local/bin/dumb-init",
            "version: \"3.8\"\nx-package: 0.72.0\nservices:\n  http-receiver-v2-c-1:\n    container_name: http-receiver-v2-1\n    hostname: http-receiver-v2-1\n    image: registry.loc/platform/iot/http-receiver-v2:2.1.7.7\n    command: app-runner --config /app-runner/app.toml\n    entrypoint:\n      - /usr/local/bin/dumb-init"
        ];


        using Stream result = dcHelper.GetTotalCompose(dockerComposeFiles);
        string totalDockerCompose = GetString(result);

        string[] lines = totalDockerCompose.Split("\n").Where(l => !string.IsNullOrEmpty(l.Replace("...", "").Trim()))
            .ToArray();
        int count = lines.Count(line => line.Contains("http-receiver-v2-c"));
        count.Should().Be(2);
    }

    [Test]
    public void When_MergeNetworks_With_DockerComposeFiles_Result_ThreeNetworks()
    {
        string[] dockerComposeFiles =
        [
            "networks:\n  disp-network:\n    external: true\n    name: disp-network",
            "networks:\n  disp-network:\n    external: true\n    name: disp-network",
            "networks:\n  disp-network:\n    external: true\n    name: disp-network"
        ];


        using Stream result = dcHelper.GetTotalCompose(dockerComposeFiles);
        string totalDockerCompose = GetString(result);

        string[] lines = totalDockerCompose.Split("\n").Where(l => !string.IsNullOrEmpty(l.Replace("...", "").Trim()))
            .ToArray();
        int count = lines.Count(line => line.Contains("networks:"));
        count.Should().Be(1);
    }

    private static string GetString(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(stream);
        StringBuilder sb = new();
        while (sr.ReadLine() is { } line) sb.AppendLine(line);

        return sb.ToString();
    }
}