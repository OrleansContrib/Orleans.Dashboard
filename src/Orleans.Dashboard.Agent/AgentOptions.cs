using System;

namespace Orleans.Dashboard
{
    public class AgentOptions
    {
        public int MaxBatchSize { get; set; } = 100;
        public TimeSpan MaxBatchInterval { get; set; } = TimeSpan.FromMilliseconds(1000);
    }
}