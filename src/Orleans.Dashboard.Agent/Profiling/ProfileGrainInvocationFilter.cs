using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Dashboard.Reports;
using Orleans.Dashboard.Reports.Logging;

namespace Orleans.Dashboard
{
    internal sealed class ProfileGrainInvocationFilter : IIncomingGrainCallFilter
    {
        private readonly ILogger _logger;
        private readonly IAgentService _agent;

        public ProfileGrainInvocationFilter(ILoggerFactory loggerFactory, IAgentService agent)
        {
            this._logger = loggerFactory.CreateLogger<ProfileGrainInvocationFilter>();
            this._agent = agent;
        }

        public async Task Invoke(IIncomingGrainCallContext context)
        {
            await context.Invoke();
            // var msg = new ReportMessage
            // {
            //     Type = ReportMessageType.Log,
            //     Payload = new LogMessage
            //     {
            //         Level = LogLevel.Warning,
            //         Content = $"DASHBOARD: {context.Grain.GetType()} | {context.ImplementationMethod.Name}"
            //     }
            // };
            // this._agent.DispatchMessage(msg);
        }
    }
}