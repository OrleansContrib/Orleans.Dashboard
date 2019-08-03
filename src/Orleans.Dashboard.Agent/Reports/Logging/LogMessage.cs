using Microsoft.Extensions.Logging;

namespace Orleans.Dashboard.Reports.Logging
{
    internal class LogMessage
    {
        public LogLevel Level { get; set; }
        public EventId EventId { get; set; }
        public string Content { get; set; }
    }
}