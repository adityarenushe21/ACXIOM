using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Leads.DTOs;
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.Leads.Interfaces;

public interface ILeadService
{
    Task<PagedResult<LeadDto>> GetLeadsAsync(PaginationQuery query, LeadStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<LeadDto>> GetLeadByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<LeadDto>> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken = default);
    Task<Result<LeadDto>> UpdateLeadStatusAsync(Guid id, UpdateLeadStatusRequest request, CancellationToken cancellationToken = default);
}
