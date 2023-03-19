using Dapper;
using Discount.API.Entities;
using Npgsql;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            var dbconnection = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");

            using var connection = new NpgsqlConnection(dbconnection);
            var command = $"INSERT INTO Coupon (ProductName, Description, Amount) VALUES (@ProductName, @Description, @Amount)";

            var affected = await connection.ExecuteAsync(command, new {ProductName=coupon.ProductName, Description=coupon.Description, Amount=coupon.Amount});
        
            return (affected > 0);
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            var dbconnection = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");

            using var connection = new NpgsqlConnection(dbconnection);
            var command = $"DELETE FROM Coupon WHERE ProductName=@ProductName";

            var affected = await connection.ExecuteAsync(command, new { ProductName = productName });

            return (affected > 0);
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            var dbconnection = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");

            using var connection = new NpgsqlConnection(dbconnection);
            var command = $"SELECT * FROM Coupon WHETRE ProductName = @ProductName";
            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(command,new {ProductName=productName});

            if (coupon is null)
            {
                return new Coupon 
                {
                    ProductName = "No Discount",
                    Amount = 0,
                    Description = "No Discount"
                };
            }
            return coupon;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            var dbconnection = _configuration.GetValue<string>("DatabaseSettings:ConnectionString");

            using var connection = new NpgsqlConnection(dbconnection);
            var command = $"UPDATE SET ProductName=@ProductName, Description=@Description, Amount=@Amount WHERE Id=@Id";

            var affected = await connection.ExecuteAsync(command, new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount, Id = coupon.Id });

            return (affected > 0);
        }
    }
}
