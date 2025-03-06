using OrderEventProcessor.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OrderEventProcessor.Data;

namespace OrderEventProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new ProcessorDbContextFactory().CreateDbContext(args);
            context.Database.EnsureCreated();
            // while(true) {
            //     Console.WriteLine("Waiting for messages...");
            //     System.Threading.Thread.Sleep(1000);
            // }

            context.OrderEvents.Add(new OrderEvent
            {
                Id = "1",
                Product = "Apple",
                Total = 1.23m,
                Currency = "USD"
            });
            context.SaveChanges();
        }
    }
}