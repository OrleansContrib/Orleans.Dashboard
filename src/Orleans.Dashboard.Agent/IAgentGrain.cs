using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.Dashboard
{
    internal interface IAgentGrain : IGrainWithStringKey
    {
        Task Report(Immutable<AgentMessage[]> batch);
    }
}