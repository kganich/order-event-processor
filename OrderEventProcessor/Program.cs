using OrderEventProcessor.Sender;
using OrderEventProcessor.Data;
using RabbitMQ.Client;
using OrderEventProcessor.Consumers;

namespace OrderEventProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            //create channel
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@rabbitmq:5672/");            
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // send messages
            SenderEvents.SendOrderEvent(channel);
            SenderEvents.SendPaymentEvent(channel);

            channel.Close();
            connection.Close();

            // create db
            var context = new ProcessorDbContextFactory().CreateDbContext(args);
            context.Database.EnsureCreated();

            // consumers
            var consumerOrder = new OrderEventConsumer();
            var consumerPayment = new PaymentEventConsumer();
            consumerOrder.Consume(context);
            consumerPayment.Consume(context);
        }
    }
}