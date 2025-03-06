using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using OrderEventProcessor.Models;

namespace OrderEventProcessor.Sender
{
    public class SenderEvents
    {
        public static void SendOrderEvent(IModel channel)
        {
            channel.QueueDeclare(queue: "order-event-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var orderEventBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new OrderEvent
            {
                Id = "5834",
                Product = "Apple",
                Total = 3m,
                Currency = "USD"
            }));

            var OrderEventProperties = channel.CreateBasicProperties();
            OrderEventProperties.Headers = new Dictionary<string, object>
            {
                { "X-MsgType", "OrderEvent" }
            };


            channel.BasicPublish(exchange: "",
                                 routingKey: "order-event-queue",
                                 basicProperties: OrderEventProperties,
                                 body: orderEventBodyBytes);

            Console.WriteLine(" [x] Sent OrderEvent");
        }

        public static void SendPaymentEvent(IModel channel)
        {
            channel.QueueDeclare(queue: "payment-event-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var paymentEventBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new PaymentEvent
            {
                Amount = 1.43m,
                OrderId = "555"
            }));

            var paymentEventProperties = channel.CreateBasicProperties();
            paymentEventProperties.Headers = new Dictionary<string, object>
            {
                { "X-MsgType", "PaymentEvent" }
            };

            channel.BasicPublish(exchange: "",
                                 routingKey: "payment-event-queue",
                                 basicProperties: paymentEventProperties,
                                 body: paymentEventBodyBytes);

            Console.WriteLine(" [x] Sent PaymentEvent");
        }
    }
}