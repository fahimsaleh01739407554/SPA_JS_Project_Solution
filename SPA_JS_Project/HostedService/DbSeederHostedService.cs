using SPA_JS_Project.Models;

namespace SPA_JS_Project.HostedService
{
    public class DbSeederHostedService : IHostedService
    {
       
        IServiceProvider serviceProvider;
        public DbSeederHostedService(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
           
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
               
                  var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                  await SeedDbAsync(db);
               
            }
        }
       public async Task SeedDbAsync(ProductDbContext db)
        {
            await db.Database.EnsureCreatedAsync();
            if (!db.Customers.Any())
            {
                var c1 = new Customer { CustomerName = "Customer 1", Email = "customer1@mysite.com", Address = "Armanitola, Armenia" };
                await db.Customers.AddAsync(c1);
                var p1 = new Product { ProductName = "Product Name", IsAvailable = true, Price = 0.99M, Picture = "1.jpg" };
                await db.Products.AddAsync(p1);
                var o1 = new Order { OrderDate = DateTime.Today.AddDays(-8), DeliveryDate = DateTime.Today.AddDays(-1), Customer = c1, Status = Status.Pending };
                o1.OrderItems.Add(new OrderItem { Order = o1, Product = p1, Quantity = 2 });
                await db.Orders.AddAsync(o1);
                await db.SaveChangesAsync();
            }

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
    }
}
