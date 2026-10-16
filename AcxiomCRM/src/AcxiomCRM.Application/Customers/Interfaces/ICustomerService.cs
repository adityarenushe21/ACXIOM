using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Customers.DTOs;
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.Customers.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(PaginationQuery query, CustomerStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> GetCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomerDto>> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken = default);
}
