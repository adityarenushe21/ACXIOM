using Microsoft.Extensions.Logging;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Domain.Entities;
using AcxiomCRM.Infrastructure.Persistence;

namespace AcxiomCRM.Infrastructure.Auditing;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(
        string? userId,
        string action,
        string entityName,
        string recordId,
        string? oldValue,
        string? newValue,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                RecordId = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedDate = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit event recorded: {Action} on {EntityName} ({RecordId}) by user {UserId}",
                action, entityName, recordId, userId ?? "Anonymous");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for {Action} on {EntityName} ({RecordId})",
                action, entityName, recordId);
        }
    }
}
