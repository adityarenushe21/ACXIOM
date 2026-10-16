using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Dashboard.DTOs;

namespace AcxiomCRM.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StatusDistributionItemDto>>> GetLeadStatusDistributionAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<StagePipelineItemDto>>> GetOpportunityPipelineAsync(CancellationToken cancellationToken = default);
}
