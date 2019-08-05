using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Orleans.Dashboard.Reports.Logging
{
    internal class AgentLogger : ILogger
    {
        private const string ScopeKey = "Scope";
        private const string ArrowToken = " => ";
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
                    Category = this._categoryName,
                    Level = logLevel,
                    EventId = eventId.Id != 0 ? eventId.Id.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    EventName = !string.IsNullOrWhiteSpace(eventId.Name) ? eventId.Name : string.Empty,
                    Content = formatter(state, exception),
                    ActivityId = Activity.Current?.Id,
                    DateTime = DateTime.UtcNow
                };

                if (this._options.IncludeScopes)
                {
                    message.Properties = new Dictionary<string, string>();

                    if (state is IReadOnlyCollection<KeyValuePair<string, object>> stateDictionary)
                    {
                        foreach (KeyValuePair<string, object> item in stateDictionary)
                        {
                            message.Properties[item.Key] = Convert.ToString(item.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    if (this.ExternalScopeProvider != null)
                    {
                        var sb = new StringBuilder();
                        this.ExternalScopeProvider.ForEachScope(
                            (activeScope, builder) =>
                            {
                                // Ideally we expect that the scope to implement IReadOnlyList<KeyValuePair<string, object>>.
                                // But this is not guaranteed as user can call BeginScope and pass anything. Hence
                                // we try to resolve the scope as Dictionary and if we fail, we just serialize the object and add it.

                                if (activeScope is IReadOnlyCollection<KeyValuePair<string, object>> activeScopeDictionary)
                                {
                                    foreach (KeyValuePair<string, object> item in activeScopeDictionary)
                                    {
                                        message.Properties[item.Key] = Convert.ToString(item.Value, CultureInfo.InvariantCulture);
                                    }
                                }
                                else
                                {
                                    builder.Append(ArrowToken).Append(activeScope);
                                }
                            },
                            sb);

                        if (sb.Length > 0)
                        {
                            message.Properties[ScopeKey] = sb.ToString();
                        }
                    }
                }

                this._messageWriter.Write(new ReportMessage { Type = ReportMessageType.Log, Payload = message });
            }
        }
    }
}