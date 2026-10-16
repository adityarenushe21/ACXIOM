using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Opportunities.DTOs;
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.Opportunities.Interfaces;

public interface IOpportunityService
{
    Task<PagedResult<OpportunityDto>> GetOpportunitiesAsync(PaginationQuery query, OpportunityStage? stage = null, CancellationToken cancellationToken = default);
    Task<Result<OpportunityDto>> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<OpportunityDto>> CreateOpportunityAsync(CreateOpportunityRequest request, CancellationToken cancellationToken = default);
    Task<Result<OpportunityDto>> UpdateOpportunityStageAsync(Guid id, UpdateOpportunityStageRequest request, CancellationToken cancellationToken = default);
}
