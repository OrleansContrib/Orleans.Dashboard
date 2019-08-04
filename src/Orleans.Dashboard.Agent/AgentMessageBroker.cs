using System.Threading.Channels;
using System.Threading.Tasks;
using Orleans.Dashboard.Reports;

namespace Orleans.Dashboard
{
    internal interface IAgentMessageWriter
    {
        void Write(ReportMessage message);

        void Complete();
    }

    internal interface IAgentMessageReader
    {
        ValueTask<bool> HasMore();
        bool Read(out ReportMessage message);
    }

    internal interface IAgentMessageBroker : IAgentMessageReader, IAgentMessageWriter { }

    internal class AgentMessageBroker : IAgentMessageBroker
    {
        private readonly Channel<ReportMessage> _messages;

        public ChannelReader<ReportMessage> Reader => this._messages.Reader;

        public void Write(ReportMessage message) => this._messages.Writer.TryWrite(message);

        public void Complete() => this._messages?.Writer?.TryComplete();

        public ValueTask<bool> HasMore() => this._messages.Reader.WaitToReadAsync();

        public bool Read(out ReportMessage message) => this._messages.Reader.TryRead(out message);

        public AgentMessageBroker()
        {
            this._messages = Channel.CreateUnbounded<ReportMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });
        }
    }
}