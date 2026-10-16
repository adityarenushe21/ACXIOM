using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Domain.Entities;

public class Customer
{
    public Guid CustomerId { get; set; } = Guid.NewGuid();
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;

    // Navigation
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public class Lead
{
    public Guid LeadId { get; set; } = Guid.NewGuid();
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Source { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public decimal ExpectedValue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;

    // Navigation
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public class Opportunity
{
    public Guid OpportunityId { get; set; } = Guid.NewGuid();
    public string OpportunityName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public decimal Amount { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    public int Probability { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;

    public decimal WeightedValue => (Amount * Probability) / 100m;
}

public class FollowUp
{
    public Guid FollowUpId { get; set; } = Guid.NewGuid();
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public DateTime FollowUpDate { get; set; }
    public FollowUpType FollowUpType { get; set; } = FollowUpType.Call;
    public string Subject { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
}

public class Activity
{
    public Guid ActivityId { get; set; } = Guid.NewGuid();
    public ActivityType ActivityType { get; set; } = ActivityType.Note;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
}

public class AuditLog
{
    public Guid AuditLogId { get; set; } = Guid.NewGuid();
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
