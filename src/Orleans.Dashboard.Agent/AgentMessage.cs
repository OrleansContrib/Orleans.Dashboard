using System;

namespace Orleans.Dashboard
{
    internal class AgentMessage
    {
        public string SiloName { get; set; }
        public string SiloAddress { get; set; }
        public string ClusterId { get; set; }
        public string ServiceId { get; set; }
        public DateTime DateTime { get; set; }
        public object Message { get; set; }
    }

    internal enum AgentMessageType
    {
        Ping = 0,
        Report = 1
    }
}