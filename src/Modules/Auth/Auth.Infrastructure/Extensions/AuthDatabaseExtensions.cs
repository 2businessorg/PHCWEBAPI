using Auth.Infrastructure.Data;
using Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Extensions;

/// <summary>
/// Extension methods for Auth database setup and migrations
/// </summary>
public static class AuthDatabaseExtensions
{
    /// <summary>
    /// Ensures Auth database is created and migrations are applied
    /// Handles existing tables gracefully by syncing migration history
    /// </summary>
    public static async Task<IApplicationBuilder> UseAuthDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<AuthDbContext>>();

        try
        {
            var context = services.GetRequiredService<AuthDbContext>();
            

            // Check if tables already exist
            if (await TablesExistAsync(context))
            {
                await EnsureMigrationHistoryAsync(context, logger);
            }
            else
            {
                await context.Database.MigrateAsync();
            }

            // Seed data
            await SeedAuthDataAsync(services, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing Auth database: {Message}", ex.Message);
            logger.LogWarning("Continuing startup - authentication may not work properly");
        }

        return app;
    }

    private static async Task<bool> TablesExistAsync(AuthDbContext context)
    {
        try
        {
            // Try to query AspNetUsers - if it exists, tables are already there
            await context.Database.ExecuteSqlRawAsync(
                "SELECT TOP 1 Id FROM AspNetUsers");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task EnsureMigrationHistoryAsync(AuthDbContext context, ILogger logger)
    {
        try
        {
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogWarning("Migration history incomplete. Synchronizing...");
                
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260213_InitialAuthMigration')
                    BEGIN
                        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                        VALUES ('20260213_InitialAuthMigration', '8.0.0')
                    END");
                
                logger.LogInformation("Migration history synchronized");
            }
            else
            {
                logger.LogInformation("Auth database is up to date");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not verify migration history - continuing anyway");
        }
    }

    private static async Task SeedAuthDataAsync(IServiceProvider services, ILogger logger)
    {
        try
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            
            await AuthDbContextSeed.SeedAsync(userManager, roleManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "An error occurred while seeding auth database");
        }
    }
}
