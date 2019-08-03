using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace Orleans.Dashboard.TestSilo
{
    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task DoSomething();
    }

    public class TestGrain : Grain, ITestGrain, IRemindable
    {
        private readonly ILogger _logger;

        public TestGrain(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<TestGrain>();
        }

        public override Task OnActivateAsync() => this.RegisterOrUpdateReminder("TEST", TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        public Task DoSomething()
        {
            this._logger.LogInformation("Logging from a GRAIN!");
            return Task.CompletedTask;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            this._logger.LogInformation("REMINDER");
            return Task.CompletedTask;
        }
    }

    public class TestStart : IStartupTask
    {
        private readonly IGrainFactory _grainFactory;

        public TestStart(IGrainFactory grainFactory)
        {
            this._grainFactory = grainFactory;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            var grain = this._grainFactory.GetGrain<ITestGrain>(0);
            await grain.DoSomething();
        }
    }
}