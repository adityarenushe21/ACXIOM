using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AcxiomCRM.Domain.Constants;
using AcxiomCRM.Infrastructure.Identity;

namespace AcxiomCRM.Infrastructure.Persistence.Seed;

public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider, IConfiguration configuration, IHostEnvironment environment)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ApplicationDbContextSeed");
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Seed Roles
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeded role: {Role}", role);
            }
        }

        var isInMemory = configuration["DatabaseProvider"]?.Equals("InMemory", StringComparison.OrdinalIgnoreCase) ?? false;
        if (environment.IsProduction() && !isInMemory)
        {
            var adminEnvPass = configuration["SEED_ADMIN_PASSWORD"];
            if (string.IsNullOrWhiteSpace(adminEnvPass))
            {
                logger.LogWarning("Skipping automatic user seeding in Production: SEED_ADMIN_PASSWORD not set.");
                return;
            }
        }

        var defaultPassword = configuration["SEED_DEFAULT_PASSWORD"] ?? "CrmDev@2024Secure!";
        var adminPassword = configuration["SEED_ADMIN_PASSWORD"] ?? defaultPassword;
        var managerPassword = configuration["SEED_MANAGER_PASSWORD"] ?? defaultPassword;
        var salesPassword = configuration["SEED_SALES_PASSWORD"] ?? defaultPassword;

        // 3. Seed Admin
        var adminEmail = configuration["SEED_ADMIN_EMAIL"] ?? "admin@acxiomcrm.local";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                logger.LogInformation("Seeded admin user: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to seed admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 4. Seed Manager
        var managerEmail = configuration["SEED_MANAGER_EMAIL"] ?? "manager@acxiomcrm.local";
        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var managerUser = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                FirstName = "Sales",
                LastName = "Manager",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(managerUser, managerPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, Roles.Manager);
                logger.LogInformation("Seeded manager user: {Email}", managerEmail);
            }
        }

        // 5. Seed Sales Executive
        var salesEmail = configuration["SEED_SALES_EMAIL"] ?? "sales@acxiomcrm.local";
        if (await userManager.FindByEmailAsync(salesEmail) == null)
        {
            var salesUser = new ApplicationUser
            {
                UserName = salesEmail,
                Email = salesEmail,
                FirstName = "Alex",
                LastName = "Executive",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(salesUser, salesPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(salesUser, Roles.SalesExecutive);
                logger.LogInformation("Seeded sales executive user: {Email}", salesEmail);
            }
        }
    }
}
