using FluentValidation;
using AcxiomCRM.Application.Opportunities.DTOs;

namespace AcxiomCRM.Application.Opportunities.Validators;

public class CreateOpportunityRequestValidator : AbstractValidator<CreateOpportunityRequest>
{
    public CreateOpportunityRequestValidator()
    {
        RuleFor(x => x.OpportunityName)
            .NotEmpty().WithMessage("Opportunity name is required.")
            .MaximumLength(150).WithMessage("Opportunity name cannot exceed 150 characters.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Valid Customer ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero for active opportunities.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100 inclusive.");

        RuleFor(x => x.ExpectedCloseDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Expected close date cannot be in the past for an active opportunity.");
    }
}
