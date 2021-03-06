namespace Orleans.Dashboard.Reports
{
    internal class ReportMessage
    {
        public ReportMessageType Type { get; set; }
        public object Payload { get; set; }
    }

    internal enum ReportMessageType
    {
        Log = 0,
        GrainMethodExecution = 1,
        GrainMethodInvocation = 2,
        ExternalDependencyTrack = 3
    }
}