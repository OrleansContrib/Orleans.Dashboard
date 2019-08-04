using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Orleans.Dashboard.Reports.Logging;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Orleans.Dashboard
{
    public static class HostingExtensions
    {
        public static ISiloBuilder AddDashboardAgent(this ISiloBuilder builder,
            Action<AgentLoggerOptions> configureAgentLoggerOptions = null)
        {
            return builder.ConfigureServices((builderContext, services) =>
            {
                if (configureAgentLoggerOptions == null)
                {
                    configureAgentLoggerOptions = opt => { };
                }

                services.AddSingleton<Func<IAgentService>>(sp => () => sp.GetRequiredService<IAgentService>());

                services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AgentLoggerProvider>());

                services
                    .Configure(configureAgentLoggerOptions)
                    .AddSingleton<IAgentMessageBroker, AgentMessageBroker>()
                    .AddSingleton<IAgentMessageReader>(sp => sp.GetRequiredService<IAgentMessageBroker>())
                    .AddSingleton<IAgentMessageWriter>(sp => sp.GetRequiredService<IAgentMessageBroker>())
                    .AddSingleton<IIncomingGrainCallFilter, ProfileGrainInvocationFilter>()
                    .AddSingleton<IAgentService, AgentService>()
                    .AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp =>
                        sp.GetRequiredService<IAgentService>() as ILifecycleParticipant<ISiloLifecycle>);
            });
        }
    }
}