namespace AcxiomCRM.Domain.Enums;

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Lead = 3,
    Archived = 4
}

public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Unqualified = 4,
    Converted = 5
}

public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum OpportunityStage
{
    Prospecting = 1,
    Qualification = 2,
    Proposal = 3,
    Negotiation = 4,
    ClosedWon = 5,
    ClosedLost = 6
}

public enum OpportunityStatus
{
    Open = 1,
    Won = 2,
    Lost = 3,
    Suspended = 4
}

public enum FollowUpType
{
    Call = 1,
    Email = 2,
    Meeting = 3,
    Task = 4
}

public enum FollowUpStatus
{
    Pending = 1,
    Completed = 2,
    Rescheduled = 3,
    Cancelled = 4
}

public enum ActivityType
{
    Call = 1,
    Email = 2,
    Meeting = 3,
    Note = 4,
    Task = 5
}

public enum AuditAction
{
    Login = 1,
    FailedLogin = 2,
    Logout = 3,
    Create = 4,
    Update = 5,
    Delete = 6,
    StatusChange = 7,
    RoleChange = 8
}
