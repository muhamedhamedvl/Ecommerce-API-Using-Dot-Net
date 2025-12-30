using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Order;
using WebApiEcomm.Core.Services;
using WebApiEcomm.Core.Sharing;

namespace WebApiEcomm.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost]
        public async Task<ActionResult> create(OrderDto orderDTO)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            Order order = await _orderService.CreateOrdersAsync(orderDTO, email);

            return Ok(order);
        }


        [HttpGet]
        public async Task<ActionResult> GetOrders([FromQuery] PaginationParams paginationParams)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var orders = await _orderService.GetAllOrdersForUserAsync(email);
            
            // Apply pagination
            var totalCount = orders.Count();
            var paginatedOrders = orders
                .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();
            
            return Ok(new PagedResponse<OrderToReturnDTO>(paginationParams.Page, paginationParams.PageSize, totalCount, paginatedOrders));
        }


        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderToReturnDTO>> GetOrderById(int orderId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var order = await _orderService.GetOrderByIdAsync(orderId, email);
            return Ok(order);
        }


        [HttpGet("delivery-methods")]
        public async Task<ActionResult> GetDeliver()
        => Ok(await _orderService.GetDeliveryMethodAsync());
    }
}
