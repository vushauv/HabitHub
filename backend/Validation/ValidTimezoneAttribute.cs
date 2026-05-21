using backend.Utils;
using System.ComponentModel.DataAnnotations;

namespace backend.Validation
{
    public class ValidTimezoneAttribute: ValidationAttribute
    {
        public ValidTimezoneAttribute()
        {
            ErrorMessage = "Invalid timezone.";
        }
        public override bool IsValid(object? value)
        {
            string? timezone = value as string;
            if (string.IsNullOrWhiteSpace(timezone))
                return false;
            return SupportedTimezones.Timezones.Contains(timezone.Trim());
        }
    }
}
