using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AcxiomCRM.Application.Auth.DTOs;
using AcxiomCRM.Application.Auth.Interfaces;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Infrastructure.Identity;

namespace AcxiomCRM.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<UserDto>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed for non-existent user: {Email}", request.Email);
            await _auditService.LogAsync(null, "FailedLogin", "User", request.Email, null, "User not found", ipAddress, cancellationToken);
            return Result<UserDto>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive account: {Email}", request.Email);
            await _auditService.LogAsync(user.Id, "FailedLogin", "User", user.Id, null, "Account inactive", ipAddress, cancellationToken);
            return Result<UserDto>.Failure("Your account has been deactivated. Please contact an administrator.");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Account locked out: {Email}", request.Email);
            await _auditService.LogAsync(user.Id, "AccountLocked", "User", user.Id, null, "Lockout triggered", ipAddress, cancellationToken);
            return Result<UserDto>.Failure("Account is locked due to multiple failed login attempts. Please try again later.");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Invalid password for user: {Email}", request.Email);
            await _auditService.LogAsync(user.Id, "FailedLogin", "User", user.Id, null, "Invalid credentials", ipAddress, cancellationToken);
            return Result<UserDto>.Failure("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        await _auditService.LogAsync(user.Id, "LoginSuccess", "User", user.Id, null, "Session created", ipAddress, cancellationToken);

        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        };

        return Result<UserDto>.Success(dto);
    }

    public async Task LogoutAsync(string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        await _signInManager.SignOutAsync();
        if (userId != null)
        {
            await _auditService.LogAsync(userId, "Logout", "User", userId, null, "Session ended", ipAddress, cancellationToken);
        }
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Result<UserDto>.Failure("Not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return Result<UserDto>.Failure("User account is invalid or deactivated.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = roles.ToList()
        };

        return Result<UserDto>.Success(dto);
    }
}
