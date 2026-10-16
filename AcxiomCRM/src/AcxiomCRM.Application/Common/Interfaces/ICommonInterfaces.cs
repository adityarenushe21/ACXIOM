namespace AcxiomCRM.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface IAuditService
{
    Task LogAsync(string? userId, string action, string entityName, string recordId, string? oldValue, string? newValue, string? ipAddress = null, CancellationToken cancellationToken = default);
}
