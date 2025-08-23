using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.Middle.Docker;

namespace versioning_manager_api.IntegrationTests.TestData;

internal class VersioningManagerWebApplicationFactory(string dbConnectionString)
    : WebApplicationFactory<Startup>
{
    [NotNull] public string? BaseAddress { get; private set; }


    public Task InitializeAsync()
    {
        BaseAddress = CreateClient().BaseAddress!.ToString();
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDockerController>();
            services.TryAddSingleton<IDockerController, DockerControllerMock>();
        });

        builder.ConfigureAppConfiguration(ConfigureConfiguration);

        //base.ConfigureWebHost(builder);
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Properties["ConnectionStrings:0"] = dbConnectionString;

        config.Properties["JWTOptions:Issuer"] = "https://issuer.com";
        config.Properties["JWTOptions:Audience"] = "https://issuer.com";
        config.Properties["JWTOptions:SecretFilePath"] = FilesCreator.JwtKeyFilePath;
        config.Properties["JWTOptions:TokenTimeToLive"] = "00:30:00";

        config.Properties["ApiKeyOptions:CryptKeyFilePath"] = FilesCreator.CryptKeyFilePath;
        config.Properties["ApiKeyOptions:CryptIVFilePath"] = FilesCreator.CryptIvFilePath;
        config.Properties["ApiKeyOptions:Prefix"] = "ebvm-";

        config.Properties["DefaultUser:DefaultUsername"] = TestsContext.DefaultUsername;
        config.Properties["DefaultUser:DefaultPassword"] = TestsContext.DefaultPassword;
        config.Properties["DefaultUser:DefaultRoleName"] = TestsContext.DefaultRole;

        config.Properties["DockerClient:UseDefaultConnection"] = "true";
        config.Properties["DockerClient:ConnectionTimeout"] = "00:00:10";

        config.Properties["GitlabRegistry:Address"] = "https://my-gitlab.com";
        config.Properties["GitlabRegistry:Username"] = "gitlab";
        config.Properties["GitlabRegistry:KeyFile"] = FilesCreator.GitlabKeyFilePath;
    }

    public Task TeardownAsync()
    {
        return Task.CompletedTask;
    }
}