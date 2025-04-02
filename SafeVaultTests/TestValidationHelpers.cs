using SafeVault;

namespace SafeVaultTests;

public class TestValidationHelpers
{
    [Fact]
    public void TestForSQLInjection()
    {
        // Placeholder for SQL Injection test
        string maliciousInput = "'; DROP TABLE Users; --";
        bool isValid = ValidationHelpers.IsValidInput(maliciousInput);
        Assert.False(isValid, "Input validation failed to detect SQL injection.");
    }

    [Fact]
    public void TestForXSS()
    {
        // Placeholder for XSS test
        string maliciousInput = "<script>alert('XSS');</script>";
        bool isValid = ValidationHelpers.IsValidInput(maliciousInput);
        Assert.False(isValid, "Input validation failed to detect XSS.");
    }
}