using Asp.Versioning.ApiExplorer;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using StretchRoom.Infrastructure.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StretchRoom.Infrastructure.Helpers.Swagger;

/// <summary>
///     The <see cref="ConfigureSwaggerOptions" /> class.
/// </summary>
/// <param name="provider"></param>
[PublicAPI]
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptions<ServiceApiInfo> serviceApiInfo)
    : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    ///     The api info. Will use instead of default if specified.
    /// </summary>
    public static OpenApiInfo? ApiInfo { get; set; }

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = ApiInfo ?? new OpenApiInfo
        {
            Title = serviceApiInfo.Value.ServiceName,
            Version = description.ApiVersion.ToString(),
            Description = serviceApiInfo.Value.Description
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}