using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountClientService
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _service;

        public DiscountClientService(DiscountProtoService.DiscountProtoServiceClient service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest { ProductName = productName };
            return await _service.GetDiscountAsync(discountRequest);
        }
    }
}
