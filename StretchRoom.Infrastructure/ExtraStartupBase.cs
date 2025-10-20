using System.Reflection;
using Asp.Versioning;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using HealthChecks.ApplicationStatus.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using StretchRoom.Infrastructure.ControllerFilters;
using StretchRoom.Infrastructure.Extensions;
using StretchRoom.Infrastructure.Helpers;
using StretchRoom.Infrastructure.Helpers.Jwt;
using StretchRoom.Infrastructure.Interfaces;
using StretchRoom.Infrastructure.Models;
using StretchRoom.Infrastructure.Options;

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
    ///     Indicates that server should use authentication.
    /// </summary>
    protected virtual bool UseAuthentication { get; init; }

    /// <summary>
    ///     Indicates that jwt should be proxied and not be configured at the service.
    /// </summary>
    protected virtual bool ProxyToken { get; init; } = true;

    /// <summary>
    ///     The health check server port.
    /// </summary>
    protected virtual int? HealthCheckPort { get; init; }

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
        services.AddHttpContextAccessor();

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

        if (UseAuthentication)
        {
            services.AddAuthorization();
            ConfigureAuthentication(services);
        }

        ConfigureSwagger(services);


        var healthChecksBuilder = services.ConfigureHealthChecks();
        healthChecksBuilder?.AddApplicationStatus();
        healthChecksBuilder?.AddApplicationInsightsPublisher();
        ConfigureHealthChecks(healthChecksBuilder);

        services.AddSingleton<ICommandExecutor, CommandExecutor>();
        services.AddScoped<IScopedCommandExecutor, ScopedCommandExecutor>();
        ServicesConfiguration(services);
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

        app.UseMetricServer();
        app.UseHttpMetrics();

        if (HealthCheckPort is null)
            app.ConfigureHealthChecks();
        else
            app.ConfigureHealthChecks(HealthCheckPort.Value);

        if (UseAuthentication)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            foreach (var apiVersion in ServiceApiInfo.ApiVersions)
                opts.SwaggerEndpoint($"{ServiceApiInfo.BaseAddress}/swagger/{apiVersion}/swagger.json", apiVersion);
            opts.DocumentTitle = ServiceApiInfo.ServiceName;
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
        var builder = services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtGenerator.AuthSchema;
            opts.DefaultScheme = JwtGenerator.AuthSchema;
            opts.DefaultChallengeScheme = JwtGenerator.AuthSchema;
        });

        if (ProxyToken) // do it later after users-service realization!!! On start request users-service for jwt settings and apply it here
        {
            builder.AddJwtBearer();
            return;
        }
        services.Configure<JwtOptions>(Configuration.GetSection(JwtOptionsConfigPath));
        var jwtOpts = Configuration.GetSection(JwtOptionsConfigPath).Get<JwtOptions>()
                      ?? throw new ApplicationException("Can not find jwt options!");
        builder.AddJwtBearer(cfg =>
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
        services.TryAddSingleton<IJwtGenerator, JwtGenerator>();
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
            if (UseAuthentication)
            {
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
            }

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