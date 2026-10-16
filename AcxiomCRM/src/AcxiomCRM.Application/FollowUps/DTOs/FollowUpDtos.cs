using AcxiomCRM.Domain.Enums;

namespace AcxiomCRM.Application.FollowUps.DTOs;

public class FollowUpDto
{
    public Guid FollowUpId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public FollowUpType FollowUpType { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public FollowUpStatus Status { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class CreateFollowUpRequest
{
    public Guid? CustomerId { get; set; }
    public Guid? LeadId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public FollowUpType FollowUpType { get; set; } = FollowUpType.Call;
    public string Subject { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? AssignedTo { get; set; }
}

public class CompleteFollowUpRequest
{
    public string? Remarks { get; set; }
}

public class RescheduleFollowUpRequest
{
    public DateTime NewFollowUpDate { get; set; }
    public string? Remarks { get; set; }
}
