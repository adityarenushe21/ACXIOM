using Microsoft.AspNetCore.Antiforgery;
using Microsoft.OpenApi.Models;
using AcxiomCRM.Api.Middleware;
using AcxiomCRM.Api.Services;
using AcxiomCRM.Application;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Infrastructure;
using AcxiomCRM.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Layer Dependencies
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Register Web/API Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Critical for cookie authentication
    });
});

// 4. Configure Anti-forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = ".AcxiomCRM.Antiforgery";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AcxiomCRM API",
        Version = "v1",
        Description = "Modular ASP.NET Core CRM Web API backend with Cookie Authentication and Role-based Authorization."
    });
});

var app = builder.Build();

// 6. Seed Default Users & Roles in Development / Staging
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await ApplicationDbContextSeed.SeedDefaultUsersAsync(services, app.Configuration, app.Environment);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding default roles and users.");
    }
}

// 7. Configure Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AcxiomCRM API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

// CORS must be after UseRouting and before UseAuthentication
app.UseCors("SpaCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
