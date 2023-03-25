using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shop.Services.Common.EventBus.Messages.Events;
using System.Net;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly GrpcServices.DiscountClientService _discountClientService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository, GrpcServices.DiscountClientService discountClientService, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _discountClientService = discountClientService ?? throw new ArgumentNullException(nameof(discountClientService));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _basketRepository.GetBasketAsync(userName);

            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var coupon = await _discountClientService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);

            if(updatedBasket is null)
            {
                NotFound(updatedBasket);
            }

            return Ok(updatedBasket);
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> DeleteBasket(string userName)
        {
            await _basketRepository.DeleteBasketAsync(userName);

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // Validate the backet checkout
            if (!await ValidateBasketCheckoutAsync(basketCheckout))
            {
                return BadRequest();
            }

            // Retrieve basket information
            var basket = await GetBasketAsync(basketCheckout.UserName);
            if (basket == null)
            {
                return BadRequest();
            }

            // Mapping
            var eventMessage = MapBasketToCheckoutEvent(basket, basketCheckout);

            // Publish the event
            await PublishBasketCheckoutEventAsync(eventMessage);

            // Delete the basket
            await DeleteBasketAsync(basket.UserName);

            // Add additional information to response
            var response = new
            {
                eventMessage.OrderId,
                eventMessage.TotalPrice
            };

            return Accepted(response);
        }

        private async Task<bool> ValidateBasketCheckoutAsync(BasketCheckout basketCheckout)
        {
            // Validate that basket checkout data is valid
            return true;
        }

        private async Task<ShoppingCart> GetBasketAsync(string userName)
        {
            // Retrieve basket from database or other storage
            // get existing basket with total price
            var basket = await _basketRepository.GetBasketAsync(userName);

            return basket;
        }

        private BasketCheckoutEvent MapBasketToCheckoutEvent(ShoppingCart basket, BasketCheckout basketCheckout)
        {
            // Map basket data to checkout event data
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;

            return eventMessage;
        }

        private async Task PublishBasketCheckoutEventAsync(BasketCheckoutEvent eventMessage)
        {
            // Publish basket checkout event to message broker
            await _publishEndpoint.Publish(eventMessage);
        }

        private async Task DeleteBasketAsync(string userName)
        {
            // Delete basket from database or other storage
            await _basketRepository.DeleteBasketAsync(userName);
        }
    }
}
