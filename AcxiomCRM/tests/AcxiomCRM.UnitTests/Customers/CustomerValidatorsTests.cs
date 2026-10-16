using FluentAssertions;
using AcxiomCRM.Application.Customers.DTOs;
using AcxiomCRM.Application.Customers.Validators;
using Xunit;

namespace AcxiomCRM.UnitTests.Validators;

public class CustomerValidatorsTests
{
    private readonly CreateCustomerRequestValidator _createValidator = new();

    [Fact]
    public void CreateCustomer_WithValidFields_ShouldPassValidation()
    {
        var model = new CreateCustomerRequest
        {
            CustomerCode = "CUST-001",
            CustomerName = "Acme Global Solutions",
            Email = "contact@acme.com",
            Phone = "+15551234567",
            CompanyName = "Acme Corp",
            Address = "123 Market St",
            City = "San Francisco",
            State = "CA"
        };

        var result = _createValidator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Customer name is required.")]
    public void CreateCustomer_WithEmptyName_ShouldFailValidation(string name, string expectedError)
    {
        var model = new CreateCustomerRequest
        {
            CustomerCode = "CUST-001",
            CustomerName = name,
            Email = "contact@acme.com",
            Phone = "+15551234567"
        };

        var result = _createValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedError));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("invalid@")]
    public void CreateCustomer_WithInvalidEmail_ShouldFailValidation(string email)
    {
        var model = new CreateCustomerRequest
        {
            CustomerCode = "CUST-001",
            CustomerName = "Valid Corp",
            Email = email,
            Phone = "+15551234567"
        };

        var result = _createValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerRequest.Email));
    }

    [Fact]
    public void CreateCustomer_WithInvalidCustomerCodeCharacters_ShouldFailValidation()
    {
        var model = new CreateCustomerRequest
        {
            CustomerCode = "CUST 001 *&^%",
            CustomerName = "Valid Corp",
            Email = "valid@corp.com",
            Phone = "+15551234567"
        };

        var result = _createValidator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCustomerRequest.CustomerCode));
    }
}
