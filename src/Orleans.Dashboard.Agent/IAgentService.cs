using Orleans.Dashboard.Reports;

namespace Orleans.Dashboard
{
    internal interface IAgentService
    {
        void DispatchMessage(ReportMessage message);
    }
}