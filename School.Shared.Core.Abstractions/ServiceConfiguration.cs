using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace School.Shared.Core.Abstractions;

public abstract class ServiceConfiguration
{
    public IConfiguration Configuration { get; set; } = default!;

    public abstract void InjectMiddleware(IApplicationBuilder builder);
    public virtual void ConfigureMapper(TypeAdapterConfig config) { }
    public abstract void InjectServiceRegistrations(IServiceCollection services);
}