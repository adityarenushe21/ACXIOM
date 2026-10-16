using AcxiomCRM.Application.Auth.DTOs;
using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;

namespace AcxiomCRM.Application.Users.DTOs;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserStatusAsync(string id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserRoleAsync(string id, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
}
