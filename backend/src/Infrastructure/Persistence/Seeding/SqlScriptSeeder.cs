namespace TicketWorld.Infrastructure.Persistence.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;

public class SqlScriptSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SqlScriptSeeder> _logger;

    public SqlScriptSeeder(
        ApplicationDbContext context,
        ILogger<SqlScriptSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Executes custom PostgreSQL extensions, indexes, or initial seed scripts if present
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";", cancellationToken);
            _logger.LogInformation("Verified PostgreSQL uuid-ossp extension.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Note: Could not run CREATE EXTENSION uuid-ossp (may require superuser or already enabled).");
        }
    }
}
