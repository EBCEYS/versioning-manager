using System.Reflection;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using versioning_manager_api.Attributes;
using versioning_manager_api.DbContext.DevDatabase;
using versioning_manager_api.Middle;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.Middle.Docker;
using versioning_manager_api.Middle.DockerCompose;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Middle.UnitOfWorks;
using versioning_manager_api.Middlewares;
using versioning_manager_api.SystemObjects.Options;
using static versioning_manager_api.Program;

namespace versioning_manager_api;

/// <summary>
///     The startup.
/// </summary>
/// <param name="configuration"></param>
public class Startup(IConfiguration configuration) : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilog((srv, lc) =>
            lc.ReadFrom.Configuration(configuration).ReadFrom.Services(srv));

        var jwtOpts = configuration.GetSection(OptionConfigKey).Get<JwtOptions>() ??
                      throw new Exception("Not found JWT options in configuration!");
        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtOpts.Issuer,
                ValidAudience = jwtOpts.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = jwtOpts.TokenTimeToLive != null,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Convert.FromBase64String(File.ReadAllText(jwtOpts.SecretFilePath)))
            };
        });

        services.AddRouting(opts => { opts.LowercaseUrls = true; });
        services.AddMemoryCache();
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                ctx.HttpContext.Response.StatusCode =
                    ctx.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            };
        });
        services.AddDbContext<VmDatabaseContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("postgres")));

        //service services
        services.AddUnitsOfWork();
        services.AddPasswordHasher();
        services.AddCryptHelper();
        services.AddApiKeyProcessor();
        services.AddDockerController();
        services.AddDockerComposeHelper();


        services.Configure<JwtOptions>(configuration.GetSection(OptionConfigKey));
        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyConfigKey));
        services.Configure<DefaultUserOptions>(configuration.GetSection(DefaultUserConfigKey));
        services.Configure<DockerClientOptions>(configuration.GetSection(DockerClientConfigKey));
        services.Configure<GitlabRegistryConnectionOptions>(configuration.GetSection(GitlabRegistryConfigKey));

        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
        });

        services.AddAppStartingService();


        services.AddCheckCacheMiddleware();
        services.AddApiCheckMiddleware();

        services.AddControllers(opts => { opts.Filters.Add<RequireApiKeyAttribute>(); })
            .AddJsonOptions(ConfigureJsonOptions);

        services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc(SwaggerV1Name, new OpenApiInfo
            {
                Title = "Versioning Manager Api",
                License = new OpenApiLicense
                {
                    Name = "MIT"
                },
                Version = SwaggerV1Name
            });

            opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "bearer",
                BearerFormat = "jwt-bearer",
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            opts.IncludeXmlComments(Assembly.GetExecutingAssembly(), true);
        });

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;

            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader()
            );
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.SubstituteApiVersionInUrl = true;
        });
        base.ConfigureServices(services);
    }

    /// <inheritdoc />
    public override void Configure(IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();

        app.UsePathBase(BaseUrlPath);
        app.UseRouting();

        app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint($"{BaseUrlPath}/swagger/{SwaggerV1Name}/swagger.json", SwaggerV1Name);
            opts.DocumentTitle = "Versioning Manager Api";
        });

        app.UseCheckCacheMiddleware();
        app.UseApiCheckMiddleware();


        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
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