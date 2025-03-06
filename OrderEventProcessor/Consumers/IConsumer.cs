using RabbitMQ.Client;
using OrderEventProcessor.Data;

namespace OrderEventProcessor.Consumers
{
    public interface IConsumer
    {
        void Consume(ProcessorDbContext dbContext);
    }
}