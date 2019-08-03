using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Orleans.Dashboard.Reports.Logging
{
    [ProviderAlias(PROVIDER_ALIAS)]
    internal class AgentLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private const string PROVIDER_ALIAS = "Orleans.Dashboard";

        private readonly AgentLoggerOptions _options;
        private readonly Func<IAgentService> _agentFactory;
        private IExternalScopeProvider _externalScopeProvider;
        private readonly ConcurrentDictionary<string, AgentLogger> _loggers;

        public AgentLoggerProvider(IOptions<AgentLoggerOptions> options, Func<IAgentService> agentFactory)
        {
            this._options = options?.Value ?? throw new ArgumentNullException(nameof(AgentLoggerOptions));
            this._agentFactory = agentFactory;
            this._loggers = new ConcurrentDictionary<string, AgentLogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (categoryName == nameof(AgentService))
            {
                return NullLogger.Instance;
            }

            return this._loggers.GetOrAdd(
                categoryName,
                key => new AgentLogger(categoryName, this._options, this._agentFactory)
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