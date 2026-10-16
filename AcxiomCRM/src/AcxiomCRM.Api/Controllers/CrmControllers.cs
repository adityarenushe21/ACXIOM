using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using AcxiomCRM.Application.Common.Pagination;
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
using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService) => _leadService = leadService;

    [HttpGet]
    public async Task<IActionResult> GetLeads([FromQuery] PaginationQuery query, [FromQuery] LeadStatus? status, CancellationToken ct)
        => Ok(await _leadService.GetLeadsAsync(query, status, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLeadById(Guid id, CancellationToken ct)
    {
        var res = await _leadService.GetLeadByIdAsync(id, ct);
        return Ok(res.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken ct)
    {
        var res = await _leadService.CreateLeadAsync(request, ct);
        return CreatedAtAction(nameof(GetLeadById), new { id = res.Data!.LeadId }, res.Data);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateLeadStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, CancellationToken ct)
    {
        var res = await _leadService.UpdateLeadStatusAsync(id, request, ct);
        return Ok(res.Data);
    }
}

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _oppService;
    private readonly IValidator<CreateOpportunityRequest> _createValidator;

    public OpportunitiesController(IOpportunityService oppService, IValidator<CreateOpportunityRequest> createValidator)
    {
        _oppService = oppService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOpportunities([FromQuery] PaginationQuery query, [FromQuery] OpportunityStage? stage, CancellationToken ct)
        => Ok(await _oppService.GetOpportunitiesAsync(query, stage, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOpportunityById(Guid id, CancellationToken ct)
    {
        var res = await _oppService.GetOpportunityByIdAsync(id, ct);
        return Ok(res.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityRequest request, CancellationToken ct)
    {
        var val = await _createValidator.ValidateAsync(request, ct);
        if (!val.IsValid)
        {
            var errors = val.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var res = await _oppService.CreateOpportunityAsync(request, ct);
        return CreatedAtAction(nameof(GetOpportunityById), new { id = res.Data!.OpportunityId }, res.Data);
    }

    [HttpPatch("{id:guid}/stage")]
    public async Task<IActionResult> UpdateOpportunityStage(Guid id, [FromBody] UpdateOpportunityStageRequest request, CancellationToken ct)
    {
        var res = await _oppService.UpdateOpportunityStageAsync(id, request, ct);
        return Ok(res.Data);
    }
}

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class FollowUpsController : ControllerBase
{
    private readonly IFollowUpService _followUpService;
    private readonly IValidator<CreateFollowUpRequest> _createValidator;

    public FollowUpsController(IFollowUpService followUpService, IValidator<CreateFollowUpRequest> createValidator)
    {
        _followUpService = followUpService;
        _createValidator = createValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetFollowUps([FromQuery] PaginationQuery query, [FromQuery] FollowUpStatus? status, CancellationToken ct)
        => Ok(await _followUpService.GetFollowUpsAsync(query, status, ct));

    [HttpPost]
    public async Task<IActionResult> CreateFollowUp([FromBody] CreateFollowUpRequest request, CancellationToken ct)
    {
        var val = await _createValidator.ValidateAsync(request, ct);
        if (!val.IsValid)
        {
            var errors = val.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var res = await _followUpService.CreateFollowUpAsync(request, ct);
        return Ok(res.Data);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteFollowUp(Guid id, [FromBody] CompleteFollowUpRequest request, CancellationToken ct)
    {
        var res = await _followUpService.CompleteFollowUpAsync(id, request, ct);
        return Ok(res.Data);
    }

    [HttpPatch("{id:guid}/reschedule")]
    public async Task<IActionResult> RescheduleFollowUp(Guid id, [FromBody] RescheduleFollowUpRequest request, CancellationToken ct)
    {
        var res = await _followUpService.RescheduleFollowUpAsync(id, request, ct);
        return Ok(res.Data);
    }
}

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var res = await _dashboardService.GetSummaryAsync(ct);
        return Ok(res.Data);
    }

    [HttpGet("lead-status")]
    public async Task<IActionResult> GetLeadStatusDistribution(CancellationToken ct)
    {
        var res = await _dashboardService.GetLeadStatusDistributionAsync(ct);
        return Ok(res.Data);
    }

    [HttpGet("opportunity-pipeline")]
    public async Task<IActionResult> GetOpportunityPipeline(CancellationToken ct)
    {
        var res = await _dashboardService.GetOpportunityPipelineAsync(ct);
        return Ok(res.Data);
    }
}

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomerReport([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _reportService.GetCustomerReportAsync(query, ct));

    [HttpGet("leads")]
    public async Task<IActionResult> GetLeadReport([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _reportService.GetLeadReportAsync(query, ct));

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipelineReport([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _reportService.GetPipelineReportAsync(query, ct));

    [HttpGet("audit")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAuditReport([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _reportService.GetAuditReportAsync(query, ct));
}

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationQuery query, CancellationToken ct)
        => Ok(await _userService.GetUsersAsync(query, ct));

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var res = await _userService.CreateUserAsync(request, ct);
        if (!res.Succeeded)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = res.Error });
        return Ok(res.Data);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        await _userService.UpdateUserStatusAsync(id, request, ct);
        return NoContent();
    }

    [HttpPatch("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleRequest request, CancellationToken ct)
    {
        var res = await _userService.UpdateUserRoleAsync(id, request, ct);
        if (!res.Succeeded)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Detail = res.Error });
        return NoContent();
    }
}
