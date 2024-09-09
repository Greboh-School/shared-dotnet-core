using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using School.Shared.Core.Persistence.Filters;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace School.Core;

public static class ServiceHost<TEntryPoint> where TEntryPoint : EntryPoint, new()
{
    public static int Run(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext();
        
        var telemetryEnabled = bool.Parse(config["Config:TelemetryLogging:Enabled"]!);
        var telemetryHost = config["Config:TelemetryLogging:Host"]!;
        var telemetryApiKey = config["Config:TelemetryLogging:ApiKey"]!;
        if (telemetryEnabled)
        {
            Log.Logger = loggerConfig
                .WriteTo.Console()
                .WriteTo.OpenTelemetry(x =>
                {
                    x.Endpoint = $"{telemetryHost}/ingest/otlp/v1/logs";
                    x.Protocol = OtlpProtocol.HttpProtobuf;
                    x.Headers = new Dictionary<string, string> 
                    {
                        ["X-Seq-ApiKey"] = $"{telemetryApiKey}"
                    };
                    x.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = config["Config:Service:Name"]!
                    };

                })
                .CreateLogger();
        }
        else
        {
            Log.Logger = loggerConfig
                .WriteTo.Console()
                .CreateLogger();
        }
        
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();
            
            var entryPoint = new TEntryPoint
            {
                Configuration = builder.Configuration,
            };
            
            entryPoint.ConfigureServiceContainer(builder.Services);

            var app = builder.Build();
            
            entryPoint.ConfigureAppPipeline(app);

            var migratorEnabled = bool.Parse(config["Config:Inclusion:Migrator"]!);

            if (migratorEnabled)
            {
                ApplyMigrations(app).Wait();
            }
            
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error at application startup!");
        }

        return 0;
    }

    private static async Task ApplyMigrations(IHost host)
    {
        var provider = host.Services.GetRequiredService<IServiceProvider>();

        using var scope = provider.CreateScope();

        var migrations = scope.ServiceProvider.GetService<IEnumerable<IMigrationFilter>>();

        if (migrations?.Any() ?? false)
        {
            foreach (var migration in migrations)
            {
                await migration.ApplyPending();
            }
        }
    }

}


public static class ServiceHost
{
    public static int Run(string[] args)
    {
        return ServiceHost<EntryPoint>.Run(args);
    }
}