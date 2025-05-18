using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using versioning_manager_api.Attributes;
using versioning_manager_api.DevDatabase;
using versioning_manager_api.Middle;
using versioning_manager_api.Middle.ApiKeyProcess;
using versioning_manager_api.Middle.CryptsProcess;
using versioning_manager_api.Middle.Docker;
using versioning_manager_api.Middle.DockerCompose;
using versioning_manager_api.Middle.HashProcess;
using versioning_manager_api.Middle.UnitOfWorks;
using versioning_manager_api.Middlewares;
using versioning_manager_api.SystemObjects.Options;

namespace versioning_manager_api
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private const string BaseUrlPath = "/version-manager";

        private const string OptionConfigKey = "JWTOptions";
        private const string ApiKeyConfigKey = "ApiKeyOptions";
        private const string DefaultUserConfigKey = "DefaultUser";
        private const string DockerClientConfigKey = "DockerClient";
        private const string GitlabRegistryConfigKey = "GitlabRegistry";
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
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            string confFilePath = args.Length > 0 ? args.First() : "appsettings.json";
            
            ConfigureConfiguration(builder.Configuration, confFilePath);

            ConfigureLogging(builder.Logging);

            ConfigureServices(builder.Services, builder.Configuration);

            WebApplication app = builder.Build();

            
            app.UsePathBase(BaseUrlPath);
            app.UseRouting();
            
            app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseSwagger();
            app.UseSwaggerUI(opts =>
            {
                opts.SwaggerEndpoint($"{BaseUrlPath}/swagger/VersioningManagerApi/swagger.json", "VersioningManagerApi");
                opts.DocumentTitle = "Versioning Manager Api";
            });

            app.UseCheckCacheMiddleware();
            app.UseApiCheckMiddleware();



            app.MapControllers();

            app.Run();
        }

        private static void ConfigureConfiguration(ConfigurationManager configuration, string confFilePath)
        {
            configuration.AddJsonFile(confFilePath, false);
            configuration.AddEnvironmentVariables();
        }

        private static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Trace);
            //TODO: log formater
        }

        private static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration)
        {
            JwtOptions jwtOpts = configuration.GetSection(OptionConfigKey).Get<JwtOptions>() ?? throw new Exception("Not found JWT options in configuration!");
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
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(File.ReadAllText(jwtOpts.SecretFilePath)))
                };
            });
            
            services.AddRouting(opts =>
            {
                opts.LowercaseUrls = true;
            });
            services.AddMemoryCache();
            services.AddProblemDetails(opts =>
            {
                opts.CustomizeProblemDetails = ctx =>
                {
                    ctx.HttpContext.Response.StatusCode = ctx.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
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

            services.AddAppStartingService();
            

            services.AddCheckCacheMiddleware();
            services.AddApiCheckMiddleware();
            
            services.AddControllers(opts =>
            {
                opts.Filters.Add<RequireApiKeyAttribute>();
            }).AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            
            services.AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("VersioningManagerApi", new OpenApiInfo
                {
                    Title = "Versioning Manager Api",
                    License = new OpenApiLicense
                    {
                        Name = "UNLICENSED"
                    },
                    Version = "v1"
                });
                
                opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "bearer",
                    BearerFormat = "jwt-bearer",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
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
        }
    }
}