using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AcxiomCRM.Application.Auth.DTOs;
using AcxiomCRM.Application.Common.Exceptions;
using AcxiomCRM.Application.Common.Interfaces;
using AcxiomCRM.Application.Common.Models;
using AcxiomCRM.Application.Common.Pagination;
using AcxiomCRM.Application.Customers.DTOs;
using AcxiomCRM.Application.Customers.Interfaces;
using AcxiomCRM.Application.Dashboard.DTOs;
using AcxiomCRM.Application.Dashboard.Interfaces;
using AcxiomCRM.Application.FollowUps.DTOs;
using AcxiomCRM.Application.FollowUps.Interfaces;
using AcxiomCRM.Application.Leads.DTOs;
using AcxiomCRM.Application.Leads.Interfaces;
using AcxiomCRM.Application.Opportunities.DTOs;
using AcxiomCRM.Application.Opportunities.Interfaces;
using AcxiomCRM.Application.Reports.Interfaces;
using AcxiomCRM.Application.Users.DTOs;
using AcxiomCRM.Domain.Constants;
using AcxiomCRM.Domain.Entities;
using AcxiomCRM.Domain.Enums;
using AcxiomCRM.Infrastructure.Identity;
using AcxiomCRM.Infrastructure.Persistence;

namespace AcxiomCRM.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public LeadService(ApplicationDbContext context, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    private IQueryable<Lead> ApplyScope(IQueryable<Lead> query)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.Admin) || _currentUserService.IsInRole(Roles.Manager))
            return query;
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
            return query.Where(l => l.AssignedTo == uid);
        return query.Where(_ => false);
    }

    public async Task<PagedResult<LeadDto>> GetLeadsAsync(PaginationQuery query, LeadStatus? status = null, CancellationToken cancellationToken = default)
    {
        var q = ApplyScope(_context.Leads.AsNoTracking());
        if (status.HasValue) q = q.Where(l => l.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(l => l.LeadName.ToLower().Contains(s) || l.LeadCode.ToLower().Contains(s) || l.Email.ToLower().Contains(s));
        }

        var count = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(l => l.CreatedDate)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(l => new LeadDto
            {
                LeadId = l.LeadId,
                LeadCode = l.LeadCode,
                LeadName = l.LeadName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Source = l.Source,
                Status = l.Status,
                Priority = l.Priority,
                ExpectedValue = l.ExpectedValue,
                CreatedDate = l.CreatedDate,
                AssignedTo = l.AssignedTo
            }).ToListAsync(cancellationToken);

        return new PagedResult<LeadDto>(items, count, query.Page, query.PageSize);
    }

    public async Task<Result<LeadDto>> GetLeadByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lead = await ApplyScope(_context.Leads.AsNoTracking()).FirstOrDefaultAsync(l => l.LeadId == id, cancellationToken);
        if (lead == null)
        {
            if (await _context.Leads.AnyAsync(l => l.LeadId == id, cancellationToken))
                throw new ForbiddenAccessException("You do not have access to view this lead.");
            throw new NotFoundException(nameof(Lead), id);
        }

        return Result<LeadDto>.Success(new LeadDto
        {
            LeadId = lead.LeadId,
            LeadCode = lead.LeadCode,
            LeadName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            Source = lead.Source,
            Status = lead.Status,
            Priority = lead.Priority,
            ExpectedValue = lead.ExpectedValue,
            CreatedDate = lead.CreatedDate,
            AssignedTo = lead.AssignedTo
        });
    }

    public async Task<Result<LeadDto>> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken = default)
    {
        if (await _context.Leads.AnyAsync(l => l.LeadCode == request.LeadCode, cancellationToken))
            throw new ConflictException($"Lead code '{request.LeadCode}' already exists.");

        var assigned = request.AssignedTo;
        var uid = _currentUserService.UserId ?? "SYSTEM";
        if (_currentUserService.IsInRole(Roles.SalesExecutive) || string.IsNullOrEmpty(assigned))
            assigned = uid;

        var lead = new Lead
        {
            LeadCode = request.LeadCode.Trim(),
            LeadName = request.LeadName.Trim(),
            Email = request.Email.Trim().ToLower(),
            Phone = request.Phone.Trim(),
            CompanyName = request.CompanyName?.Trim(),
            Source = request.Source?.Trim(),
            Priority = request.Priority,
            ExpectedValue = request.ExpectedValue,
            Status = LeadStatus.New,
            CreatedDate = DateTime.UtcNow,
            AssignedTo = assigned
        };

        _context.Leads.Add(lead);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "Create", nameof(Lead), lead.LeadId.ToString(), null, $"Created lead {lead.LeadCode}", cancellationToken: cancellationToken);

        return await GetLeadByIdAsync(lead.LeadId, cancellationToken);
    }

    public async Task<Result<LeadDto>> UpdateLeadStatusAsync(Guid id, UpdateLeadStatusRequest request, CancellationToken cancellationToken = default)
    {
        var lead = await _context.Leads.FirstOrDefaultAsync(l => l.LeadId == id, cancellationToken);
        if (lead == null) throw new NotFoundException(nameof(Lead), id);

        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && lead.AssignedTo != uid)
            throw new ForbiddenAccessException("You can only modify leads assigned to you.");

        var oldStatus = lead.Status.ToString();
        lead.Status = request.Status;
        lead.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(uid, "StatusChange", nameof(Lead), id.ToString(), oldStatus, request.Status.ToString(), cancellationToken: cancellationToken);
        return await GetLeadByIdAsync(id, cancellationToken);
    }
}

