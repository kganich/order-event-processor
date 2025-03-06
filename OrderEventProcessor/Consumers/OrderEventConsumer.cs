using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderEventProcessor.Data;
using OrderEventProcessor.Models;

namespace OrderEventProcessor.Consumers
{
    public class OrderEventConsumer : IConsumer
    {
        public void Consume(ProcessorDbContext dbContext)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672/")
            };
            
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order-event-queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var waitHandle = new TaskCompletionSource<bool>();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("X-MsgType"))
                {
                    var msgType = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["X-MsgType"]);
                    if (msgType == "OrderEvent")
                    {
                        var orderEvent = JsonSerializer.Deserialize<OrderEvent>(message);
                        if (orderEvent != null) 
                        {
                            var orderModel = new OrderEvent()
                            {
                                Id = orderEvent.Id,
                                Product = orderEvent.Product,
                                Total = orderEvent.Total,
                                Currency = orderEvent.Currency
                            };

                            try
                            {
                                dbContext.OrderEvents.Add(orderModel);
                                dbContext.SaveChanges();
                                Console.WriteLine("Order event saved to DB.");
                            }
                            catch (Exception e)
                            {
                                dbContext.Remove(orderModel);
                                Console.WriteLine($"Error saving order event: {e.Message}");
                            }
                        }
                        else 
                        {
                            Console.WriteLine("Order event is null");
                        }
                    }
                    else 
                    {
                        Console.WriteLine(" [x] Received Unknown Event {0}", message);
                    }
                }
                else 
                {
                    Console.WriteLine(" [x] Received Unknown Event {0}", message);
                }
            };

            channel.BasicConsume(queue: "order-event-queue",
                                autoAck: true,
                                consumer: consumer);
            
            Console.WriteLine("Waiting for messages...");

            Thread.Sleep(10000);
            
            Console.WriteLine("Message processed. Exiting...");
        }        
    }
}