using FluentValidation;
using AcxiomCRM.Application.FollowUps.DTOs;

namespace AcxiomCRM.Application.FollowUps.Validators;

public class CreateFollowUpRequestValidator : AbstractValidator<CreateFollowUpRequest>
{
    public CreateFollowUpRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.FollowUpDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddMinutes(-5))
            .WithMessage("Follow-up date cannot be earlier than today for a new planned follow-up.");

        RuleFor(x => x)
            .Must(x => x.CustomerId.HasValue || x.LeadId.HasValue)
            .WithMessage("Follow-up must be associated with either a customer or a lead.");
    }
}
