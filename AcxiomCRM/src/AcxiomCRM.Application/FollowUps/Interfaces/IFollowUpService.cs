using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.FollowUps.DTOs;
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.FollowUps.Interfaces;

public interface IFollowUpService
{
    Task<PagedResult<FollowUpDto>> GetFollowUpsAsync(PaginationQuery query, FollowUpStatus? status = null, CancellationToken cancellationToken = default);
    Task<Result<FollowUpDto>> CreateFollowUpAsync(CreateFollowUpRequest request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpDto>> CompleteFollowUpAsync(Guid id, CompleteFollowUpRequest request, CancellationToken cancellationToken = default);
    Task<Result<FollowUpDto>> RescheduleFollowUpAsync(Guid id, RescheduleFollowUpRequest request, CancellationToken cancellationToken = default);
}