public class OpportunityService : IOpportunityService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public OpportunityService(ApplicationDbContext context, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    private IQueryable<Opportunity> ApplyScope(IQueryable<Opportunity> query)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.Admin) || _currentUserService.IsInRole(Roles.Manager))
            return query;
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
            return query.Where(o => o.AssignedTo == uid);
        return query.Where(_ => false);
    }

    public async Task<PagedResult<OpportunityDto>> GetOpportunitiesAsync(PaginationQuery query, OpportunityStage? stage = null, CancellationToken cancellationToken = default)
    {
        var q = ApplyScope(_context.Opportunities.AsNoTracking());
        if (stage.HasValue) q = q.Where(o => o.Stage == stage.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(o => o.OpportunityName.ToLower().Contains(s));
        }

        var count = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(o => o.CreatedDate)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(o => new OpportunityDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerId = o.CustomerId,
                LeadId = o.LeadId,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                WeightedValue = (o.Amount * o.Probability) / 100m,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                CreatedDate = o.CreatedDate,
                AssignedTo = o.AssignedTo
            }).ToListAsync(cancellationToken);

        return new PagedResult<OpportunityDto>(items, count, query.Page, query.PageSize);
    }

    public async Task<Result<OpportunityDto>> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var o = await ApplyScope(_context.Opportunities.AsNoTracking()).FirstOrDefaultAsync(x => x.OpportunityId == id, cancellationToken);
        if (o == null)
        {
            if (await _context.Opportunities.AnyAsync(x => x.OpportunityId == id, cancellationToken))
                throw new ForbiddenAccessException("You do not have permission to view this opportunity.");
            throw new NotFoundException(nameof(Opportunity), id);
        }

        return Result<OpportunityDto>.Success(new OpportunityDto
        {
            OpportunityId = o.OpportunityId,
            OpportunityName = o.OpportunityName,
            CustomerId = o.CustomerId,
            LeadId = o.LeadId,
            Amount = o.Amount,
            Stage = o.Stage,
            Probability = o.Probability,
            WeightedValue = (o.Amount * o.Probability) / 100m,
            ExpectedCloseDate = o.ExpectedCloseDate,
            Status = o.Status,
            CreatedDate = o.CreatedDate,
            AssignedTo = o.AssignedTo
        });
    }

    public async Task<Result<OpportunityDto>> CreateOpportunityAsync(CreateOpportunityRequest request, CancellationToken cancellationToken = default)
    {
        var custExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (!custExists) throw new NotFoundException(nameof(Customer), request.CustomerId);

        if (request.LeadId.HasValue)
        {
            var leadExists = await _context.Leads.AnyAsync(l => l.LeadId == request.LeadId.Value, cancellationToken);
            if (!leadExists) throw new NotFoundException(nameof(Lead), request.LeadId.Value);
        }

        var uid = _currentUserService.UserId ?? "SYSTEM";
        var assigned = request.AssignedTo;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) || string.IsNullOrEmpty(assigned))
            assigned = uid;

        var opp = new Opportunity
        {
            OpportunityName = request.OpportunityName.Trim(),
            CustomerId = request.CustomerId,
            LeadId = request.LeadId,
            Amount = request.Amount,
            Stage = request.Stage,
            Probability = request.Probability,
            ExpectedCloseDate = request.ExpectedCloseDate,
            Status = OpportunityStatus.Open,
            CreatedDate = DateTime.UtcNow,
            AssignedTo = assigned
        };

        _context.Opportunities.Add(opp);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "Create", nameof(Opportunity), opp.OpportunityId.ToString(), null, $"Created opportunity {opp.OpportunityName}", cancellationToken: cancellationToken);

        return await GetOpportunityByIdAsync(opp.OpportunityId, cancellationToken);
    }

    public async Task<Result<OpportunityDto>> UpdateOpportunityStageAsync(Guid id, UpdateOpportunityStageRequest request, CancellationToken cancellationToken = default)
    {
        var opp = await _context.Opportunities.FirstOrDefaultAsync(o => o.OpportunityId == id, cancellationToken);
        if (opp == null) throw new NotFoundException(nameof(Opportunity), id);

        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && opp.AssignedTo != uid)
            throw new ForbiddenAccessException("You can only modify opportunities assigned to you.");

        var oldStage = opp.Stage.ToString();
        opp.Stage = request.Stage;
        if (request.Probability.HasValue) opp.Probability = request.Probability.Value;

        if (request.Stage == OpportunityStage.ClosedWon) opp.Status = OpportunityStatus.Won;
        else if (request.Stage == OpportunityStage.ClosedLost) opp.Status = OpportunityStatus.Lost;

        opp.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "StageChange", nameof(Opportunity), id.ToString(), oldStage, request.Stage.ToString(), cancellationToken: cancellationToken);

        return await GetOpportunityByIdAsync(id, cancellationToken);
    }
}

