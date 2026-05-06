using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Order;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Data;

namespace WebApiEcomm.InfraStructure.Repositores.Service
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;

        public OrderService(IUnitOfWork unitOfWork, AppDbContext context, IMapper mapper, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _mapper = mapper;
            _paymentService = paymentService;
        }
        public async Task<Order> CreateOrdersAsync(OrderDto orderDTO, string BuyerEmail)
        {
            var basket = await _unitOfWork.CustomerBasketRepository.GetCustomerBasketAsync(orderDTO.basketId);
            if (basket is null)
            {
                throw new InvalidOperationException("Basket not found or has expired.");
            }

            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (var item in basket.basketItems)
            {
                var Product = await _unitOfWork.ProductRepository.GetByIdAsync(item.Id);
                if (Product is null)
                {
                    throw new InvalidOperationException($"Basket contains an unknown product id {item.Id}.");
                }

                var orderItem = new OrderItem
                    (Product.Id, item.Image, Product.Name, item.Price, item.Quantity);
                orderItems.Add(orderItem);

            }
            var deliverMethod = await _context.DeliveryMethods.FirstOrDefaultAsync(m => m.Id == orderDTO.DelliveryMethodId);
            if (deliverMethod is null)
            {
                throw new InvalidOperationException("Delivery method is not valid.");
            }

            var subTotal = orderItems.Sum(m => m.Price * m.Quntity);

            var ship = _mapper.Map<ShippingAddress>(orderDTO.shipaddressdto);

            var ExisitOrder = await _context.Orders.Where(m => m.PaymentIntentId == basket.PaymentIntentId).FirstOrDefaultAsync();

            if (ExisitOrder is not null)
            {
                _context.Orders.Remove(ExisitOrder);
                await _paymentService.CreateOrUpdatePaymentAsync(orderDTO.basketId, deliverMethod.Id);
            }

            var order = new
                Order(BuyerEmail, subTotal, ship, deliverMethod, orderItems, basket.PaymentIntentId);

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            await _unitOfWork.CustomerBasketRepository.DeleteCustomerBasketAsync(orderDTO.basketId);
            return order;

        }

        public async Task<IReadOnlyList<OrderToReturnDTO>> GetAllOrdersForUserAsync(string BuyerEmail)
        {
            var orders = await _context.Orders.Where(m => m.BuyerEmail == BuyerEmail)
                .Include(inc => inc.orderItems).Include(inc => inc.deliveryMethod)
                .ToListAsync();
            var result = _mapper.Map<IReadOnlyList<OrderToReturnDTO>>(orders);
            result = result.OrderByDescending(m => m.Id).ToList();
            return result;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodAsync()
        => await _context.DeliveryMethods.AsNoTracking().ToListAsync();

        public async Task<OrderToReturnDTO> GetOrderByIdAsync(int Id, string BuyerEmail)
        {
            var order = await _context.Orders.Where(m => m.Id == Id && m.BuyerEmail == BuyerEmail)
                  .Include(inc => inc.orderItems).Include(inc => inc.deliveryMethod)
                  .FirstOrDefaultAsync();
            var result = _mapper.Map<OrderToReturnDTO>(order);
            return result;
        }
    }
}