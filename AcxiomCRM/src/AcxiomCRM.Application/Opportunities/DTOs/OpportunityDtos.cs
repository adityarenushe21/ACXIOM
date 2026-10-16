using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.Opportunities.DTOs;

public class OpportunityDto
{
    public Guid OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public decimal Amount { get; set; }
    public OpportunityStage Stage { get; set; }
    public int Probability { get; set; }
    public decimal WeightedValue { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public OpportunityStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class CreateOpportunityRequest
{
    public string OpportunityName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public decimal Amount { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public string? AssignedTo { get; set; }
}

public class UpdateOpportunityStageRequest
{
    public OpportunityStage Stage { get; set; }
    public int? Probability { get; set; }
}
