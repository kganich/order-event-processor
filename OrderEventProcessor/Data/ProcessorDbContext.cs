using OrderEventProcessor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderEventProcessor.Data
{
    public class ProcessorDbContext : DbContext
    {
        public ProcessorDbContext(DbContextOptions<ProcessorDbContext> options) : base(options)
        {
        }

        public DbSet<OrderEvent> OrderEvents { get; set; }
        public DbSet<PaymentEvent> PaymentEvents { get; set; }
    }

    public class ProcessorDbContextFactory : IDesignTimeDbContextFactory<ProcessorDbContext>
    {
        public ProcessorDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProcessorDbContext>();

            optionsBuilder.UseNpgsql("Host=db;Database=postgres;Username=postgres;Password=postgres");

            return new ProcessorDbContext(optionsBuilder.Options);
        }
    }
}