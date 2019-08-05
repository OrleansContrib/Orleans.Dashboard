using System;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Orleans.Dashboard.Reports.Logging
{
    internal class AgentLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly AgentLoggerOptions _options;
        private readonly IAgentMessageWriter _messageWriter;

        public AgentLogger(string categoryName, AgentLoggerOptions options, IAgentMessageWriter writer)
        {
            this._messageWriter = writer;
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

                var a = RequestContext.PropagateActivityId;

                this._messageWriter.Write(new ReportMessage { Type = ReportMessageType.Log, Payload = message });
            }
        }
    }
}