using FluentAssertions;
using AcxiomCRM.Application.Opportunities.DTOs;
using AcxiomCRM.Application.Opportunities.Validators;
using Xunit;

namespace AcxiomCRM.UnitTests.Validators;

public class OpportunityAndFollowUpValidatorsTests
{
    private readonly CreateOpportunityRequestValidator _oppValidator = new();

    [Fact]
    public void CreateOpportunity_WithNegativeAmount_ShouldFail()
    {
        var model = new CreateOpportunityRequest
        {
            OpportunityName = "Enterprise Cloud License",
            CustomerId = Guid.NewGuid(),
            Amount = -500,
            Probability = 50,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(30)
        };

        var result = _oppValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOpportunityRequest.Amount));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(105)]
    public void CreateOpportunity_WithInvalidProbability_ShouldFail(int prob)
    {
        var model = new CreateOpportunityRequest
        {
            OpportunityName = "Enterprise Cloud License",
            CustomerId = Guid.NewGuid(),
            Amount = 10000,
            Probability = prob,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(30)
        };

        var result = _oppValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOpportunityRequest.Probability));
    }

    [Fact]
    public void CreateOpportunity_WithPastCloseDate_ShouldFail()
    {
        var model = new CreateOpportunityRequest
        {
            OpportunityName = "Enterprise Cloud License",
            CustomerId = Guid.NewGuid(),
            Amount = 10000,
            Probability = 50,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(-10)
        };

        var result = _oppValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOpportunityRequest.ExpectedCloseDate));
    }
}
