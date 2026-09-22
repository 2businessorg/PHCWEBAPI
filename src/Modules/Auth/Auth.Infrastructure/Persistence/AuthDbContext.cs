using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

/// <summary>
/// Authentication DbContext using ASP.NET Identity
/// This context is isolated from other modules following the modular architecture
/// </summary>
public class AuthDbContext : IdentityDbContext<IdentityUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Customize table names if needed
        // Keep ASP.NET Identity default table names for compatibility
        
        // Configure indexes for performance
        builder.Entity<IdentityUser>()
            .HasIndex(u => u.NormalizedUserName)
            .IsUnique()
            .HasFilter("[NormalizedUserName] IS NOT NULL");
            
        builder.Entity<IdentityUser>()
            .HasIndex(u => u.NormalizedEmail);
    }
}
