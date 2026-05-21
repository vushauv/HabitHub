using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace backend.Validation
{
    public class ValidEmailAttribute: ValidationAttribute
    {
        private static Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

        public ValidEmailAttribute()
        {
            ErrorMessage = "Invalid email format.";
        }
        public override bool IsValid(object? value)
        {
            string? email = value as string;
            if (email == null)
                return false;
            return EmailRegex.IsMatch(email.Trim());
        }
    }
}
