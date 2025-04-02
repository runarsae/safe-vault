using System.Text.RegularExpressions;

namespace SafeVault;

public static class ValidationHelpers
{
    // Validates input for malicious patterns (e.g., SQL injection, XSS)
    public static bool IsValidInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        // Check for SQL injection patterns
        if (Regex.IsMatch(input, @"'|;|--|/\*|\*/|xp_"))
        {
            return false;
        }

        // Check for HTML/JS tags to prevent XSS
        if (Regex.IsMatch(input, @"<.*?>"))
        {
            return false;
        }

        return true;
    }
}