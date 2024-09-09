using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using School.Shared.Core.Abstractions.Options;

namespace School.Core.Extensions;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder Configure(this IApplicationBuilder builder)
    {
        var inclusion = builder.ApplicationServices.GetRequiredService<IOptions<InclusionOptions>>().Value;

        if (inclusion.MVC)
        {
            builder.UseResponseCaching();

            builder.UseRouting();

            if (inclusion.Auth)
            {
                builder.UseAuthorization();
            }

            builder.UseEndpoints(opt =>
            {
                opt.MapControllers();
            });
        }

        if (inclusion.Swagger)
        {
            builder.UseCustomSwagger();
        }

        return builder;
    }

    private static void UseCustomSwagger(this IApplicationBuilder builder)
    {
        var options = builder.ApplicationServices.GetRequiredService<IOptions<ServiceOptions>>().Value;

        var apiProvider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        builder.UseSwagger(config =>
        {
            config.RouteTemplate = $"{options.Route}/swagger/{{documentName}}/swagger.json";

            if (!options.Debug)
            {
                config.PreSerializeFilters.Add((swagger, _) =>
                {
                    var paths = new OpenApiPaths();
            
                    foreach (var path in swagger.Paths)
                    {
                        paths.Add(path.Key.Replace("/api/", $"/{options.Route}/"), path.Value);
                    }
                    
                    swagger.Paths = paths;
                });
            }
        });

        builder.UseSwaggerUI(config =>
        {
            foreach (var description in apiProvider.ApiVersionDescriptions)
            {
                var title = $"School.{options.Type} - {options.Name} {description.GroupName}";

                if (options.Debug)
                {
                    config.SwaggerEndpoint($"/api/swagger/{description.GroupName}/swagger.json", title);
                }
                else
                {
                    config.SwaggerEndpoint($"/{options.Route}/swagger/{description.GroupName}/swagger.json", title);
                }
            }

            config.RoutePrefix = $"{options.Route}/swagger";
        });
    }
}