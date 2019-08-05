using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Orleans.Dashboard.Reports.Logging
{
    internal class LogMessage
    {
        public DateTime DateTime { get; set; }
        public LogLevel Level { get; set; }
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public string ActivityId { get; set; }
        public IDictionary<string, string> Properties { get; set; }
    }
}