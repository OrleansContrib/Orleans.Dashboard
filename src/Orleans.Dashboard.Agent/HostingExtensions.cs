using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Orleans.Dashboard.Reports.Logging;
using Orleans.Dashboard.Reports.Tracking;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Orleans.Dashboard
{
    public static class HostingExtensions
    {
        public static ISiloBuilder AddDashboardAgent(this ISiloBuilder builder,
            Action<AgentOptions> configureAgentOptions = null,
            Action<AgentLoggerOptions> configureAgentLoggerOptions = null,
            Action<AgentTrackingOptions> configureAgentTrackingOptions = null)
        {
            return builder.ConfigureServices((builderContext, services) =>
            {
                if (configureAgentOptions == null)
                {
                    configureAgentOptions = opt => { };
                }

                if (configureAgentLoggerOptions == null)
                {
                    configureAgentLoggerOptions = opt => { };
                }

                if (configureAgentTrackingOptions == null)
                {
                    configureAgentTrackingOptions = opt => { };
                }

                services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AgentLoggerProvider>());

                services
                    .Configure(configureAgentOptions)
                    .Configure(configureAgentLoggerOptions)
                    .Configure(configureAgentTrackingOptions)
                    .AddSingleton<IAgentMessageBroker, AgentMessageBroker>()
                    .AddSingleton<IAgentMessageReader>(sp => sp.GetRequiredService<IAgentMessageBroker>())
                    .AddSingleton<IAgentMessageWriter>(sp => sp.GetRequiredService<IAgentMessageBroker>())
                    .AddSingleton<IOutgoingGrainCallFilter, GrainMethodInvocationFilter>()
                    .AddSingleton<IIncomingGrainCallFilter, GrainMethodExecutionFilter>()
                    .AddSingleton<IExternalDependencyTracker, ExternalDependencyTracker>()
                    .AddSingleton<AgentService>()
                    .AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp =>
                        sp.GetRequiredService<AgentService>() as ILifecycleParticipant<ISiloLifecycle>);
            });
        }
    }
}