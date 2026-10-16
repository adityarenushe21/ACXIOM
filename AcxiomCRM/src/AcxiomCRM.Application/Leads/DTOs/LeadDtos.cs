using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.Leads.DTOs;

public class LeadDto
{
    public Guid LeadId { get; set; }
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Source { get; set; }
    public LeadStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public decimal ExpectedValue { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class CreateLeadRequest
{
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Source { get; set; }
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public decimal ExpectedValue { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateLeadStatusRequest
{
    public LeadStatus Status { get; set; }
}
