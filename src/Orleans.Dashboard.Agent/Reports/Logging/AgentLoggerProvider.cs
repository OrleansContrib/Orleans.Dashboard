using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orleans.Dashboard.Reports.Logging
{
    [ProviderAlias(PROVIDER_ALIAS)]
    internal class AgentLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private const string PROVIDER_ALIAS = "Orleans.Dashboard";

        private readonly AgentLoggerOptions _options;
        private readonly IAgentMessageWriter _messageWriter;
        private IExternalScopeProvider _externalScopeProvider;
        private readonly ConcurrentDictionary<string, AgentLogger> _loggers;

        public AgentLoggerProvider(IOptions<AgentLoggerOptions> options, IAgentMessageWriter writer)
        {
            this._options = options?.Value ?? throw new ArgumentNullException(nameof(AgentLoggerOptions));
            this._messageWriter = writer;
            this._loggers = new ConcurrentDictionary<string, AgentLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this._loggers.GetOrAdd(
                categoryName,
                key => new AgentLogger(categoryName, this._options, this._messageWriter)
                {
                    ExternalScopeProvider = this._externalScopeProvider
                });
        }

        public void Dispose() { }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            this._externalScopeProvider = scopeProvider;
        }
    }
}