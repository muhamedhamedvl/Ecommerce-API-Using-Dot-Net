using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;

namespace WebApiEcomm.API.Controllers
{
    [Authorize]
    public class BasketController : BaseController
    {
        public BasketController(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }
		[HttpGet("{basketId}")]
		public async Task<IActionResult> GetBasket(string basketId)
        {
			var res = await _work.CustomerBasketRepository.GetCustomerBasketAsync(basketId);
            if (res is null) 
            { 
                return Ok(new CustomerBasket());

            }
            return Ok(res);
        }
		[HttpPut]
        public async Task<IActionResult> Update(CustomerBasket customerBasket)
        {
            var basket = await _work.CustomerBasketRepository.UpdateCustomerBasketAsync(customerBasket);
			return Ok(basket);
        }
		[HttpDelete("{basketId}")]
		public async Task<IActionResult> DeleteById(string basketId)
        {
			var res = await _work.CustomerBasketRepository.DeleteCustomerBasketAsync(basketId);
			return res ? NoContent() : BadRequest(new ResponseApi(400));
        }
    }
}
