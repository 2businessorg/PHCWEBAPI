using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Authorization;

namespace Auth.Infrastructure.Data;

/// <summary>
/// Seeds initial data for authentication module
/// Creates default roles and admin user
/// </summary>
public static class AuthDbContextSeed
{
    /// <summary>
    /// Seeds roles and admin user if they don't exist
    /// </summary>
    public static async Task SeedAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        try
        {
            // Seed Roles
            await SeedRolesAsync(roleManager, logger);

            // Seed Admin User
            await SeedAdminUserAsync(userManager, logger);

            logger.LogInformation("Auth database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the auth database");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        foreach (var roleName in AppRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogWarning("Failed to create role {RoleName}: {Errors}",
                        roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager, ILogger logger)
    {
        const string adminUsername = "admin";
        const string adminEmail = "admin@phcapi.local";
        const string adminPassword = "YOUR_PASSWORD"; // 12+ chars to satisfy Identity policy. Change in production!

        var existingUser = await userManager.FindByNameAsync(adminUsername);
        if (existingUser != null)
        {
            logger.LogInformation("Admin user already exists");
            return;
        }

        var adminUser = new IdentityUser
        {
            UserName = adminUsername,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (createResult.Succeeded)
        {
            logger.LogInformation("Admin user created successfully");

            // Add to Administrator role
            var roleResult = await userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Admin user added to Administrator role");
            }
            else
            {
                logger.LogWarning("Failed to add admin to Administrator role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogWarning("Failed to create admin user: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }
}
