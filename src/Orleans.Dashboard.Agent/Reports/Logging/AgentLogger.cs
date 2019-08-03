using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orleans.Dashboard.Reports.Logging
{
    internal class AgentLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly AgentLoggerOptions _options;
        private readonly Func<IAgentService> _agentFactory;
        private IAgentService _agent;

        public AgentLogger(string categoryName, AgentLoggerOptions options, Func<IAgentService> agentFactory)
        {
            this._agentFactory = agentFactory;
            this._categoryName = categoryName;
            this._options = options;
        }

        internal IExternalScopeProvider ExternalScopeProvider { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this.ExternalScopeProvider != null ? this.ExternalScopeProvider.Push(state) : NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: Handle log levels properly
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.IsEnabled(logLevel))
            {
                var message = new LogMessage
                {
                    Level = logLevel,
                    EventId = eventId,
                    Content = formatter(state, exception)
                };

                if (this._categoryName == nameof(AgentService))
                {
                    var a = this._categoryName;
                }

                if (this._agent == null)
                {
                    try
                    {
                        this._agent = this._agentFactory();
                        var aaa = 222;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                this._agent.DispatchMessage(new ReportMessage { Type = ReportMessageType.Log, Payload = message });
            }
        }
    }
}