using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Concurrency;
using Orleans.Configuration;
using Orleans.Dashboard.Reports;
using Orleans.Runtime;

namespace Orleans.Dashboard
{
    internal class AgentService : ILifecycleParticipant<ISiloLifecycle>, IDisposable
    {
        private readonly IAgentMessageReader _messageReader;
        private readonly IAgentMessageWriter _messageWriter;
        private readonly ILocalSiloDetails _siloDetails;
        private readonly IGrainFactory _grainFactory;
        private readonly ClusterOptions _clusterOptions;
        private readonly AgentOptions _agentOptions;
        private readonly Queue<AgentMessage> _pendingMessages;
        private readonly string _reporterGrainId;
        private ILogger _logger;
        private bool _disposing;
        private Task _messagePump;
        private DateTime _lastPush;

        public AgentService(
            ILoggerFactory loggerFactory,
            ILocalSiloDetails siloDetails,
            IGrainFactory grainFactory,
            IAgentMessageReader reader,
            IAgentMessageWriter writer,
            IOptions<ClusterOptions> clusterOptions,
            IOptions<AgentOptions> agentOptions)
        {
            this._grainFactory = grainFactory;
            this._siloDetails = siloDetails;
            this._logger = loggerFactory.CreateLogger<AgentService>();
            this._messageReader = reader;
            this._messageWriter = writer;
            this._clusterOptions = clusterOptions.Value;
            this._agentOptions = agentOptions.Value;
            this._pendingMessages = new Queue<AgentMessage>();
            this._reporterGrainId = $"{this._clusterOptions.ServiceId}|{this._clusterOptions.ClusterId}|{this._siloDetails.Name}";
        }

        private async Task RunReportMessagePump(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var moreTask = this._messageReader.HasMore();
                    var more = moreTask.IsCompletedSuccessfully ? moreTask.Result : await moreTask;
                    if (!more)
                    {
                        this._logger.LogInformation($"{nameof(AgentService)} completed processing all messages.Shutting down.");
                        break;
                    }

                    while (this._messageReader.Read(out var message))
                    {
                        if (message == null) continue;
                        await this.Push(message);
                    }
                }
                catch (Exception exc)
                {
                    this._logger.LogError("RunReportMessagePump has thrown an exception: {Exception}. Continuing.", exc);
                }
            }
        }

        private async Task Push(ReportMessage message)
        {
            this.EnqueueMessage(message);

            if (this._pendingMessages.Count > 0 &&
                (this._pendingMessages.Count >= this._agentOptions.MaxBatchSize ||
                (DateTime.UtcNow - this._lastPush) >= this._agentOptions.MaxBatchInterval))
            {
                var batch = new AgentMessage[this._pendingMessages.Count];
                for (int i = 0; i < batch.Length; i++)
                {
                    batch[i] = this._pendingMessages.Dequeue();
                }

                var reporter = this._grainFactory.GetGrain<IAgentGrain>(this._reporterGrainId);
                await reporter.Report(batch.AsImmutable());
                this._lastPush = DateTime.UtcNow;
            }
        }

        private void EnqueueMessage(ReportMessage message)
        {
            var agentMessage = new AgentMessage
            {
                ClusterId = this._clusterOptions.ClusterId,
                ServiceId = this._clusterOptions.ServiceId,
                Message = message,
                SiloAddress = this._siloDetails.SiloAddress.ToString(),
                SiloName = this._siloDetails.Name,
                DateTime = DateTime.UtcNow
            };

            this._pendingMessages.Enqueue(agentMessage);
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(nameof(AgentService), ServiceLifecycleStage.RuntimeGrainServices, OnStart, OnStop);

            Task OnStart(CancellationToken cancellation)
            {
                if (cancellation.IsCancellationRequested) return Task.CompletedTask;

                this._logger.LogInformation($"Initializing Orleans Dashboard Agent for '{this._siloDetails.Name} | {this._siloDetails.SiloAddress}'.");

                this._messagePump = Task.Run(() => this.RunReportMessagePump(cancellation));

                this._logger.LogInformation("Orleans Dashboard Agent initialized.");

                return Task.CompletedTask;
            }

            async Task OnStop(CancellationToken cancellation)
            {
                this._logger.LogInformation("Stopping Orleans Dashboard Agent.");
                this._messageWriter.Complete();

                if (this._messagePump != null)
                {
                    await Task.WhenAny(cancellation.WhenCancelled(), this._messagePump);
                }

                this._logger.LogInformation("Orleans Dashboard Agent Stopped.");
            }
        }

        public void Dispose()
        {
            if (this._disposing) return;
            this._disposing = true;
            Utils.SafeExecute(() => this._messageWriter?.Complete());
            Utils.SafeExecute(() => this._messagePump?.GetAwaiter().GetResult());
        }
    }
}
