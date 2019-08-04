using System.Threading.Channels;
using Orleans.Dashboard.Reports;

namespace Orleans.Dashboard
{
    internal interface IAgentMessageWriter
    {
        ChannelWriter<ReportMessage> Writer { get; }
        void Write(ReportMessage message);
    }

    internal interface IAgentMessageReader
    {
        ChannelReader<ReportMessage> Reader { get; }
    }

    internal interface IAgentMessageBroker : IAgentMessageReader, IAgentMessageWriter { }

    internal class AgentMessageBroker : IAgentMessageBroker
    {
        private readonly Channel<ReportMessage> _messages;

        public ChannelReader<ReportMessage> Reader => this._messages.Reader;

        public ChannelWriter<ReportMessage> Writer => this._messages.Writer;

        public void Write(ReportMessage message) => this._messages.Writer.TryWrite(message);

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