using FluentValidation;
using AcxiomCRM.Application.Customers.DTOs;

namespace AcxiomCRM.Application.Customers.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerCode)
            .NotEmpty().WithMessage("Customer code is required.")
            .MaximumLength(30).WithMessage("Customer code cannot exceed 30 characters.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Customer code can only contain alphanumeric characters, hyphens, and underscores.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[1-9]\d{1,14}$|^[0-9]{7,15}$").WithMessage("Phone must be a valid format with 7 to 15 digits.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(150).WithMessage("Company name cannot exceed 150 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters.");
    }
}

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters.")
            .Matches(@"^\+?[1-9]\d{1,14}$|^[0-9]{7,15}$").WithMessage("Phone must be a valid format with 7 to 15 digits.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(150).WithMessage("Company name cannot exceed 150 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(250).WithMessage("Address cannot exceed 250 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State cannot exceed 100 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid customer status.");
    }
}
