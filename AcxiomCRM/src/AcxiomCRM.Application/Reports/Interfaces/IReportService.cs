using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Customers.DTOs;
using AcxiomCRM.Application.Leads.DTOs;
using AcxiomCRM.Application.Opportunities.DTOs;
using AcxiomCRM.Domain.Entities;

namespace AcxiomCRM.Application.Reports.Interfaces;

public interface IReportService
{
    Task<PagedResult<CustomerDto>> GetCustomerReportAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<LeadDto>> GetLeadReportAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<OpportunityDto>> GetPipelineReportAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLog>> GetAuditReportAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}
