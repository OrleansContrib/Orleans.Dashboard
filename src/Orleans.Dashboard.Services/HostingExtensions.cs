using Orleans.Dashboard.Reports;
using Orleans.Hosting;

namespace Orleans.Dashboard
{
    public static class HostingExtensions
    {
        public static ISiloBuilder AddDashboardServices(this ISiloBuilder builder)
        {
            return builder.ConfigureApplicationParts(parts => parts.AddFrameworkPart(typeof(AgentGrain).Assembly));
        }
    }
}