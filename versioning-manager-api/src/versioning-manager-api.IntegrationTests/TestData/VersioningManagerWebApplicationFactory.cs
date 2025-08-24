using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using versioning_manager_api.Attributes;
using versioning_manager_api.Controllers.V1;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.Middle.Docker;

namespace versioning_manager_api.IntegrationTests.TestData;

internal class VersioningManagerWebApplicationFactory(string dbConnectionString)
    : WebApplicationFactory<Startup>
{
    [NotNull] public HttpClient? BaseClient { get; private set; }


    public Task InitializeAsync()
    {
        BaseClient = CreateClient();
        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddControllers(opts => { opts.Filters.Add<RequireApiKeyAttribute>(); }).AddApplicationPart(typeof(Startup).Assembly);
        });
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureWebHostDefaults(web => web.UseStartup<TestStartup>().UseEnvironment("TEST"));
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Sources.Clear();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"Serilog:MinimumLevel:Default", "Verbose"},
            {"Serilog:WriteTo:0:Name", "Console"},
            
            {"ConnectionStrings:postgres", dbConnectionString},
            {"JWTOptions:Issuer", "https://issuer.com"},
            {"JWTOptions:Audience", "https://issuer.com"},
            {"JWTOptions:SecretFilePath", FilesCreator.JwtKeyFilePath},
            {"JWTOptions:TokenTimeToLive", "00:30:00"},
            
            {"ApiKeyOptions:CryptKeyFilePath", FilesCreator.CryptKeyFilePath},
            {"ApiKeyOptions:CryptIVFilePath", FilesCreator.CryptIvFilePath},
            {"ApiKeyOptions:Prefix", "ebvm-"},
            
            {"DefaultUser:DefaultUsername", TestsContext.DefaultUsername},
            {"DefaultUser:DefaultPassword", TestsContext.DefaultPassword},
            {"DefaultUser:DefaultRoleName", TestsContext.DefaultRole},
            
            {"DockerClient:UseDefaultConnection", "true"},
            {"DockerClient:ConnectionTimeout", "00:00:10"},
            
            {"GitlabRegistry:Enabled", "false"},
            {"GitlabRegistry:Address" , "https://gitlab.com"},
            {"GitlabRegistry:Username", "root"},
            {"GitlabRegistry:KeyFile" , FilesCreator.GitlabKeyFilePath},
            
        });
    }

    public Task TeardownAsync()
    {
        return Task.CompletedTask;
    }
}

internal class TestStartup(IConfiguration configuration) : Startup(configuration)
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.RemoveAll<IDockerController>();
        services.AddSingleton<IDockerController, DockerControllerMock>();
    }
}