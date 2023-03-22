using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() 
                {
                    UserName = "ams", 
                    FirstName = "Ali", 
                    LastName = "MSELMI", 
                    EmailAddress = "mselmi@outlook.com", 
                    AddressLine = "Avenue de France", 
                    Country = "France", 
                    TotalPrice = 350m,
                    CardName= "",
                    CardNumber ="122223",
                    CreatedBy = "Ali, MSELMI",
                    CreatedDate = DateTime.Now,
                    CVV = "CCV",
                    Expiration = "NA",
                    LastModifiedBy = "Ali, MSELMI",
                    LastModifiedDate = DateTime.Now,
                    PaymentMethod = 1,
                    State = "Shipped",
                    ZipCode = "75015"
                }
            };
        }
    }
}
