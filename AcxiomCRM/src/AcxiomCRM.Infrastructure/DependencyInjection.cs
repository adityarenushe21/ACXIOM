using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AcxiomCRM.Application.Auth.Interfaces;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Application.Customers.Interfaces;
using AcxiomCRM.Application.Dashboard.Interfaces;
using AcxiomCRM.Application.FollowUps.Interfaces;
using AcxiomCRM.Application.Leads.Interfaces;
using AcxiomCRM.Application.Opportunities.Interfaces;
using AcxiomCRM.Application.Reports.Interfaces;
using AcxiomCRM.Application.Users.DTOs;
using AcxiomCRM.Infrastructure.Auditing;
using AcxiomCRM.Infrastructure.Identity;
using AcxiomCRM.Infrastructure.Persistence;
using AcxiomCRM.Infrastructure.Services;

namespace AcxiomCRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost,1433;Database=AcxiomCrmDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        var dbProvider = configuration["DatabaseProvider"] ?? "SqlServer";
        if (dbProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AcxiomCrmDevDb"));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password policy
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User policy
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure Application Cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".AcxiomCRM.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax; // Lax allows cross-origin localhost top-level navigation while supporting CORS fetch with credentials
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allows HTTP during local development, HTTPS in prod
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            // Return 401 and 403 instead of 302 redirect for Web API
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // Register application services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IFollowUpService, FollowUpService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
