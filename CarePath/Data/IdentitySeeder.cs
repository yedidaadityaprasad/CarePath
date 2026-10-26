using CarePath.Data;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Data;

public class SeedAdminOptions
{
    public const string SectionName = "SeedAdmin";

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = "CarePath Administrator";
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Creates the application roles and the initial Admin account at startup.
/// .NET 10: Uses IServiceProvider overload compatible with the minimal hosting model.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("CarePath.IdentitySeeder");
        var configuration = services.GetRequiredService<IConfiguration>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        if (configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
        {
            await db.Database.MigrateAsync();
        }

        if (!await db.Database.CanConnectAsync())
        {
            logger.LogWarning("Cannot connect to the database; skipping role and admin seeding. Run 'dotnet ef database update' first.");
            return;
        }

        if ((await db.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogWarning("The database has pending migrations; skipping seeding. Run 'dotnet ef database update'.");
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Could not create role {role}: {string.Join("; ", result.Errors.Select(e => e.Description))}");
                }
                logger.LogInformation("Created role {Role}", role);
            }
        }

        var options = configuration.GetSection(SeedAdminOptions.SectionName).Get<SeedAdminOptions>()
                      ?? new SeedAdminOptions();

        if (string.IsNullOrWhiteSpace(options.Email))
        {
            logger.LogWarning("SeedAdmin:Email is not configured; no Admin user was seeded.");
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(options.Email);
        if (admin == null)
        {
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                logger.LogWarning(
                    "SeedAdmin:Password is not configured; set it with 'dotnet user-secrets set SeedAdmin:Password <value>' to create {Email}.",
                    options.Email);
                return;
            }

            // .NET 10: TimeProvider.System used for consistent UTC-aware timestamps
            admin = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true,
                FullName = options.FullName,
                IsActive = true,
                CreatedAt = TimeProvider.System.GetLocalNow().DateTime
            };

            var created = await userManager.CreateAsync(admin, options.Password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create admin user: {string.Join("; ", created.Errors.Select(e => e.Description))}");
            }
            logger.LogInformation("Created admin user {Email}", options.Email);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}
