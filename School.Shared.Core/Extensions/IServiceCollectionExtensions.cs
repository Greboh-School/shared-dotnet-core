using FastExpressionCompiler; 
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using School.Core.Options;
using School.Shared.Core.Abstractions.Options;
using School.Shared.Core.Authentication.Extensions;
using School.Shared.Core.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace School.Shared.Core.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection Configure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ServiceOptions>(configuration.GetRequiredSection(ServiceOptions.Section));
        services.Configure<InclusionOptions>(configuration.GetSection(InclusionOptions.Section));
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.Section));

        var config = configuration.GetRequiredSection(ConfigOptions.Section).Get<ConfigOptions>()!;

        if (config.Inclusion.Versioning)
        {
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new(1, 0);
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.SubstituteApiVersionInUrl = true;
                options.GroupNameFormat = "'v'VVV";
            });
        }

        if (config.Inclusion.Auth)
        {
            services.AddServiceAuth(config.Auth);
        }

        if (config.Inclusion.MVC)
        {
            var mvcBuilder = services.AddControllers(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
                options.Filters.Add<ExceptionFilter>();
            });
            
            // Since this is a used as a NuGet we need to scan all assemblies and add them
            // This makes it possible for MVC to discover and use controllers and other MVC components and not just the main app assembly
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                mvcBuilder.PartManager.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        }

        if (config.Inclusion.Swagger)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigOptions>();

            services.AddSwaggerGen(options =>
            {
                options.UseAllOfToExtendReferenceSchemas();
                options.CustomOperationIds(x => $"{x.ActionDescriptor.RouteValues["action"]}");

                var schemeName = "httpBearer"; 
                var securityScheme = new OpenApiSecurityScheme
                {
                    Description = "Jwt Authorization using Bearer.",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    Reference = new()
                    {
                        Id = schemeName,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(schemeName, securityScheme);
                options.AddSecurityRequirement(new()
                {
                    { securityScheme, new List<string>() }
                });
            });

            services.AddEndpointsApiExplorer();
        }

        if (config.Inclusion.Mapper)
        {
            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
            TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = false;

            services.AddSingleton<IMapper, ServiceMapper>();
        }

        return services;
    }
}