using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using School.Shared.Core.Abstractions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace School.Core.Options;

public class SwaggerConfigOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly ServiceOptions _options;
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerConfigOptions(IOptions<ServiceOptions> options, IApiVersionDescriptionProvider provider)
    {
        _options = options.Value;
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, BuildApiInformation(description));
        }
    }

    private OpenApiInfo BuildApiInformation(ApiVersionDescription description)
    {
        var assembly = Assembly.GetEntryAssembly();

        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0";

        var info = new OpenApiInfo
        {
            Title = $"School.{_options.Type} - {_options.Name}",
            Version = version,
            Description = _options.Description
        };

        if (description.IsDeprecated)
        {
            info.Description += "Deprecated API version!";
        }

        return info;
    }
}