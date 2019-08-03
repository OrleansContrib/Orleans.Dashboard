namespace Orleans.Dashboard
{
    internal class AgentMessage
    {
        public string SiloName { get; set; }
        public string SiloAddress { get; set; }
        public object Message { get; set; }
    }

    internal enum AgentMessageType
    {
        Ping = 0,
        Report = 1
    }
}