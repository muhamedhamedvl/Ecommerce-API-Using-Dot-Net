using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }
        [HttpPost("baskets/{basketId}")]
        public async Task<ActionResult<CustomerBasket>> Create(string basketId, [FromQuery] int deliveryId)
        {
            var basket = await paymentService.CreateOrUpdatePaymentAsync(basketId, deliveryId);
            return basket is null ? NotFound() : Ok(basket);
        }
    }
}
