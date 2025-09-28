using System.Reflection;
using Asp.Versioning;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using HealthChecks.ApplicationStatus.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using StretchRoom.Infrastructure.ControllerFilters;
using StretchRoom.Infrastructure.Extensions;
using StretchRoom.Infrastructure.Helpers;
using StretchRoom.Infrastructure.Interfaces;
using StretchRoom.Infrastructure.Middlewares;
using StretchRoom.Infrastructure.Models;
using StretchRoom.Infrastructure.Options;
using StretchRoom.Infrastructure.Services.ExecutedServices;

// ReSharper disable MemberCanBePrivate.Global

namespace StretchRoom.Infrastructure;

/// <summary>
///     The extra startup base class.
/// </summary>
/// <param name="configuration">The configuration.</param>
[PublicAPI]
public abstract class ExtraStartupBase(IConfiguration configuration) : IStartupBase
{
    private const string JwtOptionsConfigPath = "JwtOptions";

    /// <summary>
    ///     Indicates that server should user authorization.
    /// </summary>
    protected bool UseAuthorization { get; init; }

    /// <summary>
    ///     Indicates that server should use authentication.
    /// </summary>
    protected bool UseAuthentication { get; init; }

    /// <summary>
    ///     The service api info.
    /// </summary>
    protected abstract ServiceApiInfo ServiceApiInfo { get; init; }

    /// <summary>
    ///     The configuration.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    protected IConfiguration Configuration { get; } = configuration;

    /// <summary>
    ///     Configures the <paramref name="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilog((_, loggerConf)
            => loggerConf.ReadFrom.Configuration(Configuration));
        services.AddControllers(opts =>
        {
            opts.Filters.Add<ApiExceptionFilter>();
            ConfigureFilters(opts.Filters);
        });

        services.AddFluentValidationAutoValidation();

        services.AddRouting(opts => { opts.LowercaseUrls = true; });
        services.AddProblemDetails(opts =>
        {
            opts.CustomizeProblemDetails = ctx =>
            {
                ctx.HttpContext.Response.StatusCode =
                    ctx.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            };
        });
        services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
        });

        if (UseAuthorization) services.AddAuthorization();

        if (UseAuthentication) ConfigureAuthentication(services);

        ConfigureSwagger(services);


        var healthChecksBuilder = services.ConfigureHealthChecks();
        healthChecksBuilder?.AddApplicationStatus();
        healthChecksBuilder?.AddApplicationInsightsPublisher();
        ConfigureHealthChecks(healthChecksBuilder);

        services.AddSingleton<ICommandExecutor, CommandExecutor>();
        ServicesConfiguration(services);
        
        services.ExecuteAllBeforeHostingStarted();
    }

    /// <summary>
    ///     Configures the <paramref name="app" /> with <paramref name="env" />.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UsePathBase(ServiceApiInfo.BaseAddress);
        app.UseRouting();
        app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

        app.UseRequestLogging();

        app.UseMetricServer($"{ServiceApiInfo.BaseAddress}{RequestMetricsMiddleware.MetricsPath}");
        app.UseHttpMetrics();

        app.ConfigureHealthChecks();

        if (UseAuthentication) app.UseAuthentication();

        if (UseAuthorization) app.UseAuthorization();

        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            foreach (var apiVersion in ServiceApiInfo.ApiVersions)
                opts.SwaggerEndpoint($"{ServiceApiInfo.BaseAddress}/swagger/{apiVersion}/swagger.json", apiVersion);
            opts.DocumentTitle = ServiceApiInfo.ServiceName;
            opts.RoutePrefix = ServiceApiInfo.BaseAddress; //TODO: test it
            opts.DocumentTitle = ServiceApiInfo.Description;
        });


        ConfigureMiddlewares(app, env);

        app.UseExceptionCatcher();
        app.UseRequestMetrics();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    /// <summary>
    ///     Use it to configure your custom filters.
    /// </summary>
    /// <param name="filters">The filters.</param>
    protected virtual void ConfigureFilters(FilterCollection filters)
    {
    }

    /// <summary>
    ///     Configures the health checks.
    /// </summary>
    /// <param name="builder">The healthcheck builder.</param>
    /// <returns>The enhanced instance of <paramref name="builder" />.</returns>
    protected virtual IHealthChecksBuilder? ConfigureHealthChecks(IHealthChecksBuilder? builder)
    {
        return builder;
    }

    /// <summary>
    ///     Service configuration.
    /// </summary>
    /// <param name="services">The service configuration.</param>
    protected abstract void ServicesConfiguration(IServiceCollection services);

    /// <summary>
    ///     Configures the app. It calls before controllers.
    ///     <br />
    ///     The middlewares could be configured here.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    protected abstract void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env);

    private void ConfigureAuthentication(IServiceCollection services)
    {
        services.Configure<JwtOptions>(Configuration.GetSection(JwtOptionsConfigPath));
        var jwtOpts = Configuration.GetSection(JwtOptionsConfigPath).Get<JwtOptions>()
                      ?? throw new ApplicationException("Can not find jwt options!");
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
                    new SymmetricSecurityKey(Convert.FromBase64String(jwtOpts.Base64Key))
            };
        });
    }

    private void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opts =>
        {
            foreach (var apiVersion in ServiceApiInfo.ApiVersions)
                opts.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = $"{ServiceApiInfo.ServiceName} - {apiVersion}",
                    Description = ServiceApiInfo.Description,
                    Version = apiVersion
                });
            opts.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);
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
    }
}