using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace School.Shared.Core.Persistence.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMySQLContext<TContext>(this IServiceCollection services, string dbName, IConfiguration config)
        where TContext : DbContext
    {
        var connectionString = config.GetConnectionString("MySQL");
        var version = new MySqlServerVersion("8.0.26");

        services.AddDbContext<DbContext, TContext>(opt =>
            opt.UseMySql($"{connectionString};Database={dbName}", version, opt =>
            {
                opt.EnableRetryOnFailure();
            })
        );

        return services;
    }
}