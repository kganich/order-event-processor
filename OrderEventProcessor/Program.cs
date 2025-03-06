using OrderEventProcessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OrderEventProcessor.Data;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace OrderEventProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            //sender
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri("amqp://guest:guest@rabbitmq:5672/");            
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order-event-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var orderEventBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new OrderEvent
            {
                Id = "2",
                Product = "Apple",
                Total = 1.23m,
                Currency = "USD"
            }));

            channel.BasicPublish(exchange: "",
                                 routingKey: "order-event-queue",
                                 basicProperties: null,
                                 body: orderEventBodyBytes);



            var context = new ProcessorDbContextFactory().CreateDbContext(args);
            context.Database.EnsureCreated();

            // receiver
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var orderEvent = JsonSerializer.Deserialize<OrderEvent>(message);
                var orderModel = new OrderEvent()
                    {
                        Id = orderEvent.Id,
                        Product = orderEvent.Product,
                        Total = orderEvent.Total,
                        Currency = orderEvent.Currency
                    };

                context.OrderEvents.Add(orderModel);
                context.SaveChanges();
                Console.WriteLine(" [x] Received {0}", message);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            string consumerTag = channel.BasicConsume(queue: "order-event-queue",
                                 autoAck: false,
                                 consumer: consumer);

            Console.ReadLine();
            channel.BasicCancel(consumerTag);

            channel.Close();
            connection.Close();

            // while (true) {
            //     Console.WriteLine("Waiting for messages...");
            //     Thread.Sleep(2000);
            // }
        }
    }
}