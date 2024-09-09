using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace School.Shared.Core.Persistence.Filters;

public interface IMigrationFilter
{
    public Task ApplyPending();
}

public class MigrationFilter<TContext> : IMigrationFilter where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<MigrationFilter<TContext>> _logger;

    public MigrationFilter(TContext context, ILogger<MigrationFilter<TContext>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ApplyPending()
    {
        if (!await _context.Database.CanConnectAsync())
        {
            _logger.LogInformation("Database does not exist. Creating database...");
            await _context.Database.MigrateAsync();
        }
        
        var migrations = await _context.Database.GetPendingMigrationsAsync();
        var pending = migrations.ToList();

        if (!pending.Any())
        {
            _logger.LogInformation("No pending migrations for {contextType}", typeof(TContext).Name);
            return;
        }
        
        _logger.LogInformation(
            "Applying {count} migrations for {contextType} with names: {migrationNames}",
            pending.Count, typeof(TContext).Name, string.Join(", ", pending));
        
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));

        foreach (var migration in pending)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Migrations/Scripts", $"{migration}.sql");
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"A migration script is missing for {migration}", path);
            }

            var raw = await File.ReadAllTextAsync(path);

            await _context.Database.ExecuteSqlRawAsync(raw);
            
            _logger.LogInformation("Applied {migration} successfully", migration);
        }
    }
}