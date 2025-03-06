namespace OrderEventProcessor.Models;

public class PaymentEvent
{
    public string Id { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}