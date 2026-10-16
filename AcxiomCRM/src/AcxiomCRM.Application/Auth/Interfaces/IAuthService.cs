using AcxiomCRM.Application.Auth.DTOs;
using AcxiomCRM.Application.Common.Models;

namespace AcxiomCRM.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<UserDto>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogoutAsync(string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
