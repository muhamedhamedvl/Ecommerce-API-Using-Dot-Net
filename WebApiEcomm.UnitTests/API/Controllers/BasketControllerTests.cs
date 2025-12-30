using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using WebApiEcomm.API.Controllers;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Basket;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Interfaces;

namespace WebApiEcomm.UnitTests.API.Controllers
{
    [TestFixture]
    public class basketControllerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<ICustomerBasketRepository> _mockBasketRepo;
        private BasketController _controller;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBasketRepo = new Mock<ICustomerBasketRepository>();
            _mockUnitOfWork.Setup(u => u.CustomerBasketRepository).Returns(_mockBasketRepo.Object);

            var mockMapper = new Mock<IMapper>();
            _controller = new BasketController(_mockUnitOfWork.Object, mockMapper.Object);
        }

        [Test]
        public async Task GetBasket_WhenBasketExists_ReturnsOkWithBasket()
        {
            // Arrange
            var basketId = "test123";
            var basket = new CustomerBasket(basketId);
            _mockBasketRepo.Setup(r => r.GetCustomerBasketAsync(basketId)).ReturnsAsync(basket);

            // Act
            var result = await _controller.GetBasket(basketId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.EqualTo(basket));
        }

        [Test]
        public async Task GetBasket_WhenBasketNotFound_ReturnsOkWithNewBasket()
        {
            // Arrange
            var basketId = "notfound";
            _mockBasketRepo.Setup(r => r.GetCustomerBasketAsync(basketId)).ReturnsAsync((CustomerBasket)null);

            // Act
            var result = await _controller.GetBasket(basketId) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.TypeOf<CustomerBasket>());
        }

        [Test]
        public async Task Update_WhenCalled_ReturnsOkWithUpdatedBasket()
        {
            // Arrange
            var basket = new CustomerBasket("test123");
            _mockBasketRepo.Setup(r => r.UpdateCustomerBasketAsync(basket)).ReturnsAsync(basket);

            // Act
            var result = await _controller.Update(basket) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.EqualTo(basket));
        }

        [Test]
        public async Task DeleteById_WhenBasketDeleted_ReturnsNoContent()
        {
            // Arrange
            var basketId = "test123";
            _mockBasketRepo.Setup(r => r.DeleteCustomerBasketAsync(basketId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteById(basketId) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(204));
        }

        [Test]
        public async Task DeleteById_WhenBasketNotDeleted_ReturnsBadRequest()
        {
            // Arrange
            var basketId = "notfound";
            _mockBasketRepo.Setup(r => r.DeleteCustomerBasketAsync(basketId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteById(basketId) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var response = result.Value as ResponseApi;
            Assert.That(response.StatusCode, Is.EqualTo(400));
        }
    }
}
