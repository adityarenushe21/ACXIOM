namespace AcxiomCRM.Application.Auth.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
}

public class CsrfTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";
}
