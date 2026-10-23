using CarePath.Data;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Tests.Infrastructure;

/// <summary>
/// SQLite cannot order or aggregate decimal columns server-side, so the test model stores them as REAL.
/// </summary>
public sealed class SqliteApplicationDbContext : ApplicationDbContext
{
    public SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }
}
