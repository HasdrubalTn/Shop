using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Shop.Services.Common.EventBus.Messages.Events;
using AutoMapper;

namespace Ordering.API.Mapper
{
    public class OrderingProfile : Profile
    {
        public OrderingProfile()
        {
            CreateMap<CheckoutOrderCommand, BasketCheckoutEvent>().ReverseMap();
        }
    }
}
