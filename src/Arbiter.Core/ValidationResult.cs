namespace Arbiter.Core
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string FailureReason { get; }

        private ValidationResult(bool isValid, string failureReason)
        {
            IsValid = isValid;
            FailureReason = failureReason;
        }

        public static ValidationResult Pass => new ValidationResult(true, string.Empty);
        public static ValidationResult Fail(string failureReason) => new ValidationResult(false, failureReason);
    }
}
