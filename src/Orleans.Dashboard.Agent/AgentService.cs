﻿using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Dashboard.Reports;
using Orleans.Dashboard.Reports.Logging;
using Orleans.Runtime;

namespace Orleans.Dashboard
{
    internal class AgentService : IAgentService, ILifecycleParticipant<ISiloLifecycle>, IDisposable
    {
        private readonly Channel<ReportMessage> _incomingMessages;
        private readonly ILocalSiloDetails _siloDetails;
        private readonly IGrainFactory _grainFactory;
        // private readonly IServiceProvider _serviceProvider;

        // private ILogger _logger;
        private bool _disposing;
        private Task _messagePump;

        public AgentService(ILocalSiloDetails siloDetails, IGrainFactory grainFactory)//, IServiceProvider serviceProvider)
        {
            this._grainFactory = grainFactory;
            this._siloDetails = siloDetails;
            // this._serviceProvider = serviceProvider;
            this._incomingMessages = Channel.CreateUnbounded<ReportMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });
        }

        public void DispatchMessage(ReportMessage message) => this._incomingMessages.Writer.TryWrite(message);

        private async Task RunReportMessagePump(CancellationToken cancellationToken)
        {
            var reader = this._incomingMessages.Reader;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var moreTask = reader.WaitToReadAsync();
                    var more = moreTask.IsCompletedSuccessfully ? moreTask.Result : await moreTask;
                    if (!more)
                    {
                        // this._logger.LogInformation($"{nameof(AgentService)} completed processing all messages.Shutting down.");
                        break;
                    }

                    while (reader.TryRead(out var message))
                    {
                        if (message == null) continue;
                        await this.Push(message);
                    }
                }
                catch (Exception exc)
                {
                    // this._logger.LogError("RunReportMessagePump has thrown an exception: {Exception}. Continuing.", exc);
                }
            }
        }

        private Task Push(ReportMessage message)
        {
            switch (message.Type)
            {
                case ReportMessageType.Log:
                    return this.PushLog(message);
                default:
                    // this._logger.LogError("Message not supported: {Message}", message.Type);
                    return Task.CompletedTask;
            }
        }

        private Task PushLog(ReportMessage message)
        {
            var log = message.Payload as LogMessage;

            //TODO: Forward to the dashboard services
            // this._logger.Log(log.Level, log.Content);

            return Task.CompletedTask;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(nameof(AgentService), ServiceLifecycleStage.RuntimeGrainServices, OnStart, OnStop);

            Task OnStart(CancellationToken cancellation)
            {
                if (cancellation.IsCancellationRequested) return Task.CompletedTask;

                // this._logger = this._serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AgentService));

                // this._logger.LogInformation($"Initializing Orleans Dashboard Agent for '{this._siloDetails.Name} | {this._siloDetails.SiloAddress}'.");

                this._messagePump = Task.Run(() => this.RunReportMessagePump(cancellation));

                // this._logger.LogInformation("Orleans Dashboard Agent initialized.");

                return Task.CompletedTask;
            }

            async Task OnStop(CancellationToken cancellation)
            {
                // this._logger.LogInformation("Stopping Orleans Dashboard Agent.");
                this._incomingMessages.Writer.TryComplete();

                if (this._messagePump != null)
                {
                    await Task.WhenAny(cancellation.WhenCancelled(), this._messagePump);
                }

                // this._logger.LogInformation("Orleans Dashboard Agent Stopped.");
            }
        }

        public void Dispose()
        {
            if (this._disposing) return;
            this._disposing = true;
            Utils.SafeExecute(() => this._incomingMessages.Writer.TryComplete());
            Utils.SafeExecute(() => this._messagePump?.GetAwaiter().GetResult());
        }
    }
}