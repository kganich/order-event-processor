using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderEventProcessor.Data;
using OrderEventProcessor.Models;

namespace OrderEventProcessor.Consumers
{
    public class PaymentEventConsumer : IConsumer
    {
        public void Consume(ProcessorDbContext dbContext)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@rabbitmq:5672/")
            };
            
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
        
            channel.QueueDeclare(queue: "payment-event-queue",
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
                    if (msgType == "PaymentEvent")
                    {
                        var paymentEvent = JsonSerializer.Deserialize<PaymentEvent>(message);
                        if (paymentEvent != null) 
                        {
                            var paymentModel = new PaymentEvent()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Amount = paymentEvent.Amount,
                                OrderId = paymentEvent.OrderId
                            };

                            try
                            {
                                dbContext.PaymentEvents.Add(paymentModel);
                                dbContext.SaveChanges();
                                Console.WriteLine("PaymentEvents saved to DB.");
                            }
                            catch (Exception e)
                            {
                                dbContext.Remove(paymentModel);
                                Console.WriteLine($"Error saving order: {e}");
                            }
                        

                            //check fully paid
                            var order = dbContext.OrderEvents.Find(paymentModel.OrderId);
                            if (order != null)
                            {
                                var totalPaid = dbContext.PaymentEvents.Where(p => p.OrderId == paymentModel.OrderId).Sum(p => p.Amount);
                                if (totalPaid >= order.Total)
                                {
                                    Console.WriteLine($"Order: {order.Id}, Product: {order.Product}, Total: {order.Total} {order.Currency}, Status: PAID");
                                }
                                else if (totalPaid < order.Total)
                                {
                                    Console.WriteLine("Order not fully paid");
                                }
                            } else 
                            {
                                Console.WriteLine("Order is null");
                            }
                        }
                        else 
                        {
                            Console.WriteLine("Order not found");
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
            channel.BasicConsume(queue: "payment-event-queue",
                                autoAck: true,
                                consumer: consumer);
            
            Console.WriteLine("Waiting for messages...");

            Thread.Sleep(10000);
            
            Console.WriteLine("Message processed. Exiting...");
        }
    }
}