namespace AcxiomCRM.Application.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalCustomers { get; set; }
    public int TotalLeads { get; set; }
    public int OpenLeads { get; set; }
    public int TotalOpportunities { get; set; }
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public decimal WeightedPipelineValue { get; set; }
}

public class StatusDistributionItemDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class StagePipelineItemDto
{
    public string Stage { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}
