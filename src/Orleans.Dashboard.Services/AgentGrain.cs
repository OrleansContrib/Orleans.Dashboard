using System;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.Dashboard.Reports
{
    internal class AgentGrain : Grain, IAgentGrain
    {
        public Task Report(Immutable<AgentMessage[]> batch)
        {
            Console.WriteLine(batch.Value.Length);
            return Task.CompletedTask;
        }
    }
}