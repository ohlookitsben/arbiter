namespace Arbiter.Core
{
    public static class Constants
    {
        public const string LogFile = "arbiter.log";

        /// <summary>
        /// Regular expression to match a <see cref="Guid"/> string.
        /// </summary>
        public const string GuidExpression = @"[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}";
    }
}
