namespace Orleans.Dashboard.Reports.Logging
{
    public class AgentLoggerOptions
    {
        public bool TrackExceptions { get; set; } = true;
        public bool IncludeScopes { get; set; } = true;
    }
}