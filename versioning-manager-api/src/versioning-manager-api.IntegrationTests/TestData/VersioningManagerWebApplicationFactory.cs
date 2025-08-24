using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using versioning_manager_api.IntegrationTests.Mocks;
using versioning_manager_api.Middle.Docker;

namespace versioning_manager_api.IntegrationTests.TestData;

public class VersioningManagerWebApplicationFactory(string dbConnectionString)
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
        //builder.ConfigureTestServices(services =>
        //{
        //    services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);
        //    //services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
        //    //services.AddControllers(opts => { opts.Filters.Add<RequireApiKeyAttribute>(); })
        //    //    .AddApplicationPart(typeof(Startup).Assembly).AddJsonOptions(ConfigureJsonOptions);
        //});
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureWebHostDefaults(web =>
                web.UseStartup<TestStartup>().UseEnvironment("Development"));
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Sources.Clear();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Serilog:MinimumLevel:Default", "Verbose" },
            { "Serilog:WriteTo:0:Name", "Console" },

            { "ConnectionStrings:postgres", dbConnectionString },
            { "JWTOptions:Issuer", "https://issuer.com" },
            { "JWTOptions:Audience", "https://issuer.com" },
            { "JWTOptions:SecretFilePath", FilesCreator.JwtKeyFilePath },
            { "JWTOptions:TokenTimeToLive", "00:30:00" },

            { "ApiKeyOptions:CryptKeyFilePath", FilesCreator.CryptKeyFilePath },
            { "ApiKeyOptions:CryptIVFilePath", FilesCreator.CryptIvFilePath },
            { "ApiKeyOptions:Prefix", "ebvm-" },

            { "DefaultUser:DefaultUsername", TestsContext.DefaultUsername },
            { "DefaultUser:DefaultPassword", TestsContext.DefaultPassword },
            { "DefaultUser:DefaultRoleName", TestsContext.DefaultRole },

            { "DockerClient:UseDefaultConnection", "true" },
            { "DockerClient:ConnectionTimeout", "00:00:10" },

            { "GitlabRegistry:Enabled", "false" },
            { "GitlabRegistry:Address", "https://gitlab.com" },
            { "GitlabRegistry:Username", "root" },
            { "GitlabRegistry:KeyFile", FilesCreator.GitlabKeyFilePath }
        });
    }

    public Task TeardownAsync()
    {
        return Task.CompletedTask;
    }

    private static void ConfigureJsonOptions(JsonOptions opts)
    {
        var webOpts = JsonSerializerOptions.Web;
        opts.AllowInputFormatterExceptionMessages = true;
        opts.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = webOpts.AllowOutOfOrderMetadataProperties;
        opts.JsonSerializerOptions.AllowTrailingCommas = webOpts.AllowTrailingCommas;
        foreach (var webOptsConverter in webOpts.Converters)
            opts.JsonSerializerOptions.Converters.Add(webOptsConverter);

        opts.JsonSerializerOptions.DefaultBufferSize = webOpts.DefaultBufferSize;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = webOpts.DefaultIgnoreCondition;
        opts.JsonSerializerOptions.DictionaryKeyPolicy = webOpts.DictionaryKeyPolicy;
        opts.JsonSerializerOptions.Encoder = webOpts.Encoder;
        opts.JsonSerializerOptions.IgnoreReadOnlyFields = webOpts.IgnoreReadOnlyFields;
        opts.JsonSerializerOptions.IgnoreReadOnlyProperties = webOpts.IgnoreReadOnlyProperties;
        opts.JsonSerializerOptions.IncludeFields = webOpts.IncludeFields;
        opts.JsonSerializerOptions.IndentCharacter = webOpts.IndentCharacter;
        opts.JsonSerializerOptions.IndentSize = webOpts.IndentSize;
        opts.JsonSerializerOptions.MaxDepth = webOpts.MaxDepth;
        opts.JsonSerializerOptions.NewLine = webOpts.NewLine;
        opts.JsonSerializerOptions.NumberHandling = webOpts.NumberHandling;
        opts.JsonSerializerOptions.PreferredObjectCreationHandling = webOpts.PreferredObjectCreationHandling;
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = webOpts.PropertyNameCaseInsensitive;
        opts.JsonSerializerOptions.PropertyNamingPolicy = webOpts.PropertyNamingPolicy;
        opts.JsonSerializerOptions.ReadCommentHandling = webOpts.ReadCommentHandling;
        opts.JsonSerializerOptions.ReferenceHandler = webOpts.ReferenceHandler;
        opts.JsonSerializerOptions.RespectNullableAnnotations = webOpts.RespectNullableAnnotations;
        opts.JsonSerializerOptions.RespectRequiredConstructorParameters = webOpts.RespectRequiredConstructorParameters;
        opts.JsonSerializerOptions.TypeInfoResolver = webOpts.TypeInfoResolver;
        opts.JsonSerializerOptions.UnknownTypeHandling = webOpts.UnknownTypeHandling;
        opts.JsonSerializerOptions.UnmappedMemberHandling = webOpts.UnmappedMemberHandling;
        opts.JsonSerializerOptions.WriteIndented = webOpts.WriteIndented;
    }
}

public class TestStartup(IConfiguration configuration) : Startup(configuration)
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.RemoveAll<IDockerController>();
        services.AddSingleton<IDockerController, DockerControllerMock>();

        services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
    }
}