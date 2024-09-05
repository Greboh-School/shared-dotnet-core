using System.Reflection;
using System.Runtime.Loader;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using School.Core.Extensions;
using School.Shared.Core.Abstractions;
using School.Shared.Core.Extensions;

namespace School.Core;

public class EntryPoint
{
    // Security measure to only allow certain plugins
    private static readonly List<string> _AllowedPluginAssemblyPrefix = new() { "School" };
    
    private static readonly List<ServiceConfiguration> _PluginConfigurations = new();
    
    public IConfiguration? Configuration { get; init; }

    public EntryPoint()
    {
        LoadPluginAssemblies();
    }

    private static void LoadPluginAssemblies()
    {
        // Get the entry assembly (main application assembly).
        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly is null)
        {
            throw new("GetEntryAssembly returned null!");
        }
        
        // Load all assemblies from entry
        foreach (var asm in entryAssembly.GetReferencedAssemblies())
        {
            try
            {
                AssemblyLoadContext.Default.LoadFromAssemblyName(asm);
            }
            catch
            {
                continue;
            }
        }
        
        // Filter plugins
        var plugins = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x => !x.IsDynamic) // Filter out dynamically generated assemblies
            .Where(x => x.FullName is { } name && _AllowedPluginAssemblyPrefix.Any(y => name.StartsWith(y))) // Make sure assemblies name starts with allowed name prefix
            .SelectMany(x => x.GetExportedTypes()) // Flatten all the public types from the filtered assemblies into a single collection
            .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(ServiceConfiguration))) // Filter out abstract assembles and those that are not a subclass of ServiceConfiguration
            .Distinct() // Remove duplicate types
            .Select(x => (ServiceConfiguration)Activator.CreateInstance(x)!) // Create an instance of the assemblies
            .Where(x => x is not null) // Ensure none of the instances are null (sanity check)
            .ToList(); // Convert the final IEnumerable<ServiceConfiguration> to a List<ServiceConfiguration>
        
        if (_PluginConfigurations.Any())
        {
            _PluginConfigurations.Clear();
        }
        
        _PluginConfigurations.AddRange(plugins);
    }
    
    public virtual void ConfigureAppPipeline(IApplicationBuilder appBuilder)
    {
        appBuilder.Configure();

        ApplyMiddlewareConfigurations(appBuilder, _PluginConfigurations);
    }

    public virtual void ConfigureServiceContainer(IServiceCollection services)
    {
        services.Configure(Configuration!);

        ApplyServiceRegistrations(services, _PluginConfigurations);
        ConfigureObjectMapping(services, _PluginConfigurations);
    }

    protected virtual void ApplyMiddlewareConfigurations(IApplicationBuilder builder, List<ServiceConfiguration> pluginConfigurations)
    {
        foreach (var config in pluginConfigurations)
        {
            config.Configuration = Configuration!;
            config.InjectMiddleware(builder);
        }
    }
    
    protected virtual void ApplyServiceRegistrations(IServiceCollection services, List<ServiceConfiguration> pluginConfigurations)
    {
        foreach (var config in pluginConfigurations)
        {
            config.Configuration = Configuration!;
            config.InjectServiceRegistrations(services);
        }
    }

    protected virtual void ConfigureObjectMapping(IServiceCollection services, List<ServiceConfiguration> configurations)
    {
        var mapperConfig = new TypeAdapterConfig();

        foreach (var config in configurations)
        {
            config.ConfigureMapper(mapperConfig);
        }

        services.AddSingleton(mapperConfig);
    }
}