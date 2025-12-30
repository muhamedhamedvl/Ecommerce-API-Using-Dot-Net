using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApiEcomm.API.Controllers;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Order;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.UnitTests.API.Controllers
{
    [TestFixture]
    public class ordersControllerTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private OrdersController _controller;

        [SetUp]
        public void Setup()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _controller = new OrdersController(_orderServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, "test@test.com")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task create_ShouldReturnOk_WithCreatedOrder()
        {
            // Arrange
            var dto = new OrderDto { basketId = "b1", DelliveryMethodId = 1 };
            var order = new Order { Id = 1, BuyerEmail = "test@test.com" };

            _orderServiceMock
                .Setup(s => s.CreateOrdersAsync(
                    It.Is<OrderDto>(d => d.basketId == "b1" && d.DelliveryMethodId == 1),
                    "test@test.com"))
                .ReturnsAsync(order);
            var ok = await _controller.create(dto) as OkObjectResult;

            // Assert
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(order));
        }

        [Test]
        public async Task getorders_ShouldReturnOk_WithOrdersList()
        {
            // Arrange
            var orders = new List<OrderToReturnDTO>
            {
                new OrderToReturnDTO { Id = 1, BuyerEmail = "test@test.com" }
            };

            _orderServiceMock
                .Setup(s => s.GetAllOrdersForUserAsync("test@test.com"))
                .ReturnsAsync(orders);
            var actionResult = await _controller.getorders();
            var ok = actionResult.Result as OkObjectResult;

            // Assert
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(orders));
        }

        [Test]
        public async Task getOrderById_ShouldReturnOk_WhenOrderExists()
        {
            // Arrange
            var orderDto = new OrderToReturnDTO { Id = 1, BuyerEmail = "test@test.com" };

            _orderServiceMock
                .Setup(s => s.GetOrderByIdAsync(1, "test@test.com"))
                .ReturnsAsync(orderDto);

            // Act
            var actionResult = await _controller.getOrderById(1);
            var ok = actionResult.Result as OkObjectResult;

            // Assert
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(orderDto));
        }

        [Test]
        public async Task GetDeliver_ShouldReturnOk_WithDeliveryMethods()
        {
            // Arrange
            var methods = new List<DeliveryMethod>
            {
                new DeliveryMethod("Fast", "1-2 days", "Quick delivery", 50m)
            };

            _orderServiceMock
                .Setup(s => s.GetDeliveryMethodAsync())
                .ReturnsAsync(methods);

            // Act 
            var ok = await _controller.GetDeliver() as OkObjectResult;

            // Assert
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok.Value, Is.SameAs(methods));
        }
    }
}