public class FollowUpService : IFollowUpService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public FollowUpService(ApplicationDbContext context, ICurrentUserService currentUserService, IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    private IQueryable<FollowUp> ApplyScope(IQueryable<FollowUp> query)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.Admin) || _currentUserService.IsInRole(Roles.Manager))
            return query;
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
            return query.Where(f => f.AssignedTo == uid);
        return query.Where(_ => false);
    }

    public async Task<PagedResult<FollowUpDto>> GetFollowUpsAsync(PaginationQuery query, FollowUpStatus? status = null, CancellationToken cancellationToken = default)
    {
        var q = ApplyScope(_context.FollowUps.AsNoTracking());
        if (status.HasValue) q = q.Where(f => f.Status == status.Value);

        var count = await q.CountAsync(cancellationToken);
        var items = await q.OrderBy(f => f.FollowUpDate)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(f => new FollowUpDto
            {
                FollowUpId = f.FollowUpId,
                CustomerId = f.CustomerId,
                LeadId = f.LeadId,
                FollowUpDate = f.FollowUpDate,
                FollowUpType = f.FollowUpType,
                Subject = f.Subject,
                Remarks = f.Remarks,
                Status = f.Status,
                AssignedTo = f.AssignedTo,
                CreatedDate = f.CreatedDate,
                CompletedDate = f.CompletedDate
            }).ToListAsync(cancellationToken);

        return new PagedResult<FollowUpDto>(items, count, query.Page, query.PageSize);
    }

    public async Task<Result<FollowUpDto>> CreateFollowUpAsync(CreateFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        var uid = _currentUserService.UserId ?? "SYSTEM";
        var assigned = request.AssignedTo;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) || string.IsNullOrEmpty(assigned))
            assigned = uid;

        var followUp = new FollowUp
        {
            CustomerId = request.CustomerId,
            LeadId = request.LeadId,
            FollowUpDate = request.FollowUpDate,
            FollowUpType = request.FollowUpType,
            Subject = request.Subject.Trim(),
            Remarks = request.Remarks?.Trim(),
            Status = FollowUpStatus.Pending,
            AssignedTo = assigned,
            CreatedDate = DateTime.UtcNow
        };

        _context.FollowUps.Add(followUp);
        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "Create", nameof(FollowUp), followUp.FollowUpId.ToString(), null, $"Created followup: {followUp.Subject}", cancellationToken: cancellationToken);

        return Result<FollowUpDto>.Success(new FollowUpDto
        {
            FollowUpId = followUp.FollowUpId,
            CustomerId = followUp.CustomerId,
            LeadId = followUp.LeadId,
            FollowUpDate = followUp.FollowUpDate,
            FollowUpType = followUp.FollowUpType,
            Subject = followUp.Subject,
            Remarks = followUp.Remarks,
            Status = followUp.Status,
            AssignedTo = followUp.AssignedTo,
            CreatedDate = followUp.CreatedDate
        });
    }

    public async Task<Result<FollowUpDto>> CompleteFollowUpAsync(Guid id, CompleteFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        var f = await _context.FollowUps.FirstOrDefaultAsync(x => x.FollowUpId == id, cancellationToken);
        if (f == null) throw new NotFoundException(nameof(FollowUp), id);

        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && f.AssignedTo != uid)
            throw new ForbiddenAccessException("You can only complete your own follow-ups.");

        f.Status = FollowUpStatus.Completed;
        f.CompletedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Remarks))
            f.Remarks = string.IsNullOrEmpty(f.Remarks) ? request.Remarks : $"{f.Remarks} | Completed: {request.Remarks}";

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "Complete", nameof(FollowUp), id.ToString(), null, "Marked as Completed", cancellationToken: cancellationToken);

        return Result<FollowUpDto>.Success(new FollowUpDto
        {
            FollowUpId = f.FollowUpId,
            CustomerId = f.CustomerId,
            LeadId = f.LeadId,
            FollowUpDate = f.FollowUpDate,
            FollowUpType = f.FollowUpType,
            Subject = f.Subject,
            Remarks = f.Remarks,
            Status = f.Status,
            AssignedTo = f.AssignedTo,
            CreatedDate = f.CreatedDate,
            CompletedDate = f.CompletedDate
        });
    }

    public async Task<Result<FollowUpDto>> RescheduleFollowUpAsync(Guid id, RescheduleFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        var f = await _context.FollowUps.FirstOrDefaultAsync(x => x.FollowUpId == id, cancellationToken);
        if (f == null) throw new NotFoundException(nameof(FollowUp), id);

        var uid = _currentUserService.UserId ?? string.Empty;
        if (_currentUserService.IsInRole(Roles.SalesExecutive) && f.AssignedTo != uid)
            throw new ForbiddenAccessException("You can only reschedule your own follow-ups.");

        var oldDate = f.FollowUpDate.ToString("o");
        f.FollowUpDate = request.NewFollowUpDate;
        f.Status = FollowUpStatus.Rescheduled;
        if (!string.IsNullOrWhiteSpace(request.Remarks))
            f.Remarks = string.IsNullOrEmpty(f.Remarks) ? request.Remarks : $"{f.Remarks} | Rescheduled: {request.Remarks}";

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(uid, "Reschedule", nameof(FollowUp), id.ToString(), oldDate, request.NewFollowUpDate.ToString("o"), cancellationToken: cancellationToken);

        return Result<FollowUpDto>.Success(new FollowUpDto
        {
            FollowUpId = f.FollowUpId,
            CustomerId = f.CustomerId,
            LeadId = f.LeadId,
            FollowUpDate = f.FollowUpDate,
            FollowUpType = f.FollowUpType,
            Subject = f.Subject,
            Remarks = f.Remarks,
            Status = f.Status,
            AssignedTo = f.AssignedTo,
            CreatedDate = f.CreatedDate
        });
    }
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        var isRestricted = _currentUserService.IsInRole(Roles.SalesExecutive);

        var custQuery = _context.Customers.AsNoTracking();
        var leadQuery = _context.Leads.AsNoTracking();
        var oppQuery = _context.Opportunities.AsNoTracking();

        if (isRestricted)
        {
            custQuery = custQuery.Where(c => c.OwnerId == uid || c.CreatedBy == uid);
            leadQuery = leadQuery.Where(l => l.AssignedTo == uid);
            oppQuery = oppQuery.Where(o => o.AssignedTo == uid);
        }

        var totalCust = await custQuery.CountAsync(cancellationToken);
        var totalLeads = await leadQuery.CountAsync(cancellationToken);
        var openLeads = await leadQuery.CountAsync(l => l.Status == LeadStatus.New || l.Status == LeadStatus.Contacted || l.Status == LeadStatus.Qualified, cancellationToken);

        var totalOpp = await oppQuery.CountAsync(cancellationToken);
        var openOpp = await oppQuery.CountAsync(o => o.Status == OpportunityStatus.Open, cancellationToken);
        var wonOpp = await oppQuery.CountAsync(o => o.Status == OpportunityStatus.Won, cancellationToken);
        var lostOpp = await oppQuery.CountAsync(o => o.Status == OpportunityStatus.Lost, cancellationToken);

        var eligibleOpps = await oppQuery.Where(o => o.Status == OpportunityStatus.Open).ToListAsync(cancellationToken);
        var totalValue = eligibleOpps.Sum(o => o.Amount);
        var weightedValue = eligibleOpps.Sum(o => o.WeightedValue);

        return Result<DashboardSummaryDto>.Success(new DashboardSummaryDto
        {
            TotalCustomers = totalCust,
            TotalLeads = totalLeads,
            OpenLeads = openLeads,
            TotalOpportunities = totalOpp,
            OpenOpportunities = openOpp,
            WonOpportunities = wonOpp,
            LostOpportunities = lostOpp,
            TotalPipelineValue = totalValue,
            WeightedPipelineValue = weightedValue
        });
    }

    public async Task<Result<IReadOnlyList<StatusDistributionItemDto>>> GetLeadStatusDistributionAsync(CancellationToken cancellationToken = default)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        var query = _context.Leads.AsNoTracking();
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
            query = query.Where(l => l.AssignedTo == uid);

        var data = await query.GroupBy(l => l.Status)
            .Select(g => new StatusDistributionItemDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            }).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StatusDistributionItemDto>>.Success(data);
    }

    public async Task<Result<IReadOnlyList<StagePipelineItemDto>>> GetOpportunityPipelineAsync(CancellationToken cancellationToken = default)
    {
        var uid = _currentUserService.UserId ?? string.Empty;
        var query = _context.Opportunities.AsNoTracking().Where(o => o.Status == OpportunityStatus.Open);
        if (_currentUserService.IsInRole(Roles.SalesExecutive))
            query = query.Where(o => o.AssignedTo == uid);

        var data = await query.GroupBy(o => o.Stage)
            .Select(g => new StagePipelineItemDto
            {
                Stage = g.Key.ToString(),
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.Amount)
            }).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<StagePipelineItemDto>>.Success(data);
    }
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ICustomerService _customerService;
    private readonly ILeadService _leadService;
    private readonly IOpportunityService _opportunityService;

    public ReportService(
        ApplicationDbContext context,
        ICustomerService customerService,
        ILeadService leadService,
        IOpportunityService opportunityService)
    {
        _context = context;
        _customerService = customerService;
        _leadService = leadService;
        _opportunityService = opportunityService;
    }

    public Task<PagedResult<CustomerDto>> GetCustomerReportAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        => _customerService.GetCustomersAsync(query, null, cancellationToken);

    public Task<PagedResult<LeadDto>> GetLeadReportAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        => _leadService.GetLeadsAsync(query, null, cancellationToken);

    public Task<PagedResult<OpportunityDto>> GetPipelineReportAsync(PaginationQuery query, CancellationToken cancellationToken = default)
        => _opportunityService.GetOpportunitiesAsync(query, null, cancellationToken);

    public async Task<PagedResult<AuditLog>> GetAuditReportAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var q = _context.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(a => a.Action.ToLower().Contains(s) || a.EntityName.ToLower().Contains(s));
        }

        var count = await q.CountAsync(cancellationToken);
        var items = await q.OrderByDescending(a => a.CreatedDate)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>(items, count, query.Page, query.PageSize);
    }
}

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(u => u.Email!.ToLower().Contains(s) || u.FirstName.ToLower().Contains(s) || u.LastName.ToLower().Contains(s));
        }

        var count = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery.OrderBy(u => u.Email)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive,
                Roles = roles.ToList()
            });
        }

        return new PagedResult<UserDto>(dtos, count, query.Page, query.PageSize);
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            throw new ConflictException($"User with email '{request.Email}' already exists.");

        if (!Roles.All.Contains(request.Role))
            return Result<UserDto>.Failure($"Invalid role '{request.Role}'. Must be Admin, Manager, or SalesExecutive.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            EmailConfirmed = true
        };

        var res = await _userManager.CreateAsync(user, request.Password);
        if (!res.Succeeded)
            return Result<UserDto>.Failure(string.Join("; ", res.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, request.Role);
        await _auditService.LogAsync(_currentUserService.UserId, "Create", "User", user.Id, null, $"Created user with role {request.Role}", cancellationToken: cancellationToken);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            Roles = new[] { request.Role }
        });
    }

    public async Task<Result> UpdateUserStatusAsync(string id, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        var oldStatus = user.IsActive.ToString();
        user.IsActive = request.IsActive;
        await _userManager.UpdateAsync(user);

        await _auditService.LogAsync(_currentUserService.UserId, "StatusChange", "User", id, oldStatus, request.IsActive.ToString(), cancellationToken: cancellationToken);
        return Result.Success();
    }

    public async Task<Result> UpdateUserRoleAsync(string id, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        if (!Roles.All.Contains(request.Role))
            return Result.Failure($"Invalid role '{request.Role}'.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, request.Role);

        await _auditService.LogAsync(_currentUserService.UserId, "RoleChange", "User", id, string.Join(",", currentRoles), request.Role, cancellationToken: cancellationToken);
        return Result.Success();
    }
}
